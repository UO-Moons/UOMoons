using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Second;
using Server.Spells.Spellweaving;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells;

public abstract class Spell : ISpell
{
	#region Gettters/Settters
	public int Id => SpellRegistry.GetRegistryNumber(this);
	public SpellState State { get; private set; }
	public Mobile Caster { get; }
	public object SpellTarget { get; private set; }
	public SpellInfo Info { get; }
	public string Name => Info.Name;
	public string Mantra => Info.Mantra;
	public Type[] Reagents => Info.Reagents;
	public Item Scroll { get; }
	private long StartCastTime { get; set; }
	public IDamageable InstantTarget { get; set; }
	public virtual SkillName CastSkill => SkillName.Magery;
	public virtual SkillName DamageSkill => SkillName.EvalInt;
	public virtual bool RevealOnCast => m_RevealOnCast;
	public virtual bool ClearHandsOnCast => m_ClearHandsOnCast;
	public virtual bool ShowHandMovement => m_ShowHandMovement;
	public virtual bool BlocksMovement => m_BlocksMovement;
	public virtual bool ConsumeRegs => m_ConsumeRegs;
	public virtual bool Precast => m_PreCast;
	public virtual int SpellRange => m_SpellRange;
	public virtual TargetFlags SpellTargetFlags => TargetFlags.None;
	//In reality, it's ANY delayed Damage spell Post-AoS that can't stack, but, only
	//Expo & Magic Arrow have enough delay and a short enough cast time to bring up
	//the possibility of stacking 'em.  Note that a MA & an Explosion will stack, but
	//of course, two MA's won't.
	public virtual Type[] DelayDamageFamily => null;
	public abstract TimeSpan CastDelayBase { get; }
	public virtual bool IsCasting => State == SpellState.Casting;
	public virtual bool CheckNextSpellTime => Scroll is not BaseWand;
	public virtual DamageType SpellDamageType => DamageType.Spell;
	#endregion

	#region Settings
	private static readonly TimeSpan NextSpellDelay = TimeSpan.FromSeconds(Settings.Configuration.Get<double>("Spells", "NextSpellDelay"));
	private static readonly TimeSpan AnimateDelay = TimeSpan.FromSeconds(1.5);
	private static readonly bool m_RevealOnCast = Settings.Configuration.Get<bool>("Spells", "RevealOnCast");
	private static readonly bool m_ClearHandsOnCast = Settings.Configuration.Get<bool>("Spells", "ClearHandsOnCast");
	private static readonly bool m_ShowHandMovement = Settings.Configuration.Get<bool>("Spells", "ShowHandMovement");
	private static readonly bool m_BlocksMovement = Settings.Configuration.Get<bool>("Spells", "BlocksMovement");
	private static readonly bool m_ConsumeRegs = Settings.Configuration.Get<bool>("Spells", "ConsumeRegs");
	private static readonly bool m_PreCast = Settings.Configuration.Get<bool>("Spells", "Precast");
	private static readonly int m_SpellRange = Settings.Configuration.Get("Spells", "SpellRange", Core.ML ? 10 : 12);
	private static readonly bool m_RequiredMana = Settings.Configuration.Get<bool>("Spells", "RequiredMana");
	public virtual bool CanTargetGround => false;
	public virtual bool RequireTarget => true;
	public virtual bool BlockedByHorrificBeast => true;
	public virtual bool BlockedByAnimalForm => true;
	public virtual bool DelayedDamage => false;
	public virtual bool DelayedDamageStacking => true;
	public virtual double CastDelayFastScalar => 1;
	public virtual double CastDelaySecondsPerTick => 0.25;
	public virtual TimeSpan CastDelayMinimum => TimeSpan.FromSeconds(0.25);
	#endregion

	private static readonly Dictionary<Type, DelayedDamageContextWrapper> m_ContextTable = new();

	private class DelayedDamageContextWrapper
	{
		private readonly Dictionary<IDamageable, Timer> _contexts = new();

		public void Add(IDamageable d, Timer t)
		{

			if (_contexts.TryGetValue(d, out Timer oldTimer))
			{
				oldTimer.Stop();
				_contexts.Remove(d);
			}

			_contexts.Add(d, t);
		}

