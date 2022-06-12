using Server.Engines.Craft;
using Server.Engines.XmlSpawner2;
using Server.Factions;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Items
{
	public abstract class BaseWeapon : BaseEquipment, IWeapon, IFactionItem, ICraftable, ISlayer, IDurability, ISetItem, IResource, IArtifact, IQuality
	{
		#region Var declarations
		private WeaponDamageLevel m_DamageLevel;
		private WeaponAccuracyLevel m_AccuracyLevel;
		private DurabilityLevel m_DurabilityLevel;
		private Poison m_Poison;
		private int m_PoisonCharges;
		private int m_Hits;
		private int m_MaxHits;
		private SlayerName m_Slayer;
		private SlayerName m_Slayer2;
		private SkillMod m_SkillMod, m_MageMod;
		private AosWeaponAttributes m_AosWeaponAttributes;
		private AosSkillBonuses m_AosSkillBonuses;
		private AosElementAttributes m_AosElementDamages;
		private TalismanSlayerName m_Slayer3;
		private int m_StrReq, m_DexReq, m_IntReq;
		private int m_MinDamage, m_MaxDamage;
		private int m_HitSound, m_MissSound;
		private float m_Speed;
		private int m_MaxRange;
		private SkillName m_Skill;
		private WeaponType m_Type;
		private WeaponAnimation m_Animation;
		private WeaponEffect m_WeaponEffect;
		private int m_Charges;
		#endregion

		#region Virtual Properties
		public virtual WeaponAbility PrimaryAbility => null;
		public virtual WeaponAbility SecondaryAbility => null;
		public virtual int DefMaxRange => 1;
		public virtual int DefHitSound => 0;
		public virtual int DefMissSound => 0;
		public virtual SkillName DefSkill => SkillName.Swords;
		public virtual WeaponType DefType => WeaponType.Slashing;
		public virtual WeaponAnimation DefAnimation => WeaponAnimation.Slash1H;
		public virtual int StrReq => 0;
		public virtual int DexReq => 0;
		public virtual int IntReq => 0;
		public virtual int MinDamageBase => 0;
		public virtual int MaxDamageBase => 0;
		public virtual float SpeedBase => 0;
		public virtual int InitMinHits => 0;
		public virtual int InitMaxHits => 0;
		public override int PhysicalResistance => m_AosWeaponAttributes.ResistPhysicalBonus;
		public override int FireResistance => m_AosWeaponAttributes.ResistFireBonus;
		public override int ColdResistance => m_AosWeaponAttributes.ResistColdBonus;
		public override int PoisonResistance => m_AosWeaponAttributes.ResistPoisonBonus;
		public override int EnergyResistance => m_AosWeaponAttributes.ResistEnergyBonus;
		public virtual SkillName AccuracySkill => SkillName.Tactics;
		#endregion

		#region Getters & Setters
		[CommandProperty(AccessLevel.GameMaster)]
		public AosWeaponAttributes WeaponAttributes
		{
			get => m_AosWeaponAttributes;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get => m_AosSkillBonuses;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosElementAttributes AosElementDamages
		{
			get => m_AosElementDamages;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Cursed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public ConsecratedWeaponContext ConsecratedContext { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get => m_Hits;
			set
			{
				if (m_Hits == value)
					return;

				if (value > m_MaxHits)
					value = m_MaxHits;

				m_Hits = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get => m_MaxHits;
			set { m_MaxHits = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PoisonCharges
		{
			get => m_PoisonCharges;
			set { m_PoisonCharges = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Poison Poison
		{
			get => m_Poison;
			set { m_Poison = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override ItemQuality Quality
		{
			get => base.Quality;
			set
			{
				UnscaleDurability();
				base.Quality = value;
				ScaleDurability();
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer
		{
			get => m_Slayer;
			set { m_Slayer = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer2
		{
			get => m_Slayer2;
			set { m_Slayer2 = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TalismanSlayerName Slayer3
		{
			get => m_Slayer3;
			set
			{
				m_Slayer3 = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override CraftResource Resource
		{
			get => base.Resource;
			set { UnscaleDurability(); base.Resource = value; Hue = CraftResources.GetHue(Resource); InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponDamageLevel DamageLevel
		{
			get => m_DamageLevel;
			set { m_DamageLevel = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DurabilityLevel DurabilityLevel
		{
			get => m_DurabilityLevel;
			set { UnscaleDurability(); m_DurabilityLevel = value; InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxRange
		{
			get => m_MaxRange == -1 ? DefMaxRange : m_MaxRange;
			set { m_MaxRange = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponAnimation Animation
		{
			get => m_Animation == (WeaponAnimation)(-1) ? DefAnimation : m_Animation;
			set => m_Animation = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponType Type
		{
			get => m_Type == (WeaponType)(-1) ? DefType : m_Type;
			set => m_Type = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName Skill
		{
			get => m_Skill == (SkillName)(-1) ? DefSkill : m_Skill;
			set { m_Skill = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitSound
		{
			get => m_HitSound == -1 ? DefHitSound : m_HitSound;
			set => m_HitSound = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MissSound
		{
			get => m_MissSound == -1 ? DefMissSound : m_MissSound;
			set => m_MissSound = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MinDamage
		{
			get => m_MinDamage == -1 ? MinDamageBase : m_MinDamage;
			set { m_MinDamage = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxDamage
		{
			get => m_MaxDamage == -1 ? MaxDamageBase : m_MaxDamage;
			set { m_MaxDamage = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public float Speed
		{
			get => SpeedBase;
			set { m_Speed = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get => m_StrReq == -1 ? StrReq : m_StrReq;
			set { m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get => m_DexReq == -1 ? DexReq : m_DexReq;
			set => m_DexReq = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get => m_IntReq == -1 ? IntReq : m_IntReq;
			set => m_IntReq = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponAccuracyLevel AccuracyLevel
		{
			get => m_AccuracyLevel;
			set
			{
				if (m_AccuracyLevel != value)
				{
					m_AccuracyLevel = value;

					if (UseSkillMod)
					{
						if (m_AccuracyLevel == WeaponAccuracyLevel.Regular)
						{
							if (m_SkillMod != null)
								m_SkillMod.Remove();

							m_SkillMod = null;
						}
						else if (m_SkillMod == null && Parent is Mobile mob)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							mob.AddSkillMod(m_SkillMod);
						}
						else if (m_SkillMod != null)
						{
							m_SkillMod.Value = (int)m_AccuracyLevel * 5;
						}
					}

					InvalidateProperties();
				}
			}
		}

		public Mobile FocusWeilder { get; set; }
		public Mobile EnchantedWeilder { get; set; }
		public int LastParryChance { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponEffect Effect
		{
			get { return m_WeaponEffect; }
			set { m_WeaponEffect = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Charges
		{
			get { return m_Charges; }
			set { m_Charges = value; InvalidateProperties(); }
		}
		#endregion

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem != null && newItem is BaseWeapon weapon)
			{
				weapon.m_AosElementDamages = new AosElementAttributes(weapon, m_AosElementDamages);
				weapon.m_AosSkillBonuses = new AosSkillBonuses(weapon, m_AosSkillBonuses);
				weapon.m_AosWeaponAttributes = new AosWeaponAttributes(weapon, m_AosWeaponAttributes);
			}
		}

		public virtual void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
			m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;
			InvalidateProperties();
		}

		public virtual void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = ((m_Hits * scale) + 99) / 100;
			m_MaxHits = ((m_MaxHits * scale) + 99) / 100;
			InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if (Quality == ItemQuality.Exceptional)
				bonus += 20;

			switch (m_DurabilityLevel)
			{
				case DurabilityLevel.Durable: bonus += 20; break;
				case DurabilityLevel.Substantial: bonus += 50; break;
				case DurabilityLevel.Massive: bonus += 70; break;
				case DurabilityLevel.Fortified: bonus += 100; break;
				case DurabilityLevel.Indestructible: bonus += 120; break;
			}

			if (Core.AOS)
			{
				bonus += m_AosWeaponAttributes.DurabilityBonus;

				if (Resource == CraftResource.Heartwood)
					return bonus;

				CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);
				CraftAttributeInfo attrInfo = null;

				if (resInfo != null)
					attrInfo = resInfo.AttributeInfo;

				if (attrInfo != null)
					bonus += attrInfo.WeaponDurability;
			}

			return bonus;
		}

		public override int GetLowerStatReq()
		{
			if (!Core.AOS)
				return 0;

			int v = m_AosWeaponAttributes.LowerStatReq;

			if (Resource == CraftResource.Heartwood)
				return v;

			CraftResourceInfo info = CraftResources.GetInfo(Resource);

			if (info != null)
			{
				CraftAttributeInfo attrInfo = info.AttributeInfo;

				if (attrInfo != null)
					v += attrInfo.WeaponLowerRequirements;
			}

			if (v > 100)
				v = 100;

			return v;
		}

		public static void BlockEquip(Mobile m, TimeSpan duration)
		{
			if (m.BeginAction(typeof(BaseWeapon)))
				new ResetEquipTimer(m, duration).Start();
		}

		private class ResetEquipTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public ResetEquipTimer(Mobile m, TimeSpan duration) : base(duration)
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction(typeof(BaseWeapon));
			}
		}

		public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
		{
			if (base.CheckConflictingLayer(m, item, layer))
				return true;

			if (Layer == Layer.TwoHanded && layer == Layer.OneHanded)
			{
				m.SendLocalizedMessage(500214); // You already have something in both hands.
				return true;
			}
			else if (Layer == Layer.OneHanded && layer == Layer.TwoHanded && item is not BaseShield && item is not BaseEquipableLight)
			{
				m.SendLocalizedMessage(500215); // You can only wield one weapon at a time.
				return true;
			}

			return false;
		}

		//public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		//{
		//	if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
		//		return false;

		//	return base.AllowSecureTrade(from, to, newOwner, accepted);
		//}

		public override bool CanEquip(Mobile from)
		{
			if (!Ethics.Ethic.CheckEquip(from, this))
				return false;

			if (from.IsPlayer() && this is IAccountRestricted restricted && restricted.Account != null)
			{
				if (from.Account is not Accounting.Account acct || acct.Username != restricted.Account)
				{
					from.SendLocalizedMessage(1071296); // This item is Account Bound and your character is not bound to it. You cannot use this item.
					return false;
				}
			}

			if (!RaceDefinitions.ValidateEquipment(from, this))
			{
				return false;
			}
			else if (from.Dex < DexRequirement)
			{
				from.SendMessage("You are not nimble enough to equip that.");
				return false;
			}
			else if (from.Str < AOS.Scale(StrRequirement, 100 - GetLowerStatReq()))
			{
				from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
				return false;
			}
			else if (from.Int < IntRequirement)
			{
				from.SendMessage("You are not smart enough to equip that.");
				return false;
			}
			else if (!from.CanBeginAction(typeof(BaseWeapon)))
			{
				return false;
			}
			else if (!XmlAttach.CheckCanEquip(this, from))
			{
				return false;
			}
			else
			{
				return base.CanEquip(from);
			}
		}

		public virtual bool UseSkillMod => !Core.AOS;

		public override bool OnEquip(Mobile from)
		{
			int strBonus = Attributes.BonusStr;
			int dexBonus = Attributes.BonusDex;
			int intBonus = Attributes.BonusInt;

			if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
			{
				Mobile m = from;

				string modName = Serial.ToString();

				if (strBonus != 0)
					m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

				if (dexBonus != 0)
					m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

				if (intBonus != 0)
					m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			}

			from.NextCombatTime = Core.TickCount + (int)GetDelay(from).TotalMilliseconds;

			if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
			{
				if (m_SkillMod != null)
					m_SkillMod.Remove();

				m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
				from.AddSkillMod(m_SkillMod);
			}

			if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30)
			{
				if (m_MageMod != null)
					m_MageMod.Remove();

				m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
				from.AddSkillMod(m_MageMod);
			}

			XmlAttach.CheckOnEquip(this, from);
			InDoubleStrike = false;
			return true;
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (parent is Mobile from)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(from);

				if (IsSetItem)
				{
					m_SetEquipped = SetHelper.FullSetEquipped(from, SetID, Pieces);

					if (m_SetEquipped)
					{
						m_LastEquipped = true;
						SetHelper.AddSetBonus(from, SetID);
					}
				}

				from.CheckStatTimers();
				from.Delta(MobileDelta.WeaponDamage);
			}
		}

		public override void OnRemoved(IEntity parent)
		{
			if (parent is Mobile m)
			{
				RemoveStatBonuses(m);

				if (m.Weapon is BaseWeapon weapon)
					m.NextCombatTime = Core.TickCount + (int)weapon.GetDelay(m).TotalMilliseconds;

				if (UseSkillMod && m_SkillMod != null)
				{
					m_SkillMod.Remove();
					m_SkillMod = null;
				}

				if (m_MageMod != null)
				{
					m_MageMod.Remove();
					m_MageMod = null;
				}

				if (Core.AOS)
				{
					m_AosSkillBonuses.Remove();
				}

				ImmolatingWeaponSpell.StopImmolating(this, (Mobile)parent);

				if (FocusWeilder != null)
				{
					FocusWeilder = null;
				}

				if (Core.ML && IsSetItem && m_SetEquipped)
				{
					SetHelper.RemoveSetBonus(m, SetID, this);
				}

				m.CheckStatTimers();
				m.Delta(MobileDelta.WeaponDamage);
				XmlAttach.CheckOnRemoved(this, parent);
			}
		}

		public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
		{
			SkillName sk;

			if (checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0)
			{
				double swrd = m.Skills[SkillName.Swords].Value;
				double fenc = m.Skills[SkillName.Fencing].Value;
				double mcng = m.Skills[SkillName.Macing].Value;
				double val = swrd;

				sk = SkillName.Swords;

				if (fenc > val)
				{
					sk = SkillName.Fencing;
					val = fenc;
				}

				if (mcng > val)
				{
					sk = SkillName.Macing;
					val = mcng;
				}
			}
			else if (m_AosWeaponAttributes.MageWeapon != 0)
			{
				if (m.Skills[SkillName.Magery].Value > m.Skills[Skill].Value)
					sk = SkillName.Magery;
				else
					sk = Skill;
			}
			else
			{
				sk = Skill;

				if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
					sk = SkillName.Wrestling;
			}

			return sk;
		}

		public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender) => attacker.Skills[GetUsedSkill(attacker, true)].Value;

		public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender) => defender.Skills[GetUsedSkill(defender, true)].Value;

		private static bool CheckAnimal(Mobile m, Type type) => AnimalForm.UnderTransformation(m, type);

		public virtual bool CheckHit(Mobile attacker, IDamageable damageable)
		{
			if (damageable is not Mobile defender)
			{
				//if (damageable is IDamageableItem)
				//{
				//	return ((IDamageableItem)damageable).CheckHit(attacker);
				//}

				return true;
			}

			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			_ = defender.Skills[defWeapon.Skill];

			double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
			double defValue = defWeapon.GetDefendSkillValue(attacker, defender);

			double ourValue, theirValue;

			int bonus = GetHitChanceBonus();

			if (Core.AOS)
			{
				if (atkValue <= -20.0)
				{
					atkValue = -19.9;
				}

				if (defValue <= -20.0)
				{
					defValue = -19.9;
				}

				bonus += AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

				ourValue = (atkValue + 20.0) * (100 + bonus);

				bonus = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

				ForceArrow.ForceArrowInfo info = ForceArrow.GetInfo(attacker, defender);

				if (info != null && info.Defender == defender)
				{
					bonus -= info.DefenseChanceMalus;
				}

				int max = 45;

				// Defense Chance Increase = 45%
				if (bonus > max)
				{
					bonus = max;
				}

				theirValue = (defValue + 20.0) * (100 + bonus);

				bonus = 0;
			}
			else
			{
				if (atkValue <= -50.0)
				{
					atkValue = -49.9;
				}

				if (defValue <= -50.0)
				{
					defValue = -49.9;
				}

				ourValue = atkValue + 50.0;
				theirValue = defValue + 50.0;
			}

			double chance = ourValue / (theirValue * 2.0);

			chance *= 1.0 + ((double)bonus / 100);

			if (Core.AOS && chance < 0.02)
			{
				chance = 0.02;
			}

			if (Core.AOS && m_AosWeaponAttributes.MageWeapon > 0 && attacker.Skills[SkillName.Magery].Value > atkSkill.Value)
			{
				return attacker.CheckSkill(SkillName.Magery, chance);
			}

			EventSink.InvokeOnMobileCheckHit(attacker, defender);

			return attacker.CheckSkill(atkSkill.SkillName, chance);

		}

		public virtual TimeSpan GetDelay(Mobile m)
		{
			double speed = Speed;

			if (speed == 0)
				return TimeSpan.FromHours(1.0);

			double delayInSeconds;

			//Get the attack speed bonus from the mobile
			int bonus = m.GetAttackSpeedBonus();
			if (Core.SE)
			{
				double ticks;

				if (Core.ML)
				{
					int stamTicks = m.Stam / 30;

					ticks = speed * 4;
					ticks = Math.Floor((ticks - stamTicks) * (100.0 / (100 + bonus)));
				}
				else
				{
					speed = Math.Floor(speed * (bonus + 100.0) / 100.0);

					if (speed <= 0)
						speed = 1;

					ticks = Math.Floor((80000.0 / ((m.Stam + 100) * speed)) - 2);
				}

				// Swing speed currently capped at one swing every 1.25 seconds (5 ticks).
				if (ticks < 5)
					ticks = 5;

				delayInSeconds = ticks * 0.25;
			}
			else if (Core.AOS)
			{
				int v = (m.Stam + 100) * (int)speed;

				v += AOS.Scale(v, bonus);

				if (v <= 0)
					v = 1;

				delayInSeconds = Math.Floor(40000.0 / v) * 0.5;

				// Maximum swing rate capped at one swing per second
				// OSI dev said that it has and is supposed to be 1.25
				if (delayInSeconds < 1.25)
					delayInSeconds = 1.25;
			}
			else
			{
				int v = (m.Stam + 100) * (int)speed;

				if (v <= 0)
					v = 1;

				delayInSeconds = 15000.0 / v;
			}

			return TimeSpan.FromSeconds(delayInSeconds);
		}

		public virtual void OnBeforeSwing(Mobile attacker, IDamageable damageable)
		{
			Mobile defender = damageable as Mobile;

			WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

			if (a != null && (!a.OnBeforeSwing(attacker, defender)))
			{
				WeaponAbility.ClearCurrentAbility(attacker);
			}

			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

			if (move != null && !move.OnBeforeSwing(attacker, defender))
			{
				SpecialMove.ClearCurrentMove(attacker);
			}
		}

		public virtual TimeSpan OnSwing(Mobile attacker, IDamageable damageable)
		{
			return OnSwing(attacker, damageable, 1.0);
		}

		public virtual TimeSpan OnSwing(Mobile attacker, IDamageable damageable, double damageBonus)
		{
			bool canSwing = true;

			if (Core.AOS)
			{
				canSwing = (!attacker.Paralyzed && !attacker.Frozen);

				if (canSwing)
				{
					canSwing = (attacker.Spell is not Spell sp || !sp.IsCasting || !sp.BlocksMovement);
				}

				if (canSwing)
				{
					canSwing = (attacker is not PlayerMobile p || p.PeacedUntil <= DateTime.UtcNow);
				}
			}

			#region Dueling
			if (attacker is PlayerMobile pm)
			{
				if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
					canSwing = false;
			}
			#endregion

			if (canSwing && attacker.HarmfulCheck(damageable))
			{
				attacker.DisruptiveAction();

				if (attacker.NetState != null)
					_ = attacker.Send(new Swing(0, attacker, damageable));

				if (attacker is BaseCreature bc)
				{
					WeaponAbility ab = bc.GetWeaponAbility();

					if (ab != null)
					{
						if (bc.WeaponAbilityChance > Utility.RandomDouble())
							_ = WeaponAbility.SetCurrentAbility(bc, ab);
						else
							WeaponAbility.ClearCurrentAbility(bc);
					}
				}

				if (CheckHit(attacker, damageable))
					OnHit(attacker, damageable, damageBonus);
				else
					OnMiss(attacker, damageable);
			}

			return GetDelay(attacker);
		}

		#region Sounds
		public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
		{
			int sound = attacker.GetAttackSound();

			if (sound == -1)
				sound = HitSound;

			return sound;
		}

		public virtual int GetHitDefendSound(Mobile attacker, Mobile defender) => defender.GetHurtSound();

		public virtual int GetMissAttackSound(Mobile attacker, Mobile defender) => attacker.GetAttackSound() == -1 ? MissSound : -1;

		public virtual int GetMissDefendSound(Mobile attacker, Mobile defender) => -1;
		#endregion

		public static bool CheckParry(Mobile defender)
		{
			if (defender == null)
				return false;

			double parry = defender.Skills[SkillName.Parry].Value;
			double bushidoNonRacial = defender.Skills[SkillName.Bushido].NonRacialValue;
			double bushido = defender.Skills[SkillName.Bushido].Value;

			if (defender.FindItemOnLayer(Layer.TwoHanded) is BaseShield)
			{
				double chance = (parry - bushidoNonRacial) / 400.0; // As per OSI, no negitive effect from the Racial stuffs, ie, 120 parry and '0' bushido with humans

				if (chance < 0) // chance shouldn't go below 0
					chance = 0;

				// Parry/Bushido over 100 grants a 5% bonus.
				if (parry >= 100.0 || bushido >= 100.0)
					chance += 0.05;

				// Evasion grants a variable bonus post ML. 50% prior.
				if (Evasion.IsEvading(defender))
					chance *= Evasion.GetParryScalar(defender);

				// Low dexterity lowers the chance.
				if (defender.Dex < 80)
					chance = chance * (20 + defender.Dex) / 100;

				return defender.CheckSkill(SkillName.Parry, chance);
			}
			else if (!(defender.Weapon is Fists) && defender.Weapon is not BaseRanged)
			{
				BaseWeapon weapon = defender.Weapon as BaseWeapon;

				double divisor = (weapon.Layer == Layer.OneHanded) ? 48000.0 : 41140.0;

				double chance = parry * bushido / divisor;

				double aosChance = parry / 800.0;

				// Parry or Bushido over 100 grant a 5% bonus.
				if (parry >= 100.0)
				{
					chance += 0.05;
					aosChance += 0.05;
				}
				else if (bushido >= 100.0)
				{
					chance += 0.05;
				}

				// Evasion grants a variable bonus post ML. 50% prior.
				if (Evasion.IsEvading(defender))
					chance *= Evasion.GetParryScalar(defender);

				// Low dexterity lowers the chance.
				if (defender.Dex < 80)
					chance = chance * (20 + defender.Dex) / 100;

				if (chance > aosChance)
					return defender.CheckSkill(SkillName.Parry, chance);
				else
					return aosChance > Utility.RandomDouble(); // Only skillcheck if wielding a shield & there's no effect from Bushido
			}

			return false;
		}

		public virtual int AbsorbDamageAOS(Mobile attacker, Mobile defender, int damage)
		{
			bool blocked = false;
			int originaldamage = damage;

			if (defender.Player || defender.Body.IsHuman)
			{
				blocked = CheckParry(defender);

				if (blocked)
				{
					defender.FixedEffect(0x37B9, 10, 16);
					damage = 0;

					// Successful block removes the Honorable Execution penalty.
					HonorableExecution.RemovePenalty(defender);

					if (CounterAttack.IsCountering(defender))
					{
						if (defender.Weapon is BaseWeapon weapon)
						{
							defender.FixedParticles(0x3779, 1, 15, 0x158B, 0x0, 0x3, EffectLayer.Waist);
							_ = weapon.OnSwing(defender, attacker);
						}

						CounterAttack.StopCountering(defender);
					}

					if (Confidence.IsConfident(defender))
					{
						defender.SendLocalizedMessage(1063117); // Your confidence reassures you as you successfully block your opponent's blow.

						double bushido = defender.Skills.Bushido.Value;

						defender.Hits += Utility.RandomMinMax(1, (int)(bushido / 12));
						defender.Stam += Utility.RandomMinMax(1, (int)(bushido / 5));
					}

					if (defender.FindItemOnLayer(Layer.TwoHanded) is BaseShield shield)
					{
						_ = shield.OnHit(this, damage);
						XmlAttach.OnArmorHit(attacker, defender, shield, this, originaldamage);
					}
				}
			}

			if (!blocked)
			{
				double positionChance = Utility.RandomDouble();

				Item armorItem;

				if (positionChance < 0.07)
					armorItem = defender.NeckArmor;
				else if (positionChance < 0.14)
					armorItem = defender.HandArmor;
				else if (positionChance < 0.28)
					armorItem = defender.ArmsArmor;
				else if (positionChance < 0.43)
					armorItem = defender.HeadArmor;
				else if (positionChance < 0.65)
					armorItem = defender.LegsArmor;
				else
					armorItem = defender.ChestArmor;

				if (armorItem is IWearableDurability armor)
				{
					_ = armor.OnHit(this, damage); // call OnHit to lose durability
					XmlAttach.OnArmorHit(attacker, defender, armorItem, this, originaldamage);
				}
			}

			return damage;
		}

		public virtual int AbsorbDamage(Mobile attacker, Mobile defender, int damage)
		{
			if (Core.AOS)
				return AbsorbDamageAOS(attacker, defender, damage);

			BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
			if (shield != null)
			{
				damage = shield.OnHit(this, damage);
			}

			double chance = Utility.RandomDouble();

			Item armorItem;

			if (chance < 0.07)
				armorItem = defender.NeckArmor;
			else if (chance < 0.14)
				armorItem = defender.HandArmor;
			else if (chance < 0.28)
				armorItem = defender.ArmsArmor;
			else if (chance < 0.43)
				armorItem = defender.HeadArmor;
			else if (chance < 0.65)
				armorItem = defender.LegsArmor;
			else
				armorItem = defender.ChestArmor;

			if (armorItem is IWearableDurability armor)
				damage = armor.OnHit(this, damage);

			damage -= XmlAttach.OnArmorHit(attacker, defender, armorItem, this, damage);
			damage -= XmlAttach.OnArmorHit(attacker, defender, shield, this, damage);

			int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

			if (virtualArmor > 0)
			{
				double scalar;

				if (chance < 0.14)
					scalar = 0.07;
				else if (chance < 0.28)
					scalar = 0.14;
				else if (chance < 0.43)
					scalar = 0.15;
				else if (chance < 0.65)
					scalar = 0.22;
				else
					scalar = 0.35;

				int from = (int)(virtualArmor * scalar) / 2;
				int to = (int)(virtualArmor * scalar);

				damage -= Utility.Random(from, (to - from) + 1);
			}

			return damage;
		}

		public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
		{
			if (attacker.Player || defender.Player)
				return 0;

			if (attacker is not BaseCreature bc || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned))
				return 0;

			Mobile master = bc.ControlMaster;

			if (master == null)
				master = bc.SummonMaster;

			if (master == null)
				return 0;

			int inPack = 1;

			IPooledEnumerable eable = defender.GetMobilesInRange(1);
			foreach (Mobile m in eable)
			{
				if (m != attacker && m is BaseCreature tc)
				{
					if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned))
						continue;

					Mobile theirMaster = tc.ControlMaster;

					if (theirMaster == null)
						theirMaster = tc.SummonMaster;

					if (master == theirMaster && tc.Combatant == defender)
						++inPack;
				}
			}
			eable.Free();

			if (inPack >= 5)
				return 100;
			else if (inPack >= 4)
				return 75;
			else if (inPack >= 3)
				return 50;
			else if (inPack >= 2)
				return 25;

			return 0;
		}

		private bool m_InDoubleStrike;
		private bool m_ProcessingMultipleHits;

		public bool InDoubleStrike
		{
			get => m_InDoubleStrike;
			set
			{
				m_InDoubleStrike = value;

				if (m_InDoubleStrike)
				{
					ProcessingMultipleHits = true;
				}
				else
				{
					ProcessingMultipleHits = false;
				}
			}
		}

		public bool ProcessingMultipleHits
		{
			get => m_ProcessingMultipleHits;
			set
			{
				m_ProcessingMultipleHits = value;

				if (!m_ProcessingMultipleHits)
				{
					BlockHitEffects = false;
				}
			}
		}

		public bool EndDualWield { get; set; }
		public bool BlockHitEffects { get; set; }
		public DateTime NextSelfRepair { get; set; }

		public void ConsumeCharge(Mobile from)
		{
			--Charges;

			if (Charges == 0)
				from.SendAsciiMessage("This item is out of charges."); // This item is out of charges.
		}

		public void Cast(Spell spell)
		{
			bool m = Movable;
			Movable = false;
			spell.Cast();
			Movable = m;
		}

		public void OnHit(Mobile attacker, IDamageable damageable)
		{
			OnHit(attacker, damageable, 1.0);
		}

		public virtual void OnHit(Mobile attacker, IDamageable damageable, double damageBonus)
		{
			if (Core.AOS)
			{
				if (EndDualWield)
				{
					ProcessingMultipleHits = false;
					EndDualWield = false;
				}

				Mobile defender = damageable as Mobile;
				Clone clone = null;

				if (defender != null)
				{
					clone = MirrorImage.GetDeflect(attacker, defender);
				}

				if (clone != null)
				{
					defender = clone;
				}

				PlaySwingAnimation(attacker);

				if (defender != null)
				{
					PlayHurtAnimation(defender);
				}

				attacker.PlaySound(GetHitAttackSound(attacker, defender));

				if (defender != null)
				{
					defender.PlaySound(GetHitDefendSound(attacker, defender));
				}

				int damage = ComputeDamage(attacker, defender);

				WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
				SpecialMove move = SpecialMove.GetCurrentMove(attacker);

				bool ranged = this is BaseRanged;

				GetDamageTypes(attacker, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct);

				if (ConsecratedContext != null &&
					ConsecratedContext.Owner == attacker &&
					ConsecratedContext.ConsecrateProcChance >= Utility.Random(100))
				{
					phys = damageable.PhysicalResistance;
					fire = damageable.FireResistance;
					cold = damageable.ColdResistance;
					pois = damageable.PoisonResistance;
					nrgy = damageable.EnergyResistance;

					int low = phys, type = 0;

					if (fire < low) { low = fire; type = 1; }
					if (cold < low) { low = cold; type = 2; }
					if (pois < low) { low = pois; type = 3; }
					if (nrgy < low) { low = nrgy; type = 4; }

					phys = fire = cold = pois = nrgy = chaos = direct = 0;

					switch (type)
					{
						case 0:
							phys = 100;
							break;
						case 1:
							fire = 100;
							break;
						case 2:
							cold = 100;
							break;
						case 3:
							pois = 100;
							break;
						case 4:
							nrgy = 100;
							break;
					}
				}
				else if (Core.ML && ranged)
				{
					if (attacker.FindItemOnLayer(Layer.Cloak) is IRangeDamage rangeDamage)
					{
						rangeDamage.AlterRangedDamage(ref phys, ref fire, ref cold, ref pois, ref nrgy, ref chaos, ref direct);
					}
				}

				bool acidicTarget = MaxRange <= 1 && Attributes.SpellChanneling == 0 && !(this is Fists) && (defender is Slime || defender is CorrosiveSlime);

				if (acidicTarget || (defender != null))
				{
					if (MaxRange <= 1 && acidicTarget)
					{
						attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500263); // *Acid blood scars your weapon!*
					}

					int selfRepair = !Core.AOS ? 0 : m_AosWeaponAttributes.SelfRepair + (IsSetItem && m_SetEquipped ? m_SetSelfRepair : 0);

					if (selfRepair > 0 && NextSelfRepair < DateTime.UtcNow)
					{
						HitPoints += selfRepair;

						NextSelfRepair = DateTime.UtcNow + TimeSpan.FromSeconds(60);
					}
					else
					{
						if (m_MaxHits > 0)
						{
							if (m_Hits >= 1)
							{
								HitPoints--;
							}
							else if (m_MaxHits > 0)
							{
								MaxHitPoints--;

								if (Parent is Mobile)
								{
									((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
								}

								if (m_MaxHits <= 0)
								{
									Delete();
								}
							}
						}
					}
				}

				bool bladeweaving = Bladeweave.BladeWeaving(attacker, out WeaponAbility weavabil);
				bool ignoreArmor = a is ArmorIgnore || (move != null && move.IgnoreArmor(attacker)) || (bladeweaving && weavabil is ArmorIgnore);

				// object is not a mobile, so we end here
				if (defender == null)
				{
					AOS.Damage(damageable, attacker, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, false, ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

					// TODO: WeaponAbility/SpecialMove OnHit(...) convert target to IDamageable
					// Figure out which specials work on items. For now AI only.
					if (ignoreArmor)
					{
						Effects.PlaySound(damageable.Location, damageable.Map, 0x56);
						Effects.SendTargetParticles(damageable, 0x3728, 200, 25, 0, 0, 9942, EffectLayer.Waist, 0);
					}

					WeaponAbility.ClearCurrentAbility(attacker);
					SpecialMove.ClearCurrentMove(attacker);

					if (WeaponAttributes.HitLeechHits > 0)
					{
						attacker.SendLocalizedMessage(1152566); // You fail to leech life from your target!
					}

					return;
				}

				#region Damage Multipliers
				int percentageBonus = 0;

				if (a != null)
				{
					percentageBonus += (int)(a.DamageScalar * 100) - 100;
				}

				if (move != null)
				{
					percentageBonus += (int)(move.GetDamageScalar(attacker, defender) * 100) - 100;
				}

				if (ConsecratedContext != null && ConsecratedContext.Owner == attacker)
				{
					percentageBonus += ConsecratedContext.ConsecrateDamageBonus;
				}

				percentageBonus += (int)(damageBonus * 100) - 100;

				CheckSlayerResult cs1 = CheckSlayers(attacker, defender, Slayer);
				CheckSlayerResult cs2 = CheckSlayers(attacker, defender, Slayer2);
				CheckSlayerResult suit = CheckSlayers(attacker, defender, SetHelper.GetSetSlayer(attacker));
				CheckSlayerResult tal = CheckTalismanSlayer(attacker, defender);

				if (cs1 != CheckSlayerResult.None)
				{
					if (cs1 == CheckSlayerResult.SuperSlayer)
					{
						percentageBonus += 100;
					}
					else if (cs1 == CheckSlayerResult.Slayer)
					{
						percentageBonus += 200;
					}
				}

				if (cs2 != CheckSlayerResult.None)
				{
					if (cs2 == CheckSlayerResult.SuperSlayer)
					{
						percentageBonus += 100;
					}
					else if (cs2 == CheckSlayerResult.Slayer)
					{
						percentageBonus += 200;
					}
				}

				if (suit != CheckSlayerResult.None)
				{
					percentageBonus += 100;
				}

				if (tal != CheckSlayerResult.None)
				{
					percentageBonus += 100;
				}

				if (CheckSlayerOpposition(attacker, defender) != CheckSlayerResult.None)
				{
					percentageBonus += 100;
					defender.FixedEffect(0x37B9, 10, 5);
				}
				else if (cs1 != CheckSlayerResult.None || cs2 != CheckSlayerResult.None || suit != CheckSlayerResult.None || tal != CheckSlayerResult.None)
				{
					defender.FixedEffect(0x37B9, 10, 5);
				}

				#region Enemy of One
				var enemyOfOneContext = EnemyOfOneSpell.GetContext(defender);

				if (enemyOfOneContext != null && !enemyOfOneContext.IsWaitingForEnemy && !enemyOfOneContext.IsEnemy(attacker))
				{
					percentageBonus += 100;
				}
				else
				{
					enemyOfOneContext = EnemyOfOneSpell.GetContext(attacker);

					if (enemyOfOneContext != null)
					{
						enemyOfOneContext.OnHit(defender);

						if (enemyOfOneContext.IsEnemy(defender))
						{
							defender.FixedEffect(0x37B9, 10, 5, 1160, 0);
							percentageBonus += enemyOfOneContext.DamageScalar;
						}
					}
				}
				#endregion

				int packInstinctBonus = GetPackInstinctBonus(attacker, defender);

				if (packInstinctBonus != 0)
				{
					percentageBonus += packInstinctBonus;
				}

				TransformContext context = TransformationSpellHelper.GetContext(defender);

				if ((m_Slayer == SlayerName.Silver || m_Slayer2 == SlayerName.Silver || SetHelper.GetSetSlayer(attacker) == SlayerName.Silver)
					&& ((context != null && context.Spell is NecromancerSpell && context.Type != typeof(HorrificBeastSpell))
					|| (defender is BaseCreature && (defender.Body == 747 || defender.Body == 748 || defender.Body == 749 || defender.Hue == 0x847E))))
				{
					// Every necromancer transformation other than horrific beast takes an additional 25% damage
					percentageBonus += 25;
				}

				if (attacker is PlayerMobile mobile && !(Core.ML && defender is not PlayerMobile))
				{
					PlayerMobile pmAttacker = mobile;

					if (pmAttacker.HonorActive && pmAttacker.InRange(defender, 1))
					{
						percentageBonus += 25;
					}

					if (pmAttacker.SentHonorContext != null && pmAttacker.SentHonorContext.Target == defender)
					{
						percentageBonus += pmAttacker.SentHonorContext.PerfectionDamageBonus;
					}
				}

				percentageBonus -= Block.GetMeleeReduction(defender);

				#region Mondain's Legacy
				if (Core.ML)
				{
					if (attacker.Talisman is BaseTalisman talisman && talisman.Killer != null)
					{
						percentageBonus += talisman.Killer.DamageBonus(defender);
					}

					if (this is ButchersWarCleaver)
					{
						if (defender is Bull || defender is Cow || defender is Gaman)
						{
							percentageBonus += 100;
						}
					}
				}
				#endregion

				percentageBonus += ForceOfNature.GetBonus(attacker, defender);

				//if (m_ExtendedWeaponAttributes.AssassinHoned > 0 && GetOppositeDir(attacker.Direction) == defender.Direction)
				//{
				//	if (!ranged || 0.5 > Utility.RandomDouble())
				//	{
				//		percentageBonus += (int)(146.0 / MlSpeed);
				//	}
				//}

				//if (m_ExtendedWeaponAttributes.Focus > 0)
				//{
				//	percentageBonus += Focus.GetBonus(attacker, defender);
				//	Focus.OnHit(attacker, defender);
				//}

				percentageBonus = Math.Min(percentageBonus, 300);

				damage = AOS.Scale(damage, 100 + percentageBonus);
				#endregion

				damage = AbsorbDamage(attacker, defender, damage);

				if (!Core.AOS && damage < 1)
				{
					damage = 1;
				}
				else if (Core.AOS && damage == 0) // parried
				{
					if ((a != null && a.Validate(attacker)) || (move != null && move.Validate(attacker)))
					// Parried special moves have no mana cost - era specific
					{
						if (Core.SE || (a != null && a.CheckMana(attacker, true)))
						{
							WeaponAbility.ClearCurrentAbility(attacker);
							SpecialMove.ClearCurrentMove(attacker);

							attacker.SendLocalizedMessage(1061140); // Your attack was parried!
						}
					}

					return;
				}
				#region Mondain's Legacy
				if (ImmolatingWeaponSpell.IsImmolating(attacker, this))
				{
					int d = ImmolatingWeaponSpell.GetImmolatingDamage(attacker);
					d = AOS.Damage(defender, attacker, d, 0, 100, 0, 0, 0);

					ImmolatingWeaponSpell.DoDelayEffect(attacker, defender);

					if (d > 0)
					{
						defender.Damage(d);
					}
				}
				#endregion

				#region BoneBreaker/Swarm/Sparks
				/*bool sparks = false;
				if (a == null && move == null)
				{
					if (m_ExtendedWeaponAttributes.BoneBreaker > 0)
					{
						damage += BoneBreakerContext.CheckHit(attacker, defender);
					}

					if (m_ExtendedWeaponAttributes.HitSwarm > 0 && Utility.Random(100) < m_ExtendedWeaponAttributes.HitSwarm)
					{
						SwarmContext.CheckHit(attacker, defender);
					}

					if (m_ExtendedWeaponAttributes.HitSparks > 0 && Utility.Random(100) < m_ExtendedWeaponAttributes.HitSparks)
					{
						SparksContext.CheckHit(attacker, defender);
						sparks = true;
					}
				}*/
				#endregion

				Timer.DelayCall(d => AddBlood(d, damage), defender);

				int damageGiven = damage;

				if (a != null && !a.OnBeforeDamage(attacker, defender))
				{
					WeaponAbility.ClearCurrentAbility(attacker);
					a = null;
				}

				if (move != null && !move.OnBeforeDamage(attacker, defender))
				{
					SpecialMove.ClearCurrentMove(attacker);
					move = null;
				}

				if (Feint.Registry.ContainsKey(defender) && Feint.Registry[defender].Enemy == attacker)
				{
					damage -= (int)(damage * ((double)Feint.Registry[defender].DamageReduction / 100));
				}

				// Bane
				/*if (m_ExtendedWeaponAttributes.Bane > 0 && defender.Hits < defender.HitsMax / 2)
				{
					double inc = Math.Min(350, defender.HitsMax * .3);
					inc -= defender.Hits / (double)defender.HitsMax * inc;

					Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x30, 3);

					damage += (int)inc;
				}*/

				damage += WhirlwindAttack.DamageBonus(attacker, defender);

				damageGiven = AOS.Damage(defender, attacker, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, false, ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

				DualWield.DoHit(attacker, defender, damage);

				/*if (sparks)
				{
					int mana = attacker.Mana + damageGiven;
					if (!defender.Player)
					{
						mana *= 2;
					}

					attacker.Mana = Math.Min(attacker.ManaMax, attacker.Mana + mana);
				}*/

				double propertyBonus = (move == null) ? 1.0 : move.GetPropertyBonus(attacker);

				if (Core.AOS)
				{
					int lifeLeech = 0;
					int stamLeech = 0;
					int manaLeech = 0;

					if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechStam) * propertyBonus) >
						Utility.Random(100))
					{
						stamLeech += 100; // HitLeechStam% chance to leech 100% of damage as stamina
					}

					if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) * propertyBonus) >
						Utility.Random(100))
					{
						lifeLeech += 30; // HitLeechHits% chance to leech 30% of damage as hit points
					}

					if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechMana) * propertyBonus) >
						Utility.Random(100))
					{
						manaLeech += 40; // HitLeechMana% chance to leech 40% of damage as mana
					}


					int toHealCursedWeaponSpell = 0;

					if (CurseWeaponSpell.IsCursed(attacker, this))
					{
						toHealCursedWeaponSpell += AOS.Scale(damageGiven, 50); // Additional 50% life leech for cursed weapons (necro spell)
					}

					context = TransformationSpellHelper.GetContext(attacker);

					if (stamLeech != 0)
					{
						attacker.Stam += AOS.Scale(damageGiven, stamLeech);
					}

					if (toHealCursedWeaponSpell != 0)
					{
						attacker.Hits += toHealCursedWeaponSpell;
					}

					if (lifeLeech != 0)
					{
						attacker.Hits += AOS.Scale(damageGiven, lifeLeech);
					}

					if (manaLeech != 0)
					{
						attacker.Mana += AOS.Scale(damageGiven, manaLeech);
					}


					if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0 || toHealCursedWeaponSpell != 0)
					{
						attacker.PlaySound(0x44D);
					}
				}

				if (attacker is VampireBatFamiliar || attacker is VampireBat)
				{
					BaseCreature bc = (BaseCreature)attacker;
					Mobile caster = bc.ControlMaster;

					if (caster == null)
					{
						caster = bc.SummonMaster;
					}

					if (caster != null && caster.Map == bc.Map && caster.InRange(bc, 2))
					{
						caster.Hits += damage;
					}
					else
					{
						bc.Hits += damage;
					}
				}

				if (Core.AOS && !BlockHitEffects)
				{
					int physChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPhysicalArea) * propertyBonus);
					int fireChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireArea) * propertyBonus);
					int coldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitColdArea) * propertyBonus);
					int poisChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPoisonArea) * propertyBonus);
					int nrgyChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitEnergyArea) * propertyBonus);

					if (physChance != 0 && physChance > Utility.Random(100))
					{
						WeaponEffects.DoAreaAttack(attacker, defender, damageGiven, 0x10E, 50, 100, 0, 0, 0, 0);
					}

					if (fireChance != 0 && fireChance > Utility.Random(100))
					{
						WeaponEffects.DoAreaAttack(attacker, defender, damageGiven, 0x11D, 1160, 0, 100, 0, 0, 0);
					}

					if (coldChance != 0 && coldChance > Utility.Random(100))
					{
						WeaponEffects.DoAreaAttack(attacker, defender, damageGiven, 0x0FC, 2100, 0, 0, 100, 0, 0);
					}

					if (poisChance != 0 && poisChance > Utility.Random(100))
					{
						WeaponEffects.DoAreaAttack(attacker, defender, damageGiven, 0x205, 1166, 0, 0, 0, 100, 0);
					}

					if (nrgyChance != 0 && nrgyChance > Utility.Random(100))
					{
						WeaponEffects.DoAreaAttack(attacker, defender, damageGiven, 0x1F1, 120, 0, 0, 0, 0, 100);
					}

					int maChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitMagicArrow) * propertyBonus);
					int harmChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitHarm) * propertyBonus);
					int fireballChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireball) * propertyBonus);
					int lightningChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLightning) * propertyBonus);
					int dispelChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitDispel) * propertyBonus);
					//int explosChance = (int)(ExtendedWeaponAttributes.GetValue(attacker, ExtendedWeaponAttribute.HitExplosion) * propertyBonus);

					#region Mondains Legacy
					int velocityChance = this is BaseRanged ranged1 ? ranged1.Velocity : 0;
					#endregion

					if (maChance != 0 && maChance > Utility.Random(100))
					{
						WeaponEffects.DoMagicArrow(attacker, defender);
					}

					if (harmChance != 0 && harmChance > Utility.Random(100))
					{
						WeaponEffects.DoHarm(attacker, defender);
					}

					if (fireballChance != 0 && fireballChance > Utility.Random(100))
					{
						WeaponEffects.DoFireball(attacker, defender);
					}

					if (lightningChance != 0 && lightningChance > Utility.Random(100))
					{
						WeaponEffects.DoLightning(attacker, defender);
					}

					if (dispelChance != 0 && dispelChance > Utility.Random(100))
					{
						WeaponEffects.DoDispel(attacker, defender);
					}

					//if (explosChance != 0 && explosChance > Utility.Random(100))
					//{
					//	DoExplosion(attacker, defender);
					//}

					#region Mondains Legacy
					if (Core.ML && velocityChance != 0 && velocityChance > Utility.Random(100))
					{
						WeaponEffects.DoHitVelocity(attacker, damageable);
					}
					#endregion

					int laChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerAttack) * propertyBonus);

					if (laChance != 0 && laChance > Utility.Random(100))
					{
						WeaponEffects.DoLowerAttack(attacker, defender);
					}

					int hldWep = m_AosWeaponAttributes.HitLowerDefend;
					int hldGlasses = 0;

					var helm = attacker.FindItemOnLayer(Layer.Helm);

					if ((hldWep > 0 && hldWep > Utility.Random(100)) || (hldGlasses > 0 && hldGlasses > Utility.Random(100)))
					{
						WeaponEffects.DoLowerDefense(attacker, defender);
					}

				}

				if (attacker is BaseCreature creature)
				{
					creature.OnGaveMeleeAttack(defender);
				}

				if (defender is BaseCreature creature1)
				{
					creature1.OnGotMeleeAttack(attacker);
				}

				if (a != null)
				{
					a.OnHit(attacker, defender, damage);
				}

				if (move != null)
				{
					move.OnHit(attacker, defender, damage);
				}

				if (defender is IHonorTarget target && target.ReceivedHonorContext != null)
				{
					target.ReceivedHonorContext.OnTargetHit(attacker);
				}

				if (!ranged)
				{
					if (AnimalForm.UnderTransformation(attacker, typeof(GiantSerpent)))
					{
						defender.ApplyPoison(attacker, Poison.Lesser);
					}

					if (AnimalForm.UnderTransformation(defender, typeof(BullFrog)))
					{
						attacker.ApplyPoison(defender, Poison.Regular);
					}
				}

				BaseFamiliar.OnHit(attacker, damageable);
				XmlAttach.OnWeaponHit(this, attacker, defender, damageGiven);
			}
			else
			{
				Mobile defender = damageable as Mobile;
				PlayHurtAnimation(defender);

				attacker.PlaySound(GetHitAttackSound(attacker, defender));
				defender.PlaySound(GetHitDefendSound(attacker, defender));

				int damage = ComputeDamage(attacker, defender);

				if (Effect != WeaponEffect.None && Charges > 0)
				{
					#region Magic Weapon Effects
					if (Effect == WeaponEffect.Clumsy)
					{
						string name = String.Format("[Magic] {0} Offset", StatType.Dex);
						StatMod mod = defender.GetStatMod(name);

						if (mod != null && mod.Offset < 0)
							defender.AddStatMod(new StatMod(StatType.Dex, name, mod.Offset + -10, TimeSpan.FromSeconds(60.0)));
						else if (mod == null || mod.Offset < -10)
							defender.AddStatMod(new StatMod(StatType.Dex, name, -10, TimeSpan.FromSeconds(60.0)));

						Charges--;
						defender.FixedParticles(0x3779, 10, 15, 5002, EffectLayer.Head);
						defender.PlaySound(0x1DF);
					}
					else if (Effect == WeaponEffect.Feeblemind)
					{
						string name = String.Format("[Magic] {0} Offset", StatType.Int);
						StatMod mod = defender.GetStatMod(name);

						if (mod != null && mod.Offset < 0)
							defender.AddStatMod(new StatMod(StatType.Int, name, mod.Offset + -10, TimeSpan.FromSeconds(60.0)));
						else if (mod == null || mod.Offset < 10)
							defender.AddStatMod(new StatMod(StatType.Int, name, -10, TimeSpan.FromSeconds(60.0)));

						Charges--;
						defender.FixedParticles(0x3779, 10, 15, 5004, EffectLayer.Head);
						defender.PlaySound(0x1E4);
					}
					else if (Effect == WeaponEffect.MagicArrow)
					{
						WeaponEffects.DoMagicArrow(attacker, defender);
						Charges--;
						/*attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
						attacker.PlaySound(0x1E5);*/
					}
					else if (Effect == WeaponEffect.Weakness)
					{
						string name = String.Format("[Magic] {0} Offset", StatType.Str);
						StatMod mod = defender.GetStatMod(name);

						if (mod != null && mod.Offset < 0)
							defender.AddStatMod(new StatMod(StatType.Str, name, mod.Offset + -10, TimeSpan.FromSeconds(60.0)));
						else if (mod == null || mod.Offset < 10)
							defender.AddStatMod(new StatMod(StatType.Str, name, -10, TimeSpan.FromSeconds(60.0)));

						Charges--;
						defender.FixedParticles(0x3779, 10, 15, 5009, EffectLayer.Waist);
						defender.PlaySound(0x1E6);
					}
					else if (Effect == WeaponEffect.Harm)
					{
						WeaponEffects.DoHarm(attacker, defender);
						Charges--;
						/*defender.FixedParticles(0x374A, 10, 15, 5013, EffectLayer.Waist);
						defender.PlaySound(0x1F1);*/
					}
					else if (Effect == WeaponEffect.Paralyze)
					{
						defender.Paralyze(TimeSpan.FromSeconds(7));
						Charges--;
						defender.PlaySound(0x204);
						defender.FixedEffect(0x376A, 6, 1);
					}
					else if (Effect == WeaponEffect.Fireball)
					{
						WeaponEffects.DoFireball(attacker, defender);
						Charges--;
						/*attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
						attacker.PlaySound(0x15E);*/
					}
					else if (Effect == WeaponEffect.Curse)
					{
						string nameS = String.Format("[Magic] {0} Offset", StatType.Str);
						string nameD = String.Format("[Magic] {0} Offset", StatType.Dex);
						string nameI = String.Format("[Magic] {0} Offset", StatType.Int);
						StatMod strmod = defender.GetStatMod(nameS);
						StatMod dexmod = defender.GetStatMod(nameD);
						StatMod intmod = defender.GetStatMod(nameI);

						if (strmod != null && strmod.Offset > 0)
							defender.AddStatMod(new StatMod(StatType.Str, nameS, strmod.Offset + -10, TimeSpan.FromSeconds(60.0)));
						else if (strmod == null || strmod.Offset > 10)
							defender.AddStatMod(new StatMod(StatType.Str, nameS, -10, TimeSpan.FromSeconds(60.0)));

						if (dexmod != null && dexmod.Offset > 0)
							defender.AddStatMod(new StatMod(StatType.Dex, nameD, dexmod.Offset + -10, TimeSpan.FromSeconds(60.0)));
						else if (dexmod == null || dexmod.Offset > 10)
							defender.AddStatMod(new StatMod(StatType.Dex, nameD, -10, TimeSpan.FromSeconds(60.0)));

						if (intmod != null && intmod.Offset > 0)
							defender.AddStatMod(new StatMod(StatType.Int, nameI, intmod.Offset + -10, TimeSpan.FromSeconds(60.0)));
						else if (intmod == null || intmod.Offset > 10)
							defender.AddStatMod(new StatMod(StatType.Int, nameI, -10, TimeSpan.FromSeconds(60.0)));

						Charges--;
						defender.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
						defender.PlaySound(0x1EA);
					}
					else if (Effect == WeaponEffect.ManaDrain)
					{
						defender.Mana -= 10;
						Charges--;
						defender.FixedParticles(0x374A, 10, 15, 5032, EffectLayer.Head);
						defender.PlaySound(0x1F8);
					}
					else if (Effect == WeaponEffect.Lightning)
					{
						WeaponEffects.DoLightning(attacker, defender);
						Charges--;
						/*defender.BoltEffect(0);*/
					}
					#endregion
				}

				CheckSlayerResult cs = CheckSlayers(attacker, defender, SlayerName.None);

				if (cs != CheckSlayerResult.None)
				{
					if (cs == CheckSlayerResult.Slayer)
						defender.FixedEffect(0x37B9, 10, 5);

					damage *= 2;
				}

				if (attacker is BaseCreature)
					((BaseCreature)attacker).AlterMeleeDamageTo(defender, ref damage);

				if (defender is BaseCreature)
					((BaseCreature)defender).AlterMeleeDamageFrom(attacker, ref damage);

				damage = AbsorbDamage(attacker, defender, damage);

				// Halve the computed damage and return
				damage /= 2;

				if (damage < 1)
					damage = 1;

				if (attacker is PlayerMobile)
					damage += 2;

				Timer.DelayCall(d => AddBlood(d, damage), defender);
				//AddBlood(attacker, defender, damage);
				defender.Damage(damage, attacker);

				if (defender is Slime)
				{
					if ((damage > (defender.Hits / 4)) && (defender.Hits > 5))
					{
						defender.Say(true, "*The slime splits when struck!*");
						BaseCreature slime = new Slime();
						slime.Hits = (defender.Hits / 2);
						defender.Hits /= 2;
						slime.MoveToWorld(new Point3D(defender.X, defender.Y, defender.Z), defender.Map);
					}
				}

				Item hammer = attacker.FindItemOnLayer(Layer.OneHanded);

				if ((m_MaxHits > 0 || (hammer != null && hammer is SmithHammer && ((SmithHammer)hammer).MaxHitPoints > 0)) && ((MaxRange <= 1 && (defender is Slime || defender is AcidElemental)) || Utility.Random(25) == 0)) // Stratics says 50% chance, seems more like 4%..
				{
					if ((MaxRange <= 1 || (hammer != null && hammer is SmithHammer)) && (defender is Slime || defender is AcidElemental))
						attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, true, "*Acid blood scars your weapon!*");

					if ((m_Hits > 0) || (hammer != null && hammer is SmithHammer && ((SmithHammer)hammer).HitPoints > 0))
					{
						--HitPoints;
					}
					else if ((m_MaxHits > 1) || (hammer != null && hammer is SmithHammer && ((SmithHammer)hammer).MaxHitPoints > 1))
					{
						--MaxHitPoints;

						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, true, "Your equipment is severely damaged.");
					}
					else
					{
						Delete();
					}
				}

				if (attacker is BaseCreature)
					((BaseCreature)attacker).OnGaveMeleeAttack(defender);

				if (defender is BaseCreature)
					((BaseCreature)defender).OnGotMeleeAttack(attacker);
			}
		}

		public static double GetAosSpellDamage(Mobile attacker, Mobile defender, int bonus, int dice, int sides)
		{
			int damage = Utility.Dice(dice, sides, bonus) * 100;
			int damageBonus = 0;

			int inscribeSkill = attacker.Skills[SkillName.Inscribe].Fixed;
			int inscribeBonus = (inscribeSkill + (1000 * (inscribeSkill / 1000))) / 200;

			damageBonus += inscribeBonus;
			damageBonus += attacker.Int / 10;
			damageBonus += SpellHelper.GetSpellDamageBonus(attacker, defender, SkillName.Magery, attacker is PlayerMobile && defender is PlayerMobile);
			damage = AOS.Scale(damage, 100 + damageBonus);

			if (defender != null && Feint.Registry.ContainsKey(defender) && Feint.Registry[defender].Enemy == attacker)
			{
				damage -= (int)(damage * ((double)Feint.Registry[defender].DamageReduction / 100));
			}

			// All hit spells use 80 eval
			int evalScale = 30 + ((9 * 800) / 100);

			damage = AOS.Scale(damage, evalScale);

			return damage / 100;
		}

		public virtual double GetAosDamage(Mobile attacker, int bonus, int dice, int sides)
		{
			int damage = Utility.Dice(dice, sides, bonus) * 100;
			int damageBonus = 0;

			// Inscription bonus
			int inscribeSkill = attacker.Skills[SkillName.Inscribe].Fixed;

			damageBonus += inscribeSkill / 200;

			if (inscribeSkill >= 1000)
				damageBonus += 5;

			if (attacker.Player)
			{
				// Int bonus
				damageBonus += (attacker.Int / 10);

				// SDI bonus
				damageBonus += AosAttributes.GetValue(attacker, AosAttribute.SpellDamage);

				TransformContext context = TransformationSpellHelper.GetContext(attacker);

				if (context != null && context.Spell is ReaperFormSpell spell)
					damageBonus += spell.SpellDamageBonus;
			}

			damage = AOS.Scale(damage, 100 + damageBonus);

			return damage / 100;
		}

		public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender, SlayerName slayer)
		{
			if (slayer == SlayerName.None)
			{
				return CheckSlayerResult.None;
			}

			_ = attacker.Weapon as BaseWeapon;
			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(slayer);

			if (atkSlayer != null && atkSlayer.Slays(defender) && _SuperSlayers.Contains(atkSlayer.Name))
			{
				return CheckSlayerResult.SuperSlayer;
			}

			if (atkSlayer != null && atkSlayer.Slays(defender))
			{
				return CheckSlayerResult.Slayer;
			}

			return CheckSlayerResult.None;
		}

		public static CheckSlayerResult CheckSlayerOpposition(Mobile attacker, Mobile defender)
		{
			ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

			if (defISlayer == null)
			{
				defISlayer = defender.Weapon as ISlayer;
			}

			if (defISlayer != null)
			{
				SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
				SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);
				SlayerEntry defSetSlayer = SlayerGroup.GetEntryByName(SetHelper.GetSetSlayer(defender));

				if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker) ||
					defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(attacker) ||
					defSetSlayer != null && defSetSlayer.Group.OppositionSuperSlays(attacker))
				{
					return CheckSlayerResult.Opposition;
				}
			}

			return CheckSlayerResult.None;
		}

		public CheckSlayerResult CheckTalismanSlayer(Mobile attacker, Mobile defender)
		{
			if (attacker.Talisman is BaseTalisman talisman && TalismanSlayer.Slays(talisman.Slayer, defender))
			{
				return CheckSlayerResult.Slayer;
			}
			else if (Slayer3 != TalismanSlayerName.None && TalismanSlayer.Slays(Slayer3, defender))
			{
				return CheckSlayerResult.Slayer;
			}

			return CheckSlayerResult.None;
		}

		private readonly List<SlayerName> _SuperSlayers = new List<SlayerName>()
		{
			SlayerName.Repond, SlayerName.Silver, SlayerName.Fey,
			SlayerName.ElementalBan, SlayerName.Exorcism, SlayerName.ArachnidDoom,
			SlayerName.ReptilianDeath
		};

		#region Blood
		public void AddBlood(Mobile defender, int damage)
		{
			if (damage <= 5 || defender == null || defender.Map == null || !defender.HasBlood || !CanDrawBlood(defender))
			{
				return;
			}
			defender.Bleed(defender, damage);
			/*var m = defender.Map;
			var b = new Rectangle2D(defender.X - 2, defender.Y - 2, 5, 5);

			var count = Core.AOS ? Utility.RandomMinMax(2, 3) : Utility.RandomMinMax(1, 2);

			for (var i = 0; i < count; i++)
			{
				AddBlood(defender, m.GetRandomSpawnPoint(b), m);
			}*/
		}

		protected virtual void AddBlood(Mobile defender, Point3D target, Map map)
		{
			var blood = CreateBlood(defender);

			var id = blood.ItemID;

			blood.ItemID = 1; // No Draw

			blood.OnBeforeSpawn(target, map);
			blood.MoveToWorld(target, map);
			blood.OnAfterSpawn();

			Effects.SendMovingEffect(defender, blood, id, 7, 10, true, false, blood.Hue, 0);

			Timer.DelayCall(TimeSpan.FromMilliseconds(500), b => b.ItemID = id, blood);
		}

		protected virtual bool CanDrawBlood(Mobile defender)
		{
			return defender.HasBlood;
		}

		protected virtual Blood CreateBlood(Mobile defender)
		{
			return new Blood
			{
				Hue = defender.BloodHue
			};
		}
		#endregion

		public static BaseWeapon FindEquippedWeapon(Mobile m)
		{
			Item item = m.FindItemOnLayer(Layer.TwoHanded);
			if (item is BaseWeapon weapon1)
			{
				return weapon1;
			}

			item = m.FindItemOnLayer(Layer.OneHanded);
			if (item is BaseWeapon weapon)
			{
				return weapon;
			}

			return null;
		}

		#region Elemental Damage
		public static int[] GetElementDamages(Mobile m)
		{
			var o = new[] { 100, 0, 0, 0, 0, 0, 0 };

			var w = m.Weapon as BaseWeapon ?? Fists;

			if (w != null)
			{
				w.GetDamageTypes(m, out o[0], out o[1], out o[2], out o[3], out o[4], out o[5], out o[6]);
			}

			return o;
		}
		#endregion
		public virtual void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
		{
			if (wielder is BaseCreature bc)
			{
				phys = bc.PhysicalDamage;
				fire = bc.FireDamage;
				cold = bc.ColdDamage;
				pois = bc.PoisonDamage;
				nrgy = bc.EnergyDamage;
				chaos = bc.ChaosDamage;
				direct = bc.DirectDamage;
			}
			else
			{
				fire = m_AosElementDamages.Fire;
				cold = m_AosElementDamages.Cold;
				pois = m_AosElementDamages.Poison;
				nrgy = m_AosElementDamages.Energy;
				chaos = m_AosElementDamages.Chaos;
				direct = m_AosElementDamages.Direct;

				phys = 100 - fire - cold - pois - nrgy - chaos - direct;

				CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

				if (resInfo != null)
				{
					CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

					if (attrInfo != null)
					{
						int left = phys;

						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponColdDamage, ref cold, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponEnergyDamage, ref nrgy, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponFireDamage, ref fire, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponPoisonDamage, ref pois, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponChaosDamage, ref chaos, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponDirectDamage, ref direct, left);

						phys = left;
					}
				}
			}
		}

		private static int ApplyCraftAttributeElementDamage(int attrDamage, ref int element, int totalRemaining)
		{
			if (totalRemaining <= 0)
				return 0;

			if (attrDamage <= 0)
				return totalRemaining;

			int appliedDamage = attrDamage;

			if ((appliedDamage + element) > 100)
				appliedDamage = 100 - element;

			if (appliedDamage > totalRemaining)
				appliedDamage = totalRemaining;

			element += appliedDamage;

			return totalRemaining - appliedDamage;
		}

		public virtual void OnMiss(Mobile attacker, IDamageable damageable)
		{
			Mobile defender = damageable as Mobile;
			PlaySwingAnimation(attacker);
			attacker.PlaySound(GetMissAttackSound(attacker, defender));

			if (defender != null)
			{
				defender.PlaySound(GetMissDefendSound(attacker, defender));
			}

			WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

			if (ability != null)
				ability.OnMiss(attacker, defender);

			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

			if (move != null)
				move.OnMiss(attacker, defender);

			if (defender is IHonorTarget target && target.ReceivedHonorContext != null)
				target.ReceivedHonorContext.OnTargetMissed(attacker);
		}

		public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
		{
			if (attacker is BaseCreature c)
			{
				if (c.DamageMin >= 0)
				{
					min = c.DamageMin;
					max = c.DamageMax;
					return;
				}

				if (this is Fists && !attacker.Body.IsHuman)
				{
					min = attacker.Str / 28;
					max = attacker.Str / 28;
					return;
				}
			}

			min = MinDamage;
			max = MaxDamage;
		}

		public virtual double GetBaseDamage(Mobile attacker)
		{
			GetBaseDamageRange(attacker, out int min, out int max);

			int damage = Utility.RandomMinMax(min, max);

			if (Core.AOS)
				return damage;

			/* Apply damage level offset
			 * : Regular : 0
			 * : Ruin    : 1
			 * : Might   : 3
			 * : Force   : 5
			 * : Power   : 7
			 * : Vanq    : 9
			 */
			if (m_DamageLevel != WeaponDamageLevel.Regular)
				damage += (2 * (int)m_DamageLevel) - 1;

			return damage;
		}

		public virtual double GetBonus(double value, double scalar, double threshold, double offset)
		{
			double bonus = value * scalar;

			if (value >= threshold)
				bonus += offset;

			return bonus / 100;
		}

		public virtual int GetHitChanceBonus()
		{
			if (!Core.AOS)
				return 0;

			int bonus = 0;

			switch (m_AccuracyLevel)
			{
				case WeaponAccuracyLevel.Accurate: bonus += 02; break;
				case WeaponAccuracyLevel.Surpassingly: bonus += 04; break;
				case WeaponAccuracyLevel.Eminently: bonus += 06; break;
				case WeaponAccuracyLevel.Exceedingly: bonus += 08; break;
				case WeaponAccuracyLevel.Supremely: bonus += 10; break;
			}

			return bonus;
		}

		public virtual int GetDamageBonus()
		{
			int bonus = VirtualDamageBonus;

			switch (Quality)
			{
				case ItemQuality.Low: bonus -= 20; break;
				case ItemQuality.Exceptional: bonus += 20; break;
			}

			switch (m_DamageLevel)
			{
				case WeaponDamageLevel.Ruin: bonus += 15; break;
				case WeaponDamageLevel.Might: bonus += 20; break;
				case WeaponDamageLevel.Force: bonus += 25; break;
				case WeaponDamageLevel.Power: bonus += 30; break;
				case WeaponDamageLevel.Vanq: bonus += 35; break;
			}

			return bonus;
		}

		public virtual void GetStatusDamage(Mobile from, out int min, out int max)
		{
			GetBaseDamageRange(from, out int baseMin, out int baseMax);

			if (Core.AOS)
			{
				min = Math.Max((int)ScaleDamageAOS(from, baseMin, false), 1);
				max = Math.Max((int)ScaleDamageAOS(from, baseMax, false), 1);
			}
			else
			{
				min = Math.Max((int)ScaleDamageOld(from, baseMin, false), 1);
				max = Math.Max((int)ScaleDamageOld(from, baseMax, false), 1);
			}
		}

		public virtual double ScaleDamageAOS(Mobile attacker, double damage, bool checkSkills)
		{
			if (checkSkills)
			{
				_ = attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
				_ = attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

				if (Type == WeaponType.Axe)
					_ = attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
			}

			#region Physical bonuses
			/*
			 * These are the bonuses given by the physical characteristics of the mobile.
			 * No caps apply.
			 */
			double strengthBonus = GetBonus(attacker.Str, 0.300, 100.0, 5.00);
			double anatomyBonus = GetBonus(attacker.Skills[SkillName.Anatomy].Value, 0.500, 100.0, 5.00);
			double tacticsBonus = GetBonus(attacker.Skills[SkillName.Tactics].Value, 0.625, 100.0, 6.25);
			double lumberBonus = GetBonus(attacker.Skills[SkillName.Lumberjacking].Value, 0.200, 100.0, 10.00);

			if (Type != WeaponType.Axe)
				lumberBonus = 0.0;
			#endregion

			#region Modifiers
			//Get the damage bonus from the attacker
			int damageBonus = attacker.GetDamageBonus();

			if (damageBonus > 100)
				damageBonus = 100;
			#endregion

			double totalBonus = strengthBonus + anatomyBonus + tacticsBonus + lumberBonus + ((GetDamageBonus() + damageBonus) / 100.0);

			return damage + (int)(damage * totalBonus);
		}

		public virtual int VirtualDamageBonus => 0;

		public virtual int ComputeDamageAOS(Mobile attacker, Mobile defender)
		{
			return (int)ScaleDamageAOS(attacker, GetBaseDamage(attacker), true);
		}

		public virtual double GetTacticsModifier(Mobile attacker)
		{
			/* Compute tactics modifier
			 * :   0.0 = 50% loss
			 * :  50.0 = unchanged
			 * : 100.0 = 50% bonus
			 */
			return (attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0;
		}

		public virtual double GetDamageModifiers(Mobile attacker)
		{
			/* Compute strength modifier
			 * : 1% bonus for every 5 strength
			 */
			double modifier = attacker.Str / 5.0 / 100.0;

			/* Compute anatomy modifier
			 * : 1% bonus for every 5 points of anatomy
			 * : +10% bonus at Grandmaster or higher
			 */
			double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
			modifier += anatomyValue / 5.0 / 100.0;

			if (anatomyValue >= 100.0)
				modifier += 0.1;

			//Add the weapon damage bonus
			modifier += GetWeaponModifiers(attacker);

			return modifier;
		}

		public virtual double GetWeaponModifiers(Mobile attacker)
		{
			double modifier = 0;

			/* Compute lumberjacking bonus
			 * : 1% bonus for every 5 points of lumberjacking
			 * : +10% bonus at Grandmaster or higher
			 */
			if (Type == WeaponType.Axe)
			{
				double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;

				modifier += ((lumberValue / 5.0) / 100.0);

				if (lumberValue >= 100.0)
					modifier += 0.1;
			}

			// New quality bonus:
			if (Quality != ItemQuality.Normal)
				modifier += ((int)Quality - 1) * 0.2;

			// Virtual damage bonus:
			if (VirtualDamageBonus != 0)
				modifier += VirtualDamageBonus / 100.0;

			return modifier;
		}

		public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills)
		{
			if (checkSkills)
			{
				_ = attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
				_ = attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

				if (Type == WeaponType.Axe)
					_ = attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
			}

			// Compute tactics modifier
			damage += damage * GetTacticsModifier(attacker);

			//Get the modifiers damage
			double modifiers = GetWeaponModifiers(attacker);

			// Apply bonuses
			damage += damage * modifiers;

			return ScaleDamageByDurability((int)damage);
		}

		public virtual int ScaleDamageByDurability(int damage)
		{
			int scale = 100;

			if (m_MaxHits > 0 && m_Hits < m_MaxHits)
				scale = 50 + ((50 * m_Hits) / m_MaxHits);

			return AOS.Scale(damage, scale);
		}

		public virtual int ComputeDamage(Mobile attacker, Mobile defender)
		{
			if (Core.AOS)
				return ComputeDamageAOS(attacker, defender);

			int damage = (int)ScaleDamageOld(attacker, GetBaseDamage(attacker), true);

			// pre-AOS, halve damage if the defender is a player or the attacker is not a player
			if (defender is PlayerMobile || attacker is not PlayerMobile)
				damage = (int)(damage / 2.0);

			return damage;
		}

		public virtual void PlayHurtAnimation(Mobile from)
		{
			int action;
			int frames;

			switch (from.Body.Type)
			{
				case BodyType.Sea:
				case BodyType.Animal:
					{
						action = 7;
						frames = 5;
						break;
					}
				case BodyType.Monster:
					{
						action = 10;
						frames = 4;
						break;
					}
				case BodyType.Human:
					{
						action = 20;
						frames = 5;
						break;
					}
				default: return;
			}

			if (from.Mounted)
				return;

			from.Animate(action, frames, 1, true, false, 0);
		}

		public virtual void PlaySwingAnimation(Mobile from)
		{
			int action;

			switch (from.Body.Type)
			{
				case BodyType.Sea:
				case BodyType.Animal:
					{
						action = Utility.Random(5, 2);
						break;
					}
				case BodyType.Monster:
					{
						switch (Animation)
						{
							default:
							case WeaponAnimation.Wrestle:
							case WeaponAnimation.Bash1H:
							case WeaponAnimation.Pierce1H:
							case WeaponAnimation.Slash1H:
							case WeaponAnimation.Bash2H:
							case WeaponAnimation.Pierce2H:
							case WeaponAnimation.Slash2H: action = Utility.Random(4, 3); break;
							case WeaponAnimation.ShootBow: return; // 7
							case WeaponAnimation.ShootXBow: return; // 8
						}

						break;
					}
				case BodyType.Human:
					{
						if (!from.Mounted)
						{
							action = (int)Animation;
						}
						else
						{
							action = Animation switch
							{
								WeaponAnimation.Bash2H or WeaponAnimation.Pierce2H or WeaponAnimation.Slash2H => 29,
								WeaponAnimation.ShootBow => 27,
								WeaponAnimation.ShootXBow => 28,
								_ => 26,
							};
						}

						break;
					}
				default: return;
			}

			from.Animate(action, 7, 1, true, false, 0);
		}

		#region Serialization/Deserialization
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			#region Mondain's Legacy Sets
			SetFlag sflags = SetFlag.None;

			Utility.SetSaveFlag(ref sflags, SetFlag.Attributes, !m_SetAttributes.IsEmpty);
			Utility.SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !m_SetSkillBonuses.IsEmpty);
			Utility.SetSaveFlag(ref sflags, SetFlag.Hue, m_SetHue != 0);
			Utility.SetSaveFlag(ref sflags, SetFlag.LastEquipped, m_LastEquipped);
			Utility.SetSaveFlag(ref sflags, SetFlag.SetEquipped, m_SetEquipped);
			Utility.SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, m_SetSelfRepair != 0);
			Utility.SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, m_SetPhysicalBonus != 0);
			Utility.SetSaveFlag(ref sflags, SetFlag.FireBonus, m_SetFireBonus != 0);
			Utility.SetSaveFlag(ref sflags, SetFlag.ColdBonus, m_SetColdBonus != 0);
			Utility.SetSaveFlag(ref sflags, SetFlag.PoisonBonus, m_SetPoisonBonus != 0);
			Utility.SetSaveFlag(ref sflags, SetFlag.EnergyBonus, m_SetEnergyBonus != 0);

			writer.WriteEncodedInt((int)sflags);

			if (sflags.HasFlag(SetFlag.PhysicalBonus))
			{
				writer.WriteEncodedInt(m_SetPhysicalBonus);
			}

			if (sflags.HasFlag(SetFlag.FireBonus))
			{
				writer.WriteEncodedInt(m_SetFireBonus);
			}

			if (sflags.HasFlag(SetFlag.ColdBonus))
			{
				writer.WriteEncodedInt(m_SetColdBonus);
			}

			if (sflags.HasFlag(SetFlag.PoisonBonus))
			{
				writer.WriteEncodedInt(m_SetPoisonBonus);
			}

			if (sflags.HasFlag(SetFlag.EnergyBonus))
			{
				writer.WriteEncodedInt(m_SetEnergyBonus);
			}

			if (sflags.HasFlag(SetFlag.Attributes))
			{
				m_SetAttributes.Serialize(writer);
			}

			if (sflags.HasFlag(SetFlag.SkillBonuses))
			{
				m_SetSkillBonuses.Serialize(writer);
			}

			if (sflags.HasFlag(SetFlag.Hue))
			{
				writer.Write(m_SetHue);
			}

			if (sflags.HasFlag(SetFlag.LastEquipped))
			{
				writer.Write(m_LastEquipped);
			}

			if (sflags.HasFlag(SetFlag.SetEquipped))
			{
				writer.Write(m_SetEquipped);
			}

			if (sflags.HasFlag(SetFlag.SetSelfRepair))
			{
				writer.WriteEncodedInt(m_SetSelfRepair);
			}
			#endregion

			writer.WriteEncodedInt((int)m_WeaponEffect);
			writer.Write((int)m_Charges);

			#region Mondain's Legacy
			writer.Write((int)m_Slayer3);
			#endregion

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.DamageLevel, m_DamageLevel != WeaponDamageLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, m_AccuracyLevel != WeaponAccuracyLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, m_DurabilityLevel != DurabilityLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.Hits, m_Hits != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxHits, m_MaxHits != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.Slayer, m_Slayer != SlayerName.None);
			Utility.SetSaveFlag(ref flags, SaveFlag.Poison, m_Poison != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.PoisonCharges, m_PoisonCharges != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MinDamage, m_MinDamage != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxDamage, m_MaxDamage != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.HitSound, m_HitSound != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MissSound, m_MissSound != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.Speed, m_Speed != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxRange, m_MaxRange != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.Skill, m_Skill != (SkillName)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.Type, m_Type != (WeaponType)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.Animation, m_Animation != (WeaponAnimation)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.Slayer2, m_Slayer2 != SlayerName.None);
			Utility.SetSaveFlag(ref flags, SaveFlag.ElementalDamages, !m_AosElementDamages.IsEmpty);

			writer.Write((int)flags);

			if (flags.HasFlag(SaveFlag.DamageLevel))
				writer.Write((int)m_DamageLevel);

			if (flags.HasFlag(SaveFlag.AccuracyLevel))
				writer.Write((int)m_AccuracyLevel);

			if (flags.HasFlag(SaveFlag.DurabilityLevel))
				writer.Write((int)m_DurabilityLevel);

			if (flags.HasFlag(SaveFlag.Hits))
				writer.Write(m_Hits);

			if (flags.HasFlag(SaveFlag.MaxHits))
				writer.Write(m_MaxHits);

			if (flags.HasFlag(SaveFlag.Slayer))
				writer.Write((int)m_Slayer);

			if (flags.HasFlag(SaveFlag.Poison))
				Poison.Serialize(m_Poison, writer);

			if (flags.HasFlag(SaveFlag.PoisonCharges))
				writer.Write(m_PoisonCharges);

			if (flags.HasFlag(SaveFlag.StrReq))
				writer.Write(m_StrReq);

			if (flags.HasFlag(SaveFlag.DexReq))
				writer.Write(m_DexReq);

			if (flags.HasFlag(SaveFlag.IntReq))
				writer.Write(m_IntReq);

			if (flags.HasFlag(SaveFlag.MinDamage))
				writer.Write(m_MinDamage);

			if (flags.HasFlag(SaveFlag.MaxDamage))
				writer.Write(m_MaxDamage);

			if (flags.HasFlag(SaveFlag.HitSound))
				writer.Write(m_HitSound);

			if (flags.HasFlag(SaveFlag.MissSound))
				writer.Write(m_MissSound);

			if (flags.HasFlag(SaveFlag.Speed))
				writer.Write(m_Speed);

			if (flags.HasFlag(SaveFlag.MaxRange))
				writer.Write(m_MaxRange);

			if (flags.HasFlag(SaveFlag.Skill))
				writer.Write((int)m_Skill);

			if (flags.HasFlag(SaveFlag.Type))
				writer.Write((int)m_Type);

			if (flags.HasFlag(SaveFlag.Animation))
				writer.Write((int)m_Animation);

			if (flags.HasFlag(SaveFlag.xWeaponAttributes))
				m_AosWeaponAttributes.Serialize(writer);

			if (flags.HasFlag(SaveFlag.SkillBonuses))
				m_AosSkillBonuses.Serialize(writer);

			if (flags.HasFlag(SaveFlag.Slayer2))
				writer.Write((int)m_Slayer2);

			if (flags.HasFlag(SaveFlag.ElementalDamages))
				m_AosElementDamages.Serialize(writer);
		}

		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			DamageLevel = 0x00000001,
			AccuracyLevel = 0x00000002,
			DurabilityLevel = 0x00000004,
			Hits = 0x00000008,
			MaxHits = 0x00000010,
			Slayer = 0x00000020,
			Poison = 0x00000040,
			PoisonCharges = 0x00000060,
			StrReq = 0x00000080,
			DexReq = 0x00000100,
			IntReq = 0x00000200,
			MinDamage = 0x00000400,
			MaxDamage = 0x00000600,
			HitSound = 0x00000800,
			MissSound = 0x00001000,
			Speed = 0x00002000,
			MaxRange = 0x00004000,
			Skill = 0x00006000,
			Type = 0x00008000,
			Animation = 0x00010000,
			xAttributes = 0x00020000,
			xWeaponAttributes = 0x00040000,
			SkillBonuses = 0x00060000,
			Slayer2 = 0x00080000,
			ElementalDamages = 0x00100000
		}

		#region Mondain's Legacy Sets
		/*private static void SetSaveFlag(ref SetFlag flags, SetFlag toSet, bool setIf)
		{
			if (setIf)
			{
				flags |= toSet;
			}
		}

		private static bool GetSaveFlag(SetFlag flags, SetFlag toGet)
		{
			return ((flags & toGet) != 0);
		}*/

		[Flags]
		private enum SetFlag
		{
			None = 0x00000000,
			Attributes = 0x00000001,
			WeaponAttributes = 0x00000002,
			SkillBonuses = 0x00000004,
			Hue = 0x00000008,
			LastEquipped = 0x00000010,
			SetEquipped = 0x00000020,
			SetSelfRepair = 0x00000040,
			PhysicalBonus = 0x00000080,
			FireBonus = 0x00000100,
			ColdBonus = 0x00000200,
			PoisonBonus = 0x00000400,
			EnergyBonus = 0x00000800,
		}
		#endregion

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_WeaponEffect = (WeaponEffect)reader.ReadEncodedInt();
						m_Charges = reader.ReadInt();

						m_Slayer3 = (TalismanSlayerName)reader.ReadInt();

						#region SetItems
						SetFlag sflags = (SetFlag)reader.ReadEncodedInt();
						if (sflags.HasFlag(SetFlag.PhysicalBonus))
						{
							m_SetPhysicalBonus = reader.ReadEncodedInt();
						}

						if (sflags.HasFlag(SetFlag.FireBonus))
						{
							m_SetFireBonus = reader.ReadEncodedInt();
						}

						if (sflags.HasFlag(SetFlag.ColdBonus))
						{
							m_SetColdBonus = reader.ReadEncodedInt();
						}

						if (sflags.HasFlag(SetFlag.PoisonBonus))
						{
							m_SetPoisonBonus = reader.ReadEncodedInt();
						}

						if (sflags.HasFlag(SetFlag.EnergyBonus))
						{
							m_SetEnergyBonus = reader.ReadEncodedInt();
						}

						if (sflags.HasFlag(SetFlag.Attributes))
						{
							m_SetAttributes = new AosAttributes(this, reader);
						}
						else
						{
							m_SetAttributes = new AosAttributes(this);
						}

						if (sflags.HasFlag(SetFlag.WeaponAttributes))
						{
							m_SetSelfRepair = (new AosWeaponAttributes(this, reader)).SelfRepair;
						}

						if (sflags.HasFlag(SetFlag.SkillBonuses))
						{
							m_SetSkillBonuses = new AosSkillBonuses(this, reader);
						}
						else
						{
							m_SetSkillBonuses = new AosSkillBonuses(this);
						}

						if (sflags.HasFlag(SetFlag.Hue))
						{
							m_SetHue = reader.ReadInt();
						}

						if (sflags.HasFlag(SetFlag.LastEquipped))
						{
							m_LastEquipped = reader.ReadBool();
						}

						if (sflags.HasFlag(SetFlag.SetEquipped))
						{
							m_SetEquipped = reader.ReadBool();
						}

						if (sflags.HasFlag(SetFlag.SetSelfRepair))
						{
							m_SetSelfRepair = reader.ReadEncodedInt();
						}
						#endregion
						//SaveFlag flags;

						SaveFlag flags = (SaveFlag)reader.ReadInt();

						if (flags.HasFlag(SaveFlag.DamageLevel))
						{
							m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

							if (m_DamageLevel > WeaponDamageLevel.Vanq)
								m_DamageLevel = WeaponDamageLevel.Ruin;
						}

						if (flags.HasFlag(SaveFlag.AccuracyLevel))
						{
							m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

							if (m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
								m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
						}

						if (flags.HasFlag(SaveFlag.DurabilityLevel))
						{
							m_DurabilityLevel = (DurabilityLevel)reader.ReadInt();

							if (m_DurabilityLevel > DurabilityLevel.Indestructible)
								m_DurabilityLevel = DurabilityLevel.Durable;
						}

						if (flags.HasFlag(SaveFlag.Hits))
							m_Hits = reader.ReadInt();

						if (flags.HasFlag(SaveFlag.MaxHits))
							m_MaxHits = reader.ReadInt();

						if (flags.HasFlag(SaveFlag.Slayer))
							m_Slayer = (SlayerName)reader.ReadInt();

						if (flags.HasFlag(SaveFlag.Poison))
							m_Poison = Poison.Deserialize(reader);

						if (flags.HasFlag(SaveFlag.PoisonCharges))
							m_PoisonCharges = reader.ReadInt();

						if (flags.HasFlag(SaveFlag.StrReq))
							m_StrReq = reader.ReadInt();
						else
							m_StrReq = -1;

						if (flags.HasFlag(SaveFlag.DexReq))
							m_DexReq = reader.ReadInt();
						else
							m_DexReq = -1;

						if (flags.HasFlag(SaveFlag.IntReq))
							m_IntReq = reader.ReadInt();
						else
							m_IntReq = -1;

						if (flags.HasFlag(SaveFlag.MinDamage))
							m_MinDamage = reader.ReadInt();
						else
							m_MinDamage = -1;

						if (flags.HasFlag(SaveFlag.MaxDamage))
							m_MaxDamage = reader.ReadInt();
						else
							m_MaxDamage = -1;

						if (flags.HasFlag(SaveFlag.HitSound))
							m_HitSound = reader.ReadInt();
						else
							m_HitSound = -1;

						if (flags.HasFlag(SaveFlag.MissSound))
							m_MissSound = reader.ReadInt();
						else
							m_MissSound = -1;

						if (flags.HasFlag(SaveFlag.Speed))
						{
							m_Speed = reader.ReadFloat();
						}
						else
							m_Speed = -1;

						if (flags.HasFlag(SaveFlag.MaxRange))
							m_MaxRange = reader.ReadInt();
						else
							m_MaxRange = -1;

						if (flags.HasFlag(SaveFlag.Skill))
							m_Skill = (SkillName)reader.ReadInt();
						else
							m_Skill = (SkillName)(-1);

						if (flags.HasFlag(SaveFlag.Type))
							m_Type = (WeaponType)reader.ReadInt();
						else
							m_Type = (WeaponType)(-1);

						if (flags.HasFlag(SaveFlag.Animation))
							m_Animation = (WeaponAnimation)reader.ReadInt();
						else
							m_Animation = (WeaponAnimation)(-1);

						if (flags.HasFlag(SaveFlag.xWeaponAttributes))
							m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						else
							m_AosWeaponAttributes = new AosWeaponAttributes(this);

						if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile parentMob)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							parentMob.AddSkillMod(m_SkillMod);
						}

						if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30 && Parent is Mobile parentMobile)
						{
							m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
							parentMobile.AddSkillMod(m_MageMod);
						}

						if (flags.HasFlag(SaveFlag.SkillBonuses))
							m_AosSkillBonuses = new AosSkillBonuses(this, reader);
						else
							m_AosSkillBonuses = new AosSkillBonuses(this);

						if (flags.HasFlag(SaveFlag.Slayer2))
							m_Slayer2 = (SlayerName)reader.ReadInt();

						if (flags.HasFlag(SaveFlag.ElementalDamages))
							m_AosElementDamages = new AosElementAttributes(this, reader);
						else
							m_AosElementDamages = new AosElementAttributes(this);

						break;
					}
			}

			#region Mondain's Legacy Sets
			if (m_SetAttributes == null)
			{
				m_SetAttributes = new AosAttributes(this);
			}

			if (m_SetSkillBonuses == null)
			{
				m_SetSkillBonuses = new AosSkillBonuses(this);
			}
			#endregion

			if (Core.AOS && Parent is Mobile mobile)
				m_AosSkillBonuses.AddTo(mobile);

			int strBonus = Attributes.BonusStr;
			int dexBonus = Attributes.BonusDex;
			int intBonus = Attributes.BonusInt;

			if (Parent is Mobile m && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
			{
				string modName = Serial.ToString();

				if (strBonus != 0)
					m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

				if (dexBonus != 0)
					m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

				if (intBonus != 0)
					m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			}

			if (Parent is Mobile mob)
				mob.CheckStatTimers();

			if (m_Hits <= 0 && m_MaxHits <= 0)
			{
				m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);
			}
		}
		#endregion

		public virtual CraftResource DefaultResource => CraftResource.Iron;

		public BaseWeapon(int itemID) : base(itemID)
		{
			Layer = (Layer)ItemData.Quality;
			m_StrReq = -1;
			m_DexReq = -1;
			m_IntReq = -1;
			m_MinDamage = -1;
			m_MaxDamage = -1;
			m_HitSound = -1;
			m_MissSound = -1;
			m_Speed = -1;
			m_MaxRange = -1;
			m_Skill = (SkillName)(-1);
			m_Type = (WeaponType)(-1);
			m_Animation = (WeaponAnimation)(-1);
			m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);
			base.Resource = DefaultResource;
			Hue = CraftResources.GetHue(Resource);
			m_AosWeaponAttributes = new AosWeaponAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
			m_AosElementDamages = new AosElementAttributes(this);
			m_SetAttributes = new AosAttributes(this);
			m_SetSkillBonuses = new AosSkillBonuses(this);
			// Xml Spawner XmlSockets - SOF
			// mod to randomly add sockets and socketability features to armor. These settings will yield
			// 2% drop rate of socketed/socketable items
			// 0.1% chance of 5 sockets
			// 0.5% of 4 sockets
			// 3% chance of 3 sockets
			// 15% chance of 2 sockets
			// 50% chance of 1 socket
			// the remainder will be 0 socket (31.4% in this case)
			// uncomment the next line to prevent artifacts from being socketed
			// if(ArtifactRarity == 0)
			//XmlSockets.ConfigureRandom(this, 2.0, 0.1, 0.5, 3.0, 15.0, 50.0);
			// Xml Spawner XmlSockets - EOF
		}

		public BaseWeapon(Serial serial) : base(serial)
		{
		}

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get => base.Hue;
			set { base.Hue = value; InvalidateProperties(); }
		}

		public int GetElementalDamageHue()
		{
			GetDamageTypes(null, out _, out int fire, out int cold, out int pois, out int nrgy, out _, out _);
			//Order is Cold, Energy, Fire, Poison, Physical left

			int currentMax = 50;
			int hue = 0;

			if (pois >= currentMax)
			{
				hue = 1267 + (pois - 50) / 10;
				currentMax = pois;
			}

			if (fire >= currentMax)
			{
				hue = 1255 + (fire - 50) / 10;
				currentMax = fire;
			}

			if (nrgy >= currentMax)
			{
				hue = 1273 + (nrgy - 50) / 10;
				currentMax = nrgy;
			}

			if (cold >= currentMax)
			{
				hue = 1261 + (cold - 50) / 10;
				currentMax = cold;
			}

			return hue;
		}

		public override string BuildSingleClick()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			if (AppendLootType(sb))
				sb.Append(", ");

			if (Quality == ItemQuality.Exceptional)
				sb.Append("exceptional, ");

			if (IsMagic)
				sb.Append("magic, ");

			if (m_PoisonCharges > 0 && m_Poison != null)
				sb.Append("poisoned, ");

			if (sb.Length > 2)
				sb.Remove(sb.Length - 2, 1); // remove the last comma

			AppendClickName(sb);
			InsertNamePrefix(sb);

			/*if ( m_Crafter != null && !m_Crafter.Deleted )
				sb.AppendFormat( " (crafted by {0})", m_Crafter.Name );
			*/
			InstertHtml(sb);


			return sb.ToString();
		}

		public virtual string BuildMagicSingleClick()
		{
			StringBuilder sb = new StringBuilder();
			if (Name == null || Name.Length <= 0)
			{
				if (AppendLootType(sb))
					sb.Append(", ");

				if (Quality == ItemQuality.Exceptional)
					sb.Append("exceptional, ");

				if (m_DurabilityLevel != DurabilityLevel.Regular)
					sb.AppendFormat("{0}, ", m_DurabilityLevel.ToString().ToLower());

				if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
				{
					if (m_AccuracyLevel != WeaponAccuracyLevel.Accurate)
					{
						sb.Append(m_AccuracyLevel.ToString().ToLower());
						sb.Append(' ');
					}
					sb.Append("accurate, ");
				}

				if (m_Slayer == SlayerName.Silver)
					sb.Append("silver, ");

				if (m_PoisonCharges > 0 && m_Poison != null)
					sb.Append("poisoned, ");

				if (sb.Length > 2)
					sb.Remove(sb.Length - 2, 1); // remove the last comma


				AppendClickName(sb);
				InsertNamePrefix(sb);


				if (m_DamageLevel > WeaponDamageLevel.Regular)
				{
					sb.Append(" of ");
					if (m_DamageLevel == WeaponDamageLevel.Vanq)
						sb.Append("vanquishing");
					else
						sb.Append(m_DamageLevel.ToString().ToLower());
				}

				/*if ( m_Effect != SpellEffect.None && m_EffectCharges > 0 )
				{
					if ( m_DamageLevel > WeaponDamageLevel.Regular )
						sb.Append( " and " );
					else
						sb.Append( " of " );
					sb.Append( SpellCastEffect.GetName( m_Effect ) );
					sb.AppendFormat( " with {0} charge{1}", m_EffectCharges, m_EffectCharges != 1 ? "s" : "" );
				}*/

				//if ( m_Crafter != null && !m_Crafter.Deleted )
				//	sb.AppendFormat( " (crafted by {0})", m_Crafter.Name );

				InstertHtml(sb);
			}
			else
			{
				AppendClickName(sb);
				InstertHtml(sb);
			}

			return sb.ToString();
		}

		public override bool AllowEquipedCast(Mobile from)
		{
			if (base.AllowEquipedCast(from))
			{
				return true;
			}

			return Attributes.SpellChanneling > 0;
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			if (Core.AOS)
			{
				int oreType = CraftResources.GetResourceLabel(Resource);

				if (oreType != 0)
					list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
				else if (Name == null)
					list.Add(LabelNumber);
				else
					list.Add(Name);

				/*
				 * Want to move this to the engraving tool, let the non-harmful
				 * formatting show, and remove CLILOCs embedded: more like OSI
				 * did with the books that had markup, etc.
				 *
				 * This will have a negative effect on a few event things imgame
				 * as is.
				 *
				 * If we cant find a more OSI-ish way to clean it up, we can
				 * easily put this back, and use it in the deserialize
				 * method and engraving tool, to make it perm cleaned up.
				 */

				if (!string.IsNullOrEmpty(EngravedText))
					list.Add(1062613, EngravedText);

				/* list.Add( 1062613, Utility.FixHtml( m_EngravedText ) ); */
			}
			else
			{
				if (!IsMagic || !Identified)
				{
					list.Add(BuildSingleClick());
				}
				else
					list.Add(BuildMagicSingleClick());
			}
		}

		public override int GetLuckBonus()
		{
			if (Core.ML && Resource == CraftResource.Heartwood)
			{
				return 0;
			}

			CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

			if (resInfo == null)
				return 0;

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if (attrInfo == null)
				return 0;

			return attrInfo.WeaponLuck;
		}

		public override void AddCraftedProperties(ObjectPropertyList list)
		{
			if (OwnerName != null)
			{
				list.Add(1153213, OwnerName);
			}

			if (Crafter != null)
			{
				list.Add(1050043, Crafter.TitleName); // crafted by ~1_NAME~
				//list.Add(1050043, Crafter.Name); // crafted by ~1_NAME~
			}

			if (Quality == ItemQuality.Exceptional)
			{
				list.Add(1060636); // Exceptional
			}

			if (Altered)
			{
				list.Add(1111880); // Altered
			}
		}

		public override void AddWeightProperty(ObjectPropertyList list)
		{
			base.AddWeightProperty(list);
		}

		public override void AddUsesRemainingProperties(ObjectPropertyList list)
		{
			//if (ShowUsesRemaining)
			//{
			//	list.Add(1060584, UsesRemaining.ToString()); // uses remaining: ~1_val~
			//}

			if (this is IUsesRemaining remaining && remaining.ShowUsesRemaining)
				list.Add(1060584, remaining.UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			if (Core.AOS)
			{
				#region Factions
				if (FactionItemState != null)
					list.Add(1041350); // faction item
				#endregion

				#region Mondain's Legacy Sets
				if (IsSetItem)
				{
					list.Add(1073491, Pieces.ToString()); // Part of a Weapon/Armor Set (~1_val~ pieces)

					if (m_SetEquipped)
					{
						list.Add(1073492); // Full Weapon/Armor Set Present
						GetSetProperties(list);
					}
				}
				#endregion

				//if (m_ExtendedWeaponAttributes.Focus > 0)
				//{
				//	list.Add(1150018); // Focus
				//}

				//if (m_NegativeAttributes.Brittle == 0 && m_AosAttributes.Brittle != 0)
				//{
				//	list.Add(1116209); // Brittle
				//}

				//if (m_NegativeAttributes != null)
				//{
				//	m_NegativeAttributes.GetProperties(list, this);
				//}

				if (m_AosSkillBonuses != null)
				{
					m_AosSkillBonuses.GetProperties(list);
				}

				if (RequiredRace == Race.Elf)
				{
					list.Add(1075086); // Elves Only
				}

				if (ArtifactRarity > 0)
				{
					list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
				}

				if (m_Poison != null && m_PoisonCharges > 0 && CanShowPoisonCharges())
				{
					#region Mondain's Legacy mod
					list.Add(m_Poison.LabelNumber, m_PoisonCharges.ToString());
					#endregion
				}

				if (m_Slayer != SlayerName.None)
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
					if (entry != null)
					{
						list.Add(entry.Title);
					}
				}

				if (m_Slayer2 != SlayerName.None)
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
					if (entry != null)
					{
						list.Add(entry.Title);
					}
				}

				#region Mondain's Legacy
				if (m_Slayer3 != TalismanSlayerName.None)
				{
					if (m_Slayer3 == TalismanSlayerName.Wolf)
					{
						list.Add(1075462);
					}
					else if (m_Slayer3 == TalismanSlayerName.Goblin)
					{
						list.Add(1095010);
					}
					else if (m_Slayer3 == TalismanSlayerName.Undead)
					{
						list.Add(1060479);
					}
					else
					{
						list.Add(1072503 + (int)m_Slayer3);
					}
				}
				#endregion

				double focusBonus = 1;
				int enchantBonus = 0;
				bool fcMalus = false;
				int damBonus = 0;
				AosWeaponAttribute bonus = AosWeaponAttribute.HitColdArea;

				#region Focus Attack
				if (FocusWeilder != null)
				{
					SpecialMove move = SpecialMove.GetCurrentMove(FocusWeilder);

					if (move is FocusAttack)
					{
						focusBonus = move.GetPropertyBonus(FocusWeilder);
						damBonus = (int)(move.GetDamageScalar(FocusWeilder, null) * 100) - 100;
					}
				}
				#endregion

				int prop;
				double fprop;

				if ((prop = m_AosWeaponAttributes.DurabilityBonus) != 0)
				{
					list.Add(1151780, prop.ToString()); // durability +~1_VAL~%
				}

				if ((prop = m_AosWeaponAttributes.SplinteringWeapon) != 0)
				{
					list.Add(1112857, prop.ToString()); //splintering weapon ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitDispel * focusBonus) != 0)
				{
					list.Add(1060417, ((int)fprop).ToString()); // hit dispel ~1_val~%
				}
				else if (bonus == AosWeaponAttribute.HitDispel && enchantBonus != 0)
				{
					list.Add(1060417, ((int)(enchantBonus * focusBonus)).ToString()); // hit dispel ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitFireball * focusBonus) != 0)
				{
					list.Add(1060420, ((int)fprop).ToString()); // hit fireball ~1_val~%
				}
				else if (bonus == AosWeaponAttribute.HitFireball && enchantBonus != 0)
				{
					list.Add(1060420, ((int)(enchantBonus * focusBonus)).ToString()); // hit fireball ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitLightning * focusBonus) != 0)
				{
					list.Add(1060423, ((int)fprop).ToString()); // hit lightning ~1_val~%
				}
				else if (bonus == AosWeaponAttribute.HitLightning && enchantBonus != 0)
				{
					list.Add(1060423, ((int)(enchantBonus * focusBonus)).ToString()); // hit lightning ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitHarm * focusBonus) != 0)
				{
					list.Add(1060421, ((int)fprop).ToString()); // hit harm ~1_val~%
				}
				else if (bonus == AosWeaponAttribute.HitHarm && enchantBonus != 0)
				{
					list.Add(1060421, ((int)(enchantBonus * focusBonus)).ToString()); // hit harm ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitMagicArrow * focusBonus) != 0)
				{
					list.Add(1060426, ((int)fprop).ToString()); // hit magic arrow ~1_val~%
				}
				else if (bonus == AosWeaponAttribute.HitMagicArrow && enchantBonus != 0)
				{
					list.Add(1060426, ((int)(enchantBonus * focusBonus)).ToString()); // hit magic arrow ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitPhysicalArea * focusBonus) != 0)
				{
					list.Add(1060428, ((int)fprop).ToString()); // hit physical area ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitFireArea * focusBonus) != 0)
				{
					list.Add(1060419, ((int)fprop).ToString()); // hit fire area ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitColdArea * focusBonus) != 0)
				{
					list.Add(1060416, ((int)fprop).ToString()); // hit cold area ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitPoisonArea * focusBonus) != 0)
				{
					list.Add(1060429, ((int)fprop).ToString()); // hit poison area ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitEnergyArea * focusBonus) != 0)
				{
					list.Add(1060418, ((int)fprop).ToString()); // hit energy area ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitLeechStam * focusBonus) != 0)
				{
					list.Add(1060430, Math.Min(100, (int)fprop).ToString()); // hit stamina leech ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitLeechMana * focusBonus) != 0)
				{
					list.Add(1060427, Math.Min(100, (int)fprop).ToString()); // hit mana leech ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitLeechHits * focusBonus) != 0)
				{
					list.Add(1060422, Math.Min(100, (int)fprop).ToString()); // hit life leech ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitFatigue * focusBonus) != 0)
				{
					list.Add(1113700, ((int)fprop).ToString()); // Hit Fatigue ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitManaDrain * focusBonus) != 0)
				{
					list.Add(1113699, ((int)fprop).ToString()); // Hit Mana Drain ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitCurse * focusBonus) != 0)
				{
					list.Add(1113712, ((int)fprop).ToString()); // Hit Curse ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitLowerAttack * focusBonus) != 0)
				{
					list.Add(1060424, ((int)fprop).ToString()); // hit lower attack ~1_val~%
				}

				if ((fprop = m_AosWeaponAttributes.HitLowerDefend * focusBonus) != 0)
				{
					list.Add(1060425, ((int)fprop).ToString()); // hit lower defense ~1_val~%
				}

				if ((prop = m_AosWeaponAttributes.BloodDrinker) != 0)
				{
					list.Add(1113591, prop.ToString()); // Blood Drinker
				}

				if ((prop = m_AosWeaponAttributes.BattleLust) != 0)
				{
					list.Add(1113710, prop.ToString()); // Battle Lust
				}

				if (ImmolatingWeaponSpell.IsImmolating(RootParent as Mobile, this))
				{
					list.Add(1111917); // Immolated
				}

				if (Core.ML && this is BaseRanged ranged && (prop = ranged.Velocity) != 0)
				{
					list.Add(1072793, prop.ToString()); // Velocity ~1_val~%
				}

				if ((prop = Attributes.LowerAmmoCost) != 0)
				{
					list.Add(1075208, prop.ToString()); // Lower Ammo Cost ~1_Percentage~%
				}

				if ((prop = m_AosWeaponAttributes.SelfRepair) != 0)
				{
					list.Add(1060450, prop.ToString()); // self repair ~1_val~
				}

				if ((Attributes.NightSight) != 0)
				{
					list.Add(1060441); // night sight
				}

				if ((fcMalus ? 1 : Attributes.SpellChanneling) != 0)
				{
					list.Add(1060482); // spell channeling
				}

				if ((prop = m_AosWeaponAttributes.MageWeapon) != 0)
				{
					list.Add(1060438, (30 - prop).ToString()); // mage weapon -~1_val~ skill
				}

				if (Core.ML && Attributes.BalancedWeapon > 0 && Layer == Layer.TwoHanded)
				{
					list.Add(1072792); // Balanced
				}

				if ((prop = GetLuckBonus() + Attributes.Luck) != 0)
				{
					list.Add(1060436, prop.ToString()); // luck ~1_val~
				}

				if ((prop = Attributes.EnhancePotions) != 0)
				{
					list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%
				}

				if ((m_AosWeaponAttributes.ReactiveParalyze) != 0)
				{
					list.Add(1112364); // reactive paralyze
				}

				if ((prop = Attributes.BonusStr) != 0)
				{
					list.Add(1060485, prop.ToString()); // strength bonus ~1_val~
				}

				if ((prop = Attributes.BonusInt) != 0)
				{
					list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~
				}

				if ((prop = Attributes.BonusDex) != 0)
				{
					list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~
				}

				if ((prop = Attributes.BonusHits) != 0)
				{
					list.Add(1060431, prop.ToString()); // hit point increase ~1_val~
				}

				if ((prop = Attributes.BonusStam) != 0)
				{
					list.Add(1060484, prop.ToString()); // stamina increase ~1_val~
				}

				if ((prop = Attributes.BonusMana) != 0)
				{
					list.Add(1060439, prop.ToString()); // mana increase ~1_val~
				}

				if ((prop = Attributes.RegenHits) != 0)
				{
					list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~
				}

				if ((prop = Attributes.RegenStam) != 0)
				{
					list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~
				}

				if ((prop = Attributes.RegenMana) != 0)
				{
					list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~
				}

				if ((prop = Attributes.ReflectPhysical) != 0)
				{
					list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%
				}

				if ((prop = Attributes.SpellDamage) != 0)
				{
					list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%
				}

				if ((prop = Attributes.CastRecovery) != 0)
				{
					list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~
				}

				if ((prop = fcMalus ? Attributes.CastSpeed - 1 : Attributes.CastSpeed) != 0)
				{
					list.Add(1060413, prop.ToString()); // faster casting ~1_val~
				}

				if ((prop = (GetHitChanceBonus() + Attributes.AttackChance)) != 0)
				{
					list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%
				}

				if ((prop = Attributes.DefendChance) != 0)
				{
					list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%
				}

				if ((prop = Attributes.LowerManaCost) != 0)
				{
					list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%
				}

				if ((prop = Attributes.LowerRegCost) != 0)
				{
					list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%
				}

				if ((prop = Attributes.WeaponSpeed) != 0)
				{
					list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%
				}

				if ((prop = (GetDamageBonus() + Attributes.WeaponDamage + damBonus)) != 0)
				{
					list.Add(1060401, prop.ToString()); // damage increase ~1_val~%
				}

				if (Core.ML && (prop = Attributes.IncreasedKarmaLoss) != 0)
				{
					list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%
				}

				base.AddResistanceProperties(list);

				if ((prop = GetLowerStatReq()) != 0)
				{
					list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%
				}

				if ((m_AosWeaponAttributes.UseBestSkill) != 0)
				{
					list.Add(1060400); // use best weapon skill
				}


				GetDamageTypes(null, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct);

				#region Mondain's Legacy
				if (chaos != 0)
				{
					list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%
				}

				if (direct != 0)
				{
					list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%
				}
				#endregion

				if (phys != 0)
				{
					list.Add(1060403, phys.ToString()); // physical damage ~1_val~%
				}

				if (fire != 0)
				{
					list.Add(1060405, fire.ToString()); // fire damage ~1_val~%
				}

				if (cold != 0)
				{
					list.Add(1060404, cold.ToString()); // cold damage ~1_val~%
				}

				if (pois != 0)
				{
					list.Add(1060406, pois.ToString()); // poison damage ~1_val~%
				}

				if (nrgy != 0)
				{
					list.Add(1060407, nrgy.ToString()); // energy damage ~1_val
				}

				if (Core.ML && chaos != 0)
				{
					list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%
				}

				if (Core.ML && direct != 0)
				{
					list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%
				}

				list.Add(1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~

				if (Core.ML)
				{
					list.Add(1061167, $"{Speed}s"); // weapon speed ~1_val~
				}
				else
				{
					list.Add(1061167, Speed.ToString());
				}

				if (MaxRange > 1)
				{
					list.Add(1061169, MaxRange.ToString()); // range ~1_val~
				}

				int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());

				if (strReq > 0)
				{
					list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~
				}

				if (Layer == Layer.TwoHanded)
				{
					list.Add(1061171); // two-handed weapon
				}
				else
				{
					list.Add(1061824); // one-handed weapon
				}

				if (Core.SE || m_AosWeaponAttributes.UseBestSkill == 0)
				{
					switch (Skill)
					{
						case SkillName.Swords:
							list.Add(1061172);
							break; // skill required: swordsmanship
						case SkillName.Macing:
							list.Add(1061173);
							break; // skill required: mace fighting
						case SkillName.Fencing:
							list.Add(1061174);
							break; // skill required: fencing
						case SkillName.Archery:
							list.Add(1061175);
							break; // skill required: archery
						case SkillName.Throwing:
							list.Add(1112075); // skill required: throwing
							break;
					}
				}

				// Xml Spawner 2.36c XmlLevelItem - SOF
				//XmlLevelItem levitem = XmlAttach.FindAttachment(this, typeof(XmlLevelItem)) as XmlLevelItem;

				//if (levitem != null)
				//{
				//	list.Add(1060658, "Level\t{0}", levitem.Level);

				//	if (LevelItems.DisplayExpProp)
				//		list.Add(1060659, "Experience\t{0}", levitem.Experience);
					// Xml Spawner 2.36c XmlLevelItem - EOF
				//}

				XmlAttach.AddAttachmentProperties(this, list);

				if (m_Hits >= 0 && m_MaxHits > 0)
				{
					list.Add(1060639, "{0}\t{1}", m_Hits, m_MaxHits); // durability ~1_val~ / ~2_val~
				}

				if (IsSetItem && !m_SetEquipped)
				{
					list.Add(1072378); // <br>Only when full set is present:
					GetSetProperties(list);
				}
			}
			else
			{
				StringBuilder sr = new StringBuilder();

				if (Layer == Layer.TwoHanded)
					sr.Append("Two-Handed ");
				else
					sr.Append("One-Handed ");

				switch (Skill)
				{
					case SkillName.Swords: sr.Append("Sword"); break; // skill required: swordsmanship
					case SkillName.Macing: sr.Append("Mace"); break; // skill required: mace fighting
					case SkillName.Fencing: sr.Append("Fencing weapon"); break; // skill required: fencing
					case SkillName.Archery: sr.Append("Ranged weapon"); break; // skill required: archery
				}

				sr.AppendLine();
				sr.AppendFormat("Damage: {0} - {1}", MinDamage.ToString(), MaxDamage.ToString());

				if (GetDamageBonus() != 0)
				{
					if (m_DamageLevel != WeaponDamageLevel.Regular)
						sr.AppendFormat("<BASEFONT COLOR={0}> (+{1})<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Magic), GetDamageBonus());
					else if (Quality == ItemQuality.Exceptional)
						sr.AppendFormat("<BASEFONT COLOR={0}> (+{1})<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Crafted), GetDamageBonus());
				}

				sr.AppendLine();

				sr.Append("Speed: ");

				if (Speed < 30)
					sr.Append("Very Slow");
				else if (Speed < 35)
					sr.Append("Slow");
				else if (Speed < 40)
					sr.Append("Avarage");
				else if (Speed < 51)
					sr.Append("Fast");
				else if (Speed < 100)
					sr.Append("Very Fast");

				if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
				{
					sr.AppendLine();
					sr.AppendFormat("<BASEFONT COLOR={0}>+{1} {2}<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Magic), (int)m_AccuracyLevel * 5, (Skill == SkillName.Archery ? "Archery" : "Tactics"));
				}

				if (m_Slayer == SlayerName.Silver)
				{
					sr.AppendLine();
					sr.AppendFormat("<BASEFONT COLOR={0}>Silver<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Magic));
				}

				if (m_Slayer != SlayerName.None && m_Slayer != SlayerName.Silver)
				{
					sr.AppendLine();
					SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
					if (entry != null)
						sr.AppendFormat("<BASEFONT COLOR={0}>{1}<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Magic), entry.Title); //doesn't work, pulls the cliloc number
				}

				if (m_Slayer2 != SlayerName.None)
				{
					sr.AppendLine();
					SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
					if (entry != null)
						sr.AppendFormat("<BASEFONT COLOR={0}>{1}<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Magic), entry.Title); //doesn't work, pulls the cliloc number
				}

				if (m_Poison != null && m_PoisonCharges > 0)
				{
					sr.AppendLine();
					sr.AppendFormat("<BASEFONT COLOR={0}>{1} Poison charges: {2}<BASEFONT COLOR=#FFFFFF>", ItemRankColor(ItemRank.Crafted), m_Poison, m_PoisonCharges);
				}

				list.Add(sr.ToString());

				if (MaxRange > 1)
					list.Add(1061169, MaxRange.ToString()); // range ~1_val~

				int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());

				if (strReq > 0)
					list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~

				string MaxDurability = m_MaxHits.ToString();

				if (Quality == ItemQuality.Exceptional)
					MaxDurability = $"<BASEFONT COLOR={ItemRankColor(ItemRank.Crafted)}>{m_MaxHits}<BASEFONT COLOR=#FFFFFF>";

				if (m_DurabilityLevel != DurabilityLevel.Regular)
					MaxDurability = $"<BASEFONT COLOR={ItemRankColor(ItemRank.Magic)}>{m_MaxHits}<BASEFONT COLOR=#FFFFFF>";

				if (m_Hits >= 0 && m_MaxHits > 0)
					list.Add(1060639, "{0}\t{1}", m_Hits, MaxDurability); // durability ~1_val~ / ~2_val~
			}
		}

		public override void AddItemPowerProperties(ObjectPropertyList list)
		{
		}

		public bool CanShowPoisonCharges()
		{
			if (PrimaryAbility == WeaponAbility.InfectiousStrike || SecondaryAbility == WeaponAbility.InfectiousStrike)
			{
				return true;
			}

			return true;
		}

		public bool IsMagic
		{
			get
			{
				return m_Slayer != SlayerName.None || m_DurabilityLevel != DurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular || m_AccuracyLevel != WeaponAccuracyLevel.Regular /*|| ( m_Effect != SpellEffect.None && m_EffectCharges > 0 )*/;
			}
		}

		/*public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Crafter != null)
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

			#region Factions
			if (m_FactionState != null)
				list.Add(1041350); // faction item
			#endregion

			if (m_AosSkillBonuses != null)
				m_AosSkillBonuses.GetProperties(list);

			if (Quality == ItemQuality.Exceptional)
				list.Add(1060636); // exceptional

			if (RequiredRace == Race.Elf)
				list.Add(1075086); // Elves Only

			if (ArtifactRarity > 0)
				list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~

			if (this is IUsesRemaining remaining && remaining.ShowUsesRemaining)
				list.Add(1060584, remaining.UsesRemaining.ToString()); // uses remaining: ~1_val~

			if (m_Poison != null && m_PoisonCharges > 0)
				list.Add(1062412 + m_Poison.Level, m_PoisonCharges.ToString());

			if (m_Slayer != SlayerName.None)
			{
				SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
				if (entry != null)
					list.Add(entry.Title);
			}

			if (m_Slayer2 != SlayerName.None)
			{
				SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
				if (entry != null)
					list.Add(entry.Title);
			}


			base.AddResistanceProperties(list);

			int prop;

			if (Core.ML && this is BaseRanged ranged && ranged.Balanced)
				list.Add(1072792); // Balanced

			if ((_ = m_AosWeaponAttributes.UseBestSkill) != 0)
				list.Add(1060400); // use best weapon skill

			if ((prop = (GetDamageBonus() + Attributes.WeaponDamage)) != 0)
				list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

			if ((prop = Attributes.DefendChance) != 0)
				list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

			if ((prop = Attributes.EnhancePotions) != 0)
				list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

			if ((prop = Attributes.CastRecovery) != 0)
				list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

			if ((prop = Attributes.CastSpeed) != 0)
				list.Add(1060413, prop.ToString()); // faster casting ~1_val~

			if ((prop = (GetHitChanceBonus() + Attributes.AttackChance)) != 0)
				list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitColdArea) != 0)
				list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitDispel) != 0)
				list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitEnergyArea) != 0)
				list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitFireArea) != 0)
				list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitFireball) != 0)
				list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitHarm) != 0)
				list.Add(1060421, prop.ToString()); // hit harm ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechHits) != 0)
				list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLightning) != 0)
				list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLowerAttack) != 0)
				list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLowerDefend) != 0)
				list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitMagicArrow) != 0)
				list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechMana) != 0)
				list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitPhysicalArea) != 0)
				list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitPoisonArea) != 0)
				list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechStam) != 0)
				list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%

			if (ImmolatingWeaponSpell.IsImmolating(this))
				list.Add(1111917); // Immolated

			if (Core.ML && this is BaseRanged ranged1 && (prop = ranged1.Velocity) != 0)
				list.Add(1072793, prop.ToString()); // Velocity ~1_val~%

			if ((prop = Attributes.BonusDex) != 0)
				list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

			if ((prop = Attributes.BonusHits) != 0)
				list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

			if ((prop = Attributes.BonusInt) != 0)
				list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

			if ((prop = Attributes.LowerManaCost) != 0)
				list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

			if ((prop = Attributes.LowerRegCost) != 0)
				list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

			if ((prop = GetLowerStatReq()) != 0)
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

			if ((prop = (GetLuckBonus() + Attributes.Luck)) != 0)
				list.Add(1060436, prop.ToString()); // luck ~1_val~

			if ((prop = m_AosWeaponAttributes.MageWeapon) != 0)
				list.Add(1060438, (30 - prop).ToString()); // mage weapon -~1_val~ skill

			if ((prop = Attributes.BonusMana) != 0)
				list.Add(1060439, prop.ToString()); // mana increase ~1_val~

			if ((prop = Attributes.RegenMana) != 0)
				list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

			if ((_ = Attributes.NightSight) != 0)
				list.Add(1060441); // night sight

			if ((prop = Attributes.ReflectPhysical) != 0)
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

			if ((prop = Attributes.RegenStam) != 0)
				list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

			if ((prop = Attributes.RegenHits) != 0)
				list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

			if ((prop = m_AosWeaponAttributes.SelfRepair) != 0)
				list.Add(1060450, prop.ToString()); // self repair ~1_val~

			if ((_ = Attributes.SpellChanneling) != 0)
				list.Add(1060482); // spell channeling

			if ((prop = Attributes.SpellDamage) != 0)
				list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

			if ((prop = Attributes.BonusStam) != 0)
				list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

			if ((prop = Attributes.BonusStr) != 0)
				list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

			if ((prop = Attributes.WeaponSpeed) != 0)
				list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

			if (Core.ML && (prop = Attributes.IncreasedKarmaLoss) != 0)
				list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%

			GetDamageTypes(null, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct);

			if (phys != 0)
				list.Add(1060403, phys.ToString()); // physical damage ~1_val~%

			if (fire != 0)
				list.Add(1060405, fire.ToString()); // fire damage ~1_val~%

			if (cold != 0)
				list.Add(1060404, cold.ToString()); // cold damage ~1_val~%

			if (pois != 0)
				list.Add(1060406, pois.ToString()); // poison damage ~1_val~%

			if (nrgy != 0)
				list.Add(1060407, nrgy.ToString()); // energy damage ~1_val

			if (Core.ML && chaos != 0)
				list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%

			if (Core.ML && direct != 0)
				list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%

			list.Add(1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~

			if (Core.ML)
				list.Add(1061167, string.Format("{0}s", Speed)); // weapon speed ~1_val~
			else
				list.Add(1061167, Speed.ToString());

			if (MaxRange > 1)
				list.Add(1061169, MaxRange.ToString()); // range ~1_val~

			int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());

			if (strReq > 0)
				list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~

			if (Layer == Layer.TwoHanded)
				list.Add(1061171); // two-handed weapon
			else
				list.Add(1061824); // one-handed weapon

			if (Core.SE || m_AosWeaponAttributes.UseBestSkill == 0)
			{
				switch (Skill)
				{
					case SkillName.Swords: list.Add(1061172); break; // skill required: swordsmanship
					case SkillName.Macing: list.Add(1061173); break; // skill required: mace fighting
					case SkillName.Fencing: list.Add(1061174); break; // skill required: fencing
					case SkillName.Archery: list.Add(1061175); break; // skill required: archery
				}
			}

			XmlAttach.AddAttachmentProperties(this, list);

			if (m_Hits >= 0 && m_MaxHits > 0)
				list.Add(1060639, "{0}\t{1}", m_Hits, m_MaxHits); // durability ~1_val~ / ~2_val~
		}*/

		public static BaseWeapon Fists { get; set; }

		#region ICraftable Members

		public int OnCraft(
			int quality,
			bool makersMark,
			Mobile from,
			CraftSystem craftSystem,
			Type typeRes,
			ITool tool,
			CraftItem craftItem,
			int resHue)
		{
			Quality = (ItemQuality)quality;

			if (makersMark)
				Crafter = from;

			PlayerConstructed = true;

			Type resourceType = typeRes;

			if (resourceType == null)
				resourceType = craftItem.Resources.GetAt(0).ItemType;

			if (Core.AOS)
			{
				Resource = CraftResources.GetFromType(resourceType);

				CraftContext context = craftSystem.GetContext(from);

				if (context != null && context.DoNotColor)
					Hue = 0;

				if (tool is BaseRunicTool runicTool)
					runicTool.ApplyAttributesTo(this);

				if (Quality == ItemQuality.Exceptional)
				{
					if (Attributes.WeaponDamage > 35)
						Attributes.WeaponDamage -= 20;
					else
						Attributes.WeaponDamage = 15;

					if (Core.ML)
					{
						Attributes.WeaponDamage += (int)(from.Skills.ArmsLore.Value / 20);

						if (Attributes.WeaponDamage > 50)
							Attributes.WeaponDamage = 50;

						_ = from.CheckSkill(SkillName.ArmsLore, 0, 100);
					}
				}
			}
			else if (tool is BaseRunicTool runicTool)
			{
				CraftResource thisResource = CraftResources.GetFromType(resourceType);

				if (thisResource == runicTool.Resource)
				{
					Resource = thisResource;

					CraftContext context = craftSystem.GetContext(from);

					if (context != null && context.DoNotColor)
						Hue = 0;

					switch (thisResource)
					{
						case CraftResource.DullCopper:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Durable;
								AccuracyLevel = WeaponAccuracyLevel.Accurate;
								break;
							}
						case CraftResource.ShadowIron:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Durable;
								DamageLevel = WeaponDamageLevel.Ruin;
								break;
							}
						case CraftResource.Copper:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Fortified;
								DamageLevel = WeaponDamageLevel.Ruin;
								AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
								break;
							}
						case CraftResource.Bronze:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Fortified;
								DamageLevel = WeaponDamageLevel.Might;
								AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
								break;
							}
						case CraftResource.Gold:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Force;
								AccuracyLevel = WeaponAccuracyLevel.Eminently;
								break;
							}
						case CraftResource.Agapite:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Power;
								AccuracyLevel = WeaponAccuracyLevel.Eminently;
								break;
							}
						case CraftResource.Verite:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Power;
								AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
								break;
							}
						case CraftResource.Valorite:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Vanq;
								AccuracyLevel = WeaponAccuracyLevel.Supremely;
								break;
							}
					}
				}
			}

			return quality;
		}

		#endregion

		public virtual void DistributeMaterialBonus(CraftAttributeInfo attrInfo)
		{
			if (Resource != CraftResource.Heartwood)
			{
				Attributes.WeaponDamage += attrInfo.WeaponDamage;
				Attributes.WeaponSpeed += attrInfo.WeaponSwingSpeed;
				Attributes.AttackChance += attrInfo.WeaponHitChance;
				Attributes.RegenHits += attrInfo.WeaponRegenHits;
				m_AosWeaponAttributes.HitLeechHits += attrInfo.WeaponHitLifeLeech;
			}
			else
			{
				switch (Utility.Random(6))
				{
					case 0: Attributes.WeaponDamage += attrInfo.WeaponDamage; break;
					case 1: Attributes.WeaponSpeed += attrInfo.WeaponSwingSpeed; break;
					case 2: Attributes.AttackChance += attrInfo.WeaponHitChance; break;
					case 3: Attributes.Luck += attrInfo.WeaponLuck; break;
					case 4: m_AosWeaponAttributes.LowerStatReq += attrInfo.WeaponLowerRequirements; break;
					case 5: m_AosWeaponAttributes.HitLeechHits += attrInfo.WeaponHitLifeLeech; break;
				}
			}
		}

		#region Mondain's Legacy Sets
		public override bool OnDragLift(Mobile from)
		{
			if (Parent is Mobile && from == Parent)
			{
				if (IsSetItem && m_SetEquipped)
				{
					SetHelper.RemoveSetBonus(from, SetID, this);
				}
			}

			return base.OnDragLift(from);
		}

		public virtual SetItem SetID => SetItem.None;
		public virtual int Pieces => 0;

		public virtual bool BardMasteryBonus => (SetID == SetItem.Virtuoso);

		public bool IsSetItem => SetID != SetItem.None;

		private int m_SetHue;
		private bool m_SetEquipped;
		private bool m_LastEquipped;

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetHue
		{
			get => m_SetHue;
			set
			{
				m_SetHue = value;
				InvalidateProperties();
			}
		}

		public bool SetEquipped { get => m_SetEquipped; set => m_SetEquipped = value; }

		public bool LastEquipped { get => m_LastEquipped; set => m_LastEquipped = value; }

		private AosAttributes m_SetAttributes;
		private AosSkillBonuses m_SetSkillBonuses;
		private int m_SetSelfRepair;
		private int m_SetPhysicalBonus, m_SetFireBonus, m_SetColdBonus, m_SetPoisonBonus, m_SetEnergyBonus;

		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes SetAttributes { get => m_SetAttributes; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SetSkillBonuses { get => m_SetSkillBonuses; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetSelfRepair
		{
			get => m_SetSelfRepair;
			set
			{
				m_SetSelfRepair = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetPhysicalBonus
		{
			get => m_SetPhysicalBonus;
			set
			{
				m_SetPhysicalBonus = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetFireBonus
		{
			get => m_SetFireBonus;
			set
			{
				m_SetFireBonus = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetColdBonus
		{
			get => m_SetColdBonus;
			set
			{
				m_SetColdBonus = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetPoisonBonus
		{
			get => m_SetPoisonBonus;
			set
			{
				m_SetPoisonBonus = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SetEnergyBonus
		{
			get => m_SetEnergyBonus;
			set
			{
				m_SetEnergyBonus = value;
				InvalidateProperties();
			}
		}

		public virtual void GetSetProperties(ObjectPropertyList list)
		{
			int prop;

			if ((prop = m_SetSelfRepair) != 0 && WeaponAttributes.SelfRepair == 0)
			{
				list.Add(1060450, prop.ToString()); // self repair ~1_val~
			}

			SetHelper.GetSetProperties(list, this);
		}

		public int SetResistBonus(ResistanceType resist)
		{
			switch (resist)
			{
				case ResistanceType.Physical: return PhysicalResistance;
				case ResistanceType.Fire: return FireResistance;
				case ResistanceType.Cold: return ColdResistance;
				case ResistanceType.Poison: return PoisonResistance;
				case ResistanceType.Energy: return EnergyResistance;
			}

			return 0;
		}
		#endregion
	}
}