		public bool Contains(IDamageable d)
		{
			return _contexts.ContainsKey(d);
		}

		public void Remove(IDamageable d)
		{
			_contexts.Remove(d);
		}
	}

	public void StartDelayedDamageContext(IDamageable d, Timer t)
	{
		if (DelayedDamageStacking)
		{
			return; //Sanity
		}

		if (!m_ContextTable.TryGetValue(GetType(), out DelayedDamageContextWrapper contexts))
		{
			contexts = new DelayedDamageContextWrapper();
			Type type = GetType();

			m_ContextTable.Add(type, contexts);

			if (DelayDamageFamily != null)
			{
				foreach (var familyType in DelayDamageFamily)
				{
					m_ContextTable.Add(familyType, contexts);
				}
			}
		}

		contexts.Add(d, t);
	}

	public bool HasDelayContext(IDamageable d)
	{
		if (DelayedDamageStacking)
		{
			return false; //Sanity
		}

		Type t = GetType();

		if (m_ContextTable.ContainsKey(t))
		{
			return m_ContextTable[t].Contains(d);
		}

		return false;
	}

	public void RemoveDelayedDamageContext(IDamageable d)
	{
		Type type = GetType();

		if (!m_ContextTable.TryGetValue(type, out DelayedDamageContextWrapper contexts))
		{
			return;
		}

		contexts.Remove(d);

		if (DelayDamageFamily != null)
		{
			foreach (var t in DelayDamageFamily)
			{
				if (m_ContextTable.TryGetValue(t, out contexts))
				{
					contexts.Remove(d);
				}
			}
		}
	}

	public void HarmfulSpell(IDamageable d)
	{
		switch (d)
		{
			case BaseMobile mobile:
				mobile.OnHarmfulSpell(Caster, this);
				break;
			case IDamageableItem item:
				item.OnHarmfulSpell(Caster);
				break;
		}

		NegativeAttributes.OnCombatAction(Caster);

		if (d is not Mobile mobile1)
			return;

		if (mobile1 != Caster)
		{
			NegativeAttributes.OnCombatAction(mobile1);
		}

		EvilOmenSpell.TryEndEffect(mobile1);
	}

	public Spell(Mobile caster, Item scroll, SpellInfo info)
	{
		Caster = caster;
		Scroll = scroll;
		Info = info;
	}

	public virtual int GetNewAosDamage(int bonus, int dice, int sides, IDamageable singleTarget)
	{
		return singleTarget != null ? GetNewAosDamage(bonus, dice, sides, Caster.Player && singleTarget is PlayerMobile, GetDamageScalar(singleTarget as Mobile), singleTarget) : GetNewAosDamage(bonus, dice, sides, false, null);
	}

	public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, IDamageable damageable)
	{
		return GetNewAosDamage(bonus, dice, sides, playerVsPlayer, 1.0, damageable);
	}

	/*public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, double scalar)
	{
		int damage = Utility.Dice(dice, sides, bonus) * 100;

		int damageBonus = Caster.GetSpellDamageBonus(playerVsPlayer);

		damage = AOS.Scale(damage, 100 + damageBonus);

		int evalSkill = GetDamageFixed(Caster);
		int evalScale = 30 + ((9 * evalSkill) / 100);

		damage = AOS.Scale(damage, evalScale);

		damage = AOS.Scale(damage, (int)(scalar * 100));

		return damage / 100;
	}*/

	public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, double scalar, IDamageable damageable)
	{
		Mobile target = damageable as Mobile;

		int damage = Utility.Dice(dice, sides, bonus) * 100;

		int inscribeSkill = GetInscribeFixed(Caster);
		int scribeBonus = inscribeSkill >= 1000 ? 10 : inscribeSkill / 200;

		int damageBonus = scribeBonus +
		                  Caster.Int / 10 +
		                  SpellHelper.GetSpellDamageBonus(Caster, target, CastSkill, playerVsPlayer);

		int evalSkill = GetDamageFixed(Caster);
		int evalScale = 30 + 9 * evalSkill / 100;

		damage = AOS.Scale(damage, evalScale);
		damage = AOS.Scale(damage, 100 + damageBonus);
		damage = AOS.Scale(damage, (int)(scalar * 100));

		return damage / 100;
	}

	public virtual void OnCasterHurt()
	{
		CheckCasterDisruption();
	}

	public virtual void CheckCasterDisruption(bool checkElem = false, int phys = 0, int fire = 0, int cold = 0,
		int pois = 0, int nrgy = 0)
	{
		if (!Caster.Player || Caster.AccessLevel > AccessLevel.Player)
		{
			return;
		}

		if (!IsCasting)
			return;

		object o = ProtectionSpell.Registry[Caster];
		bool disturb = true;

		if (o is double d)
		{
			if (d > Utility.RandomDouble() * 100.0)
			{
				disturb = false;
			}
		}

		int focus = SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.CastingFocus);

		//if (BaseFishPie.IsUnderEffects(Caster, FishPieEffect.CastFocus))
		//{
		//	focus += 2;
		//}

		if (focus > 12)
		{
			focus = 12;
		}

		focus += Caster.Skills[SkillName.Inscribe].Value >= 50 ? GetInscribeFixed(Caster) / 200 : 0;

		if (focus > 0 && focus > Utility.Random(100))
		{
			disturb = false;
			Caster.SendLocalizedMessage(1113690); // You regain your focus and continue casting the spell.
		}
		else if (checkElem)
		{
			int res = 0;

			if (phys == 100)
			{
				res = Math.Min(40, SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.ResonanceKinetic));
			}
			else if (fire == 100)
			{
				res = Math.Min(40, SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.ResonanceFire));
			}
			else if (cold == 100)
			{
				res = Math.Min(40, SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.ResonanceCold));
			}
			else if (pois == 100)
			{
				res = Math.Min(40, SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.ResonancePoison));
			}
			else if (nrgy == 100)
			{
				res = Math.Min(40, SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.ResonanceEnergy));
			}

			if (res > Utility.Random(100))
			{
				disturb = false;
			}
		}

		if (disturb)
		{
			Disturb(DisturbType.Hurt, false, true);
		}
	}

	/*public virtual void OnCasterHurt()
	{
		//Confirm: Monsters and pets cannot be disturbed.
		if (!Caster.Player)
			return;

		if (IsCasting)
		{
			object protectChance = ProtectionSpell.Registry[Caster];
			bool disturb = true;

			if (protectChance != null && protectChance is double prob)
			{
				if (prob > Utility.RandomDouble() * 100.0)
					disturb = false;
			}

			if (disturb)
				Disturb(DisturbType.Hurt, false, true);
		}
	}*/

	public virtual void OnCasterKilled()
	{
		Disturb(DisturbType.Kill);
	}

	public virtual void OnConnectionChanged()
	{
		FinishSequence();
	}

	/// <summary>
	/// Pre-ML code where mobile can change directions, but doesn't move
	/// </summary>
	/// <param name="d"></param>
	/// <returns></returns>
	public virtual bool OnCasterMoving(Direction d)
	{
		if (!IsCasting || !BlocksMovement)
			return true;

		Caster.SendLocalizedMessage(500111); // You are frozen and can not move.
		return false;

	}

	/// <summary>
	/// Post ML code where player is frozen in place while casting.
	/// </summary>
	/// <param name="caster"></param>
	/// <returns></returns>
	public virtual bool CheckMovement(Mobile caster)
	{
		return !IsCasting || !BlocksMovement || Caster is BaseCreature { FreezeOnCast: false };
	}

	public virtual bool OnCasterEquiping(Item item)
	{
		if (IsCasting)
			Disturb(DisturbType.EquipRequest);

		return true;
	}

	public virtual bool OnCasterUsingObject(object o)
	{
		if (State == SpellState.Sequencing)
			Disturb(DisturbType.UseRequest);

		return true;
	}

	public virtual bool OnCastInTown(Region r)
	{
		return Info.AllowTown;
	}

	public virtual bool ConsumeReagents()
	{
		if (Scroll != null || !Caster.Player)
			return true;

		if (!ConsumeRegs)
			return true;

		if (AosAttributes.GetValue(Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
			return true;

		if (Engines.ConPVP.DuelContext.IsFreeConsume(Caster))
			return true;

		Container pack = Caster.Backpack;

		if (pack == null)
			return false;

		return pack.ConsumeTotal(Info.Reagents, Info.Amounts) == -1;
	}

	public virtual double GetInscribeSkill(Mobile m)
	{
		return m.Skills[SkillName.Inscribe].Value;
	}

	public virtual int GetInscribeFixed(Mobile m)
	{
		// There is no chance to gain
		// m.CheckSkill( SkillName.Inscribe, 0.0, 120.0 );
		return m.Skills[SkillName.Inscribe].Fixed;
	}

	public virtual int GetDamageFixed(Mobile m)
	{
		return m.Skills[DamageSkill].Fixed;
	}

	public virtual double GetDamageSkill(Mobile m)
	{
		return m.Skills[DamageSkill].Value;
	}

	public virtual double GetResistSkill(Mobile m)
	{
		if (Core.AOS)
		{
			return m.Skills[SkillName.MagicResist].Value - EvilOmenSpell.GetResistMalus(m);
		}

		return m.Skills[SkillName.MagicResist].Value;
	}

	public virtual double GetDamageScalar(Mobile target)
	{
		double scalar = 1.0;

		if (!Core.AOS)  //EvalInt stuff for AoS is handled elsewhere
		{
			double casterEi = Caster.Skills[DamageSkill].Value;
			double targetRs = target.Skills[SkillName.MagicResist].Value;

			/*
			if( Core.AOS )
				targetRS = 0;
			*/

			//m_Caster.CheckSkill( DamageSkill, 0.0, 120.0 );

			if (casterEi > targetRs)
				scalar = (1.0 + ((casterEi - targetRs) / 500.0));
			else
				scalar = (1.0 + ((casterEi - targetRs) / 200.0));

			// magery damage bonus, -25% at 0 skill, +0% at 100 skill, +5% at 120 skill
			scalar += (Caster.Skills[CastSkill].Value - 100.0) / 400.0;

			if (!target.Player && !target.Body.IsHuman /*&& !Core.AOS*/ )
				scalar *= 2.0; // Double magery damage to monsters/animals if not AOS
		}

		if (target is BaseMobile creatureTarget)
			creatureTarget.AlterDamageScalarFrom(Caster, ref scalar);

		if (Caster is BaseMobile creatureCaster)
			creatureCaster.AlterDamageScalarTo(target, ref scalar);

		if (Core.SE)
			scalar *= GetSlayerDamageScalar(target);

		target.Region.SpellDamageScalar(Caster, target, ref scalar);

		if (Evasion.CheckSpellEvasion(target))  //Only single target spells an be evaded
			scalar = 0;

		return scalar;
	}

	public virtual double GetSlayerDamageScalar(Mobile defender)
	{
		Spellbook atkBook = Spellbook.FindEquippedSpellbook(Caster);

		double scalar = 1.0;
		if (atkBook != null)
		{
			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkBook.Slayer);
			SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkBook.Slayer2);

			if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
			{
				defender.FixedEffect(0x37B9, 10, 5);
				scalar = 2.0;
			}

			TransformContext context = TransformationSpellHelper.GetContext(defender);

			if ((atkBook.Slayer == SlayerName.Silver || atkBook.Slayer2 == SlayerName.Silver) && context != null && context.Type != typeof(HorrificBeastSpell))
				scalar += .25; // Every necromancer transformation other than horrific beast take an additional 25% damage

			if (Math.Abs(scalar - 1.0) > 1.0)
				return scalar;
		}

		ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender) ?? defender.Weapon as ISlayer;

		if (defISlayer == null)
			return scalar;

		SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
		SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

		if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(Caster) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(Caster))
			scalar = 2.0;

		return scalar;
	}

	public virtual void DoFizzle()
	{
		Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502632); // The spell fizzles.

		if (Caster.Player)
		{
			if (Core.AOS)
				Caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
			else
				Caster.FixedEffect(0x3735, 6, 30);

			Caster.PlaySound(0x5C);
		}
	}

	private CastTimer _castTimer;
	private AnimTimer _animTimer;

	public virtual bool CheckDisturb(DisturbType type, bool resistable)
	{
		return !resistable || Scroll is not BaseWand;
	}

	public void Disturb(DisturbType type, bool firstCircle = true, bool resistable = false)
	{
		if (!CheckDisturb(type, resistable))
			return;

		if (State == SpellState.Casting)
		{
			if (!firstCircle && !Core.AOS && this is MagerySpell {Circle: SpellCircle.First})
				return;

			State = SpellState.None;
			Caster.Spell = null;

			OnDisturb(type, true);

			_castTimer?.Stop();

			_animTimer?.Stop();

			if (Core.AOS && Caster.Player && type == DisturbType.Hurt)
				DoHurtFizzle();

			Caster.NextSpellTime = Core.TickCount + (int)GetDisturbRecovery().TotalMilliseconds;
		}
		else if (State == SpellState.Sequencing)
		{
			if (!firstCircle && !Core.AOS && this is MagerySpell {Circle: SpellCircle.First})
				return;

			State = SpellState.None;
			Caster.Spell = null;

			OnDisturb(type, false);

			Caster.Target?.Cancel(Caster);

			if (Core.AOS && Caster.Player && type == DisturbType.Hurt)
				DoHurtFizzle();
		}
	}

	public virtual void DoHurtFizzle()
	{
		Caster.FixedEffect(0x3735, 6, 30);
		Caster.PlaySound(0x5C);
	}

	public virtual void OnDisturb(DisturbType type, bool message)
	{
		if (type == DisturbType.Hurt && message)
			Caster.SendLocalizedMessage(500641); // Your concentration is disturbed, thus ruining thy spell.
	}

	public virtual bool CheckCast()
	{
		return true;
	}

	public virtual void SayMantra()
	{
		if (Scroll is BaseWand)
			return;

		if (Info.Mantra is {Length: > 0} && Caster.Player)
			Caster.PublicOverheadMessage(MessageType.Spell, Caster.SpeechHue, true, Info.Mantra, false);
	}

	public virtual bool Cast()
	{
		return Precast ? StartCast() : RequestSpellTarget();
	}

	public bool RequestSpellTarget()
	{
		if (Caster.Target != null)
		{
			Caster.Target.Cancel(Caster, TargetCancelType.Canceled);
		}
		else if (RequireTarget)
		{
			Caster.Target = new SpellRequestTarget(this);
		}
		else
		{
			SpellTargetCallback(Caster, Caster);
		}
		return true;
	}

	public void SpellTargetCallback(Mobile caster, object target)
	{
		if (caster != target)
			SpellHelper.Turn(Caster, target);

		if (Caster.Spell is {IsCasting: true})
		{
			((Spell)Caster.Spell).DoFizzle();
			Caster.Spell = null;
		}

		switch (SpellTargetFlags)
		{
			case TargetFlags.Harmful when target is Mobile harmfullTarget && !Caster.CanBeHarmful(harmfullTarget, false):
				Caster.SendLocalizedMessage(1001018); // You can not perform negative acts on your target.
				break;
			case TargetFlags.Beneficial when target is Mobile beneficialTarget && !Caster.CanBeBeneficial(beneficialTarget, false):
				Caster.SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.
				break;
			default:
				//Set the target
				SpellTarget = target;
				StartCast();
				break;
		}
	}

	private bool StartCast()
	{
		StartCastTime = Core.TickCount;

		if (Core.AOS && Caster.Spell is Spell {State: SpellState.Sequencing} spell)
			spell.Disturb(DisturbType.NewCast);

		if (!Caster.CheckAlive())
		{
			return false;
		}

		if (Scroll is BaseWand && Caster.Spell is {IsCasting: true})
		{
			Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
		}
		else if (Caster.Spell is {IsCasting: true})
		{
			Caster.SendLocalizedMessage(502642); // You are already casting a spell.
		}
		else if (BlockedByHorrificBeast && TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)) || (BlockedByAnimalForm && AnimalForm.UnderTransformation(Caster)))
		{
			Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
		}
		else if (Scroll is not BaseWand && (Caster.Paralyzed || Caster.Frozen))
		{
			Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
		}
		else if (CheckNextSpellTime && Core.TickCount - Caster.NextSpellTime < 0)
		{
			Caster.SendLocalizedMessage(502644); // You have not yet recovered from casting a spell.
		}
		else switch (Caster)
		{
			case PlayerMobile mobile when mobile.PeacedUntil > DateTime.UtcNow:
				Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
				break;
			case PlayerMobile {DuelContext: { }} pm when !pm.DuelContext.AllowSpellCast(Caster, this):
				break;
			default:
			{
				if (Caster.Mana >= ScaleMana(GetMana()))
				{
					if (Caster.Spell == null && Caster.CheckSpellCast(this) && CheckCast() && Caster.Region.OnBeginSpellCast(Caster, this))
					{
						State = SpellState.Casting;
						Caster.Spell = this;

						if (Scroll is not BaseWand && RevealOnCast)
							Caster.RevealingAction();

						SayMantra();

						TimeSpan castDelay = GetCastDelay();

						if (ShowHandMovement && (Caster.Body.IsHuman || (Caster.Player && Caster.Body.IsMonster)))
						{
							int count = (int)Math.Ceiling(castDelay.TotalSeconds / AnimateDelay.TotalSeconds);

							if (count != 0)
							{
								_animTimer = new AnimTimer(this, count);
								_animTimer.Start();
							}

							if (Info.LeftHandEffect > 0)
								Caster.FixedParticles(0, 10, 5, Info.LeftHandEffect, EffectLayer.LeftHand);

							if (Info.RightHandEffect > 0)
								Caster.FixedParticles(0, 10, 5, Info.RightHandEffect, EffectLayer.RightHand);
						}

						if (ClearHandsOnCast)
							Caster.ClearHands();

						if (Core.ML)
							WeaponAbility.ClearCurrentAbility(Caster);

						_castTimer = new CastTimer(this, castDelay);

						OnBeginCast();

						if (castDelay > TimeSpan.Zero)
						{
							_castTimer.Start();
						}
						else
						{
							_castTimer.Tick();
						}

						return true;
					}

					return false;
				}

				Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502625); // Insufficient mana

				break;
			}
		}

		return false;
	}

	public abstract void OnCast();

	public virtual void OnBeginCast()
	{
		SendCastEffect();
		EventSink.InvokeOnMobileCastSpell(Caster, Caster.Spell, SpellTarget);
	}

	public virtual void SendCastEffect()
	{ }

	public virtual void GetCastSkills(out double min, out double max)
	{
		min = max = 0;  //Intended but not required for overriding.
	}

	public virtual bool CheckFizzle()
	{
		if (Scroll is BaseWand)
			return true;

		GetCastSkills(out double minSkill, out double maxSkill);

		if (DamageSkill != CastSkill)
			Caster.CheckSkill(DamageSkill, 0.0, Caster.Skills[DamageSkill].Cap);

		return Caster.CheckSkill(CastSkill, minSkill, maxSkill);
	}

	public abstract int GetMana();

	public virtual int ScaleMana(int mana)
	{
		double scalar = 1.0;

		if (!MindRotSpell.GetMindRotScalar(Caster, ref scalar))
			scalar = 1.0;

		// Lower Mana Cost = 40%
		int lmc = AosAttributes.GetValue(Caster, AosAttribute.LowerManaCost);
		if (lmc > 40)
			lmc = 40;

		scalar -= (double)lmc / 100;

		return (int)(mana * scalar);
	}

	public virtual TimeSpan GetDisturbRecovery()
	{
		if (Core.AOS)
			return TimeSpan.Zero;

		double delay = 1.0 - Math.Sqrt((Core.TickCount - StartCastTime) / 1000.0 / GetCastDelay().TotalSeconds);

		if (delay < 0.2)
			delay = 0.2;

		return TimeSpan.FromSeconds(delay);
	}

	public virtual int CastRecoveryBase => 6;
	public virtual int CastRecoveryFastScalar => 1;
	public virtual int CastRecoveryPerSecond => 4;
	public virtual int CastRecoveryMinimum => 0;

	public virtual TimeSpan GetCastRecovery()
	{
		if (!Core.AOS)
			return NextSpellDelay;

		int fcr = AosAttributes.GetValue(Caster, AosAttribute.CastRecovery);

		fcr -= ThunderstormSpell.GetCastRecoveryMalus(Caster);

		int fcrDelay = -(CastRecoveryFastScalar * fcr);

		int delay = CastRecoveryBase + fcrDelay;

		if (delay < CastRecoveryMinimum)
			delay = CastRecoveryMinimum;

		return TimeSpan.FromSeconds((double)delay / CastRecoveryPerSecond);
	}

	//public virtual int CastDelayBase{ get{ return 3; } }
	//public virtual int CastDelayFastScalar{ get{ return 1; } }
	//public virtual int CastDelayPerSecond{ get{ return 4; } }
	//public virtual int CastDelayMinimum{ get{ return 1; } }

	public virtual TimeSpan GetCastDelay()
	{
		if (Scroll is BaseWand)
			return Core.ML ? CastDelayBase : TimeSpan.Zero;

		int fc = Caster.GetSpellCastSpeedBonus(CastSkill);

		TimeSpan baseDelay = CastDelayBase;

		TimeSpan fcDelay = TimeSpan.FromSeconds(-(CastDelayFastScalar * fc * CastDelaySecondsPerTick));

		//int delay = CastDelayBase + circleDelay + fcDelay;
		TimeSpan delay = baseDelay + fcDelay;

		if (delay < CastDelayMinimum)
			delay = CastDelayMinimum;

		//return TimeSpan.FromSeconds( (double)delay / CastDelayPerSecond );
		return delay;
	}

	public virtual void FinishSequence()
	{
		State = SpellState.None;

		if (Caster.Spell == this)
			Caster.Spell = null;
	}

	public virtual int ComputeKarmaAward()
	{
		return 0;
	}

	public virtual bool CheckSequence()
	{
		int mana = ScaleMana(GetMana());

		if (Caster.Deleted || !Caster.Alive || Caster.Spell != this || State != SpellState.Sequencing)
		{
			DoFizzle();
		}
		else if (SpellTarget != null && SpellTarget != Caster && (!Caster.CanSee(SpellTarget) || !Caster.InLOS(SpellTarget)))
		{
			Caster.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
			DoFizzle();
		}
		else if (Scroll != null && Scroll is not Runebook && (Scroll.Amount <= 0 || Scroll.Deleted || Scroll.RootParent != Caster || (Scroll is BaseWand wand1 && (wand1.Charges <= 0 || Scroll.Parent != Caster))))
		{
			DoFizzle();
		}
		else if (!Precast && !CheckCast()) //Is precast is disabled, need to validate the CheckCast
		{
			DoFizzle();
		}
		else if (!ConsumeReagents())
		{
			Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502630); // More reagents are needed for this spell.
		}
		else if (Caster.Mana < mana)
		{
			Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502625); // Insufficient mana for this spell.
		}
		else if (Core.AOS && (Caster.Frozen || Caster.Paralyzed))
		{
			Caster.SendLocalizedMessage(502646); // You cannot cast a spell while frozen.
			DoFizzle();
		}
		else if (Caster is PlayerMobile mobile && mobile.PeacedUntil > DateTime.UtcNow)
		{
			Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
			DoFizzle();
		}
		else if (CheckFizzle())
		{
			if (m_RequiredMana)
			{
				Caster.Mana -= mana;
			}

			switch (Scroll)
			{
				case SpellScroll:
					Scroll.Consume();
					break;
				case BaseWand wand:
					wand.ConsumeCharge(Caster);
					Caster.RevealingAction();
					break;
			}

			if (Scroll is BaseWand)
			{
				bool m = Scroll.Movable;

				Scroll.Movable = false;

				if (ClearHandsOnCast)
					Caster.ClearHands();

				Scroll.Movable = m;
			}
			else
			{
				if (ClearHandsOnCast)
					Caster.ClearHands();
			}

			int karma = ComputeKarmaAward();

			if (karma != 0)
				Misc.Titles.AwardKarma(Caster, karma, true);

			if (TransformationSpellHelper.UnderTransformation(Caster, typeof(VampiricEmbraceSpell)))
			{
				bool garlic = false;

				for (int i = 0; !garlic && i < Info.Reagents.Length; ++i)
					garlic = (Info.Reagents[i] == Reagent.Garlic);

				if (garlic)
				{
					Caster.SendLocalizedMessage(1061651); // The garlic burns you!
					AOS.Damage(Caster, Utility.RandomMinMax(17, 23), 100, 0, 0, 0, 0);
				}
			}

			return true;
		}
		else
		{
			DoFizzle();
		}

		return false;
	}

	public bool CheckBSequence(Mobile target, bool allowDead = false)
	{
		if (!target.Alive && !allowDead)
		{
			Caster.SendLocalizedMessage(501857); // This spell won't work on that!
			return false;
		}

		if (!Caster.CanBeBeneficial(target, true, allowDead) || !CheckSequence())
			return false;

		Caster.DoBeneficial(target);
		return true;

	}

	public bool CheckHSequence(IDamageable target)
	{
		if (!target.Alive)
		{
			Caster.SendLocalizedMessage(501857); // This spell won't work on that!
			return false;
		}

		if (!Caster.InRange(target, SpellRange))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
			return false;
		}

		if (!Caster.CanBeHarmful(target) || !CheckSequence())
			return false;

		Caster.DoHarmful(target);
		return true;

	}

	public virtual IEnumerable<IDamageable> AcquireIndirectTargets(IPoint3D pnt, int range)
	{
		return SpellHelper.AcquireIndirectTargets(Caster, pnt, Caster.Map, range);
	}

	private class AnimTimer : Timer
	{
		private readonly Spell _spell;

		public AnimTimer(Spell spell, int count) : base(TimeSpan.Zero, AnimateDelay, count)
		{
			_spell = spell;

			Priority = TimerPriority.FiftyMs;
		}

		protected override void OnTick()
		{
			if (_spell.State != SpellState.Casting || _spell.Caster.Spell != _spell)
			{
				Stop();
				return;
			}

			if (!_spell.Caster.Mounted && _spell.Info.Action >= 0)
			{
				if (_spell.Caster.Body.IsHuman)
					_spell.Caster.Animate(_spell.Info.Action, 7, 1, true, false, 0);
				else if (_spell.Caster.Player && _spell.Caster.Body.IsMonster)
					_spell.Caster.Animate(12, 7, 1, true, false, 0);
			}

			if (!Running)
				_spell._animTimer = null;
		}
	}

	private class CastTimer : Timer
	{
		private readonly Spell _spell;

		public CastTimer(Spell spell, TimeSpan castDelay) : base(castDelay)
		{
			_spell = spell;

			Priority = TimerPriority.TwentyFiveMs;
		}

		protected override void OnTick()
		{
			if (_spell?.Caster == null)
			{
				return;
			}

			if (_spell.State == SpellState.Casting && _spell.Caster.Spell == _spell)
			{
				_spell.State = SpellState.Sequencing;
				_spell._castTimer = null;
				_spell.Caster.OnSpellCast(_spell);
				_spell.Caster.Region?.OnSpellCast(_spell.Caster, _spell);
				_spell.Caster.NextSpellTime = Core.TickCount + (int)_spell.GetCastRecovery().TotalMilliseconds; // Spell.NextSpellDelay;

				Target originalTarget = _spell.Caster.Target;

				_spell.OnCast();

				if (_spell.Caster.Player && _spell.Caster.Target != originalTarget && _spell.Caster.Target != null)
					_spell.Caster.Target.BeginTimeout(_spell.Caster, TimeSpan.FromSeconds(30.0));

				_spell._castTimer = null;
			}
		}

		public void Tick()
		{
			OnTick();
		}
	}

	private class SpellRequestTarget : Target
	{
		private Spell Spell { get; }

		public SpellRequestTarget(Spell spell) : base(spell.SpellRange, spell.CanTargetGround, spell.SpellTargetFlags)
		{
			Spell = spell;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			Spell.SpellTargetCallback(from, o);
		}
	}
}
