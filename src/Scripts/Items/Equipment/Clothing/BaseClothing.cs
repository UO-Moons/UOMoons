using Server.Engines.Craft;
using Server.Factions;
using Server.Misc;
using Server.Network;
using System;

namespace Server.Items
{
	public interface IArcaneEquip
	{
		bool IsArcane { get; }
		int CurArcaneCharges { get; set; }
		int MaxArcaneCharges { get; set; }
		int TempHue { get; set; }
	}

	public abstract class BaseClothing : BaseEquipment, IDyable, IScissorable, IFactionItem, ICraftable, IWearableDurability, IResource, ISetItem
	{
		private int m_MaxHitPoints;
		private int m_HitPoints;
		protected CraftResource m_Resource;
		private int m_StrReq = -1;

		private AosArmorAttributes m_AosClothingAttributes;
		private AosSkillBonuses m_AosSkillBonuses;
		private AosElementAttributes m_AosResistances;
		private AosWeaponAttributes m_AosWeaponAttributes;

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get => m_MaxHitPoints;
			set { m_MaxHitPoints = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get => m_HitPoints;
			set
			{
				if (value != m_HitPoints && MaxHitPoints > 0)
				{
					m_HitPoints = value;

					if (m_HitPoints < 0)
						Delete();
					else if (m_HitPoints > MaxHitPoints)
						m_HitPoints = MaxHitPoints;

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get => (m_StrReq == -1 ? (Core.AOS ? AosStrReq : OldStrReq) : m_StrReq);
			set { m_StrReq = value; InvalidateProperties(); }
		}

		public virtual CraftResource DefaultResource => CraftResource.None;

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get => m_Resource;
			set { m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosArmorAttributes ClothingAttributes
		{
			get => m_AosClothingAttributes;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get => m_AosSkillBonuses;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosElementAttributes Resistances
		{
			get => m_AosResistances;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosWeaponAttributes WeaponAttributes { get => m_AosWeaponAttributes; set { } }

		public virtual int BasePhysicalResistance => 0;
		public virtual int BaseFireResistance => 0;
		public virtual int BaseColdResistance => 0;
		public virtual int BasePoisonResistance => 0;
		public virtual int BaseEnergyResistance => 0;

		public override int PhysicalResistance => BasePhysicalResistance + m_AosResistances.Physical;
		public override int FireResistance => BaseFireResistance + m_AosResistances.Fire;
		public override int ColdResistance => BaseColdResistance + m_AosResistances.Cold;
		public override int PoisonResistance => BasePoisonResistance + m_AosResistances.Poison;
		public override int EnergyResistance => BaseEnergyResistance + m_AosResistances.Energy;

		public virtual int BaseStrBonus => 0;
		public virtual int BaseDexBonus => 0;
		public virtual int BaseIntBonus => 0;

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
				return false;

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public virtual int AosStrReq => 10;
		public virtual int OldStrReq => 0;

		public virtual int InitMinHits => 0;
		public virtual int InitMaxHits => 0;
		public virtual bool CanBeBlessed => true;

		public override int ComputeStatReq(StatType type)
		{
			int v;

			//if ( type == StatType.Str )
			v = StrRequirement;

			return AOS.Scale(v, 100 - GetLowerStatReq());
		}

		public override int ComputeStatBonus(StatType type)
		{
			if (type == StatType.Str)
				return BaseStrBonus + Attributes.BonusStr;
			else if (type == StatType.Dex)
				return BaseDexBonus + Attributes.BonusDex;
			else
				return BaseIntBonus + Attributes.BonusInt;
		}

		public override bool CanEquip(Mobile from)
		{
			if (!Ethics.Ethic.CheckEquip(from, this))
				return false;

			if (from.IsPlayer())
			{
				if (this is IAccountRestricted restricted && restricted.Account != null)
				{
					if (from.Account is not Accounting.Account acct || acct.Username != restricted.Account)
					{
						from.SendLocalizedMessage(1071296); // This item is Account Bound and your character is not bound to it. You cannot use this item.
						return false;
					}
				}

				if (Core.SA && !RaceDefinitions.ValidateEquipment(from, this))
				{
					return false;
				}
				else if (!AllowMaleWearer && !from.Female)
				{
					if (AllowFemaleWearer)
						from.SendLocalizedMessage(1010388); // Only females can wear this.
					else
						from.SendMessage("You may not wear this.");

					return false;
				}
				else if (!AllowFemaleWearer && from.Female)
				{
					if (AllowMaleWearer)
						from.SendLocalizedMessage(1063343); // Only males can wear this.
					else
						from.SendMessage("You may not wear this.");

					return false;
				}
				else
				{
					int strBonus = ComputeStatBonus(StatType.Str);
					int strReq = ComputeStatReq(StatType.Str);

					if (from.Str < strReq || (from.Str + strBonus) < 1)
					{
						from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
						return false;
					}
				}
			}

			return base.CanEquip(from);
		}

		/*public static void ValidateMobile(Mobile m)
		{
			for (int i = m.Items.Count - 1; i >= 0; --i)
			{
				if (i >= m.Items.Count)
					continue;

				Item item = m.Items[i];

				if (item is BaseClothing clothing)
				{
					if (clothing.RequiredRace != null && m.Race != clothing.RequiredRace)
					{
						if (clothing.RequiredRace == Race.Elf)
							m.SendLocalizedMessage(1072203); // Only Elves may use this.
						else
							m.SendMessage("Only {0} may use this.", clothing.RequiredRace.PluralName);

						m.AddToBackpack(clothing);
					}
					else if (!clothing.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
					{
						if (clothing.AllowFemaleWearer)
							m.SendLocalizedMessage(1010388); // Only females can wear this.
						else
							m.SendMessage("You may not wear this.");

						m.AddToBackpack(clothing);
					}
					else if (!clothing.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
					{
						if (clothing.AllowMaleWearer)
							m.SendLocalizedMessage(1063343); // Only males can wear this.
						else
							m.SendMessage("You may not wear this.");

						m.AddToBackpack(clothing);
					}
				}
			}
		}*/

		public override int GetLowerStatReq()
		{
			if (!Core.AOS)
				return 0;

			return m_AosClothingAttributes.LowerStatReq;
		}

		public override void OnAdded(IEntity parent)
		{
			if (parent is Mobile mob)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(mob);

				if (Core.ML && IsSetItem)
				{
					m_SetEquipped = SetHelper.FullSetEquipped(mob, SetID, Pieces);

					if (m_SetEquipped)
					{
						m_LastEquipped = true;
						SetHelper.AddSetBonus(mob, SetID);
					}
				}

				AddStatBonuses(mob);
				mob.CheckStatTimers();
			}

			base.OnAdded(parent);
		}

		public override void OnRemoved(IEntity parent)
		{
			if (parent is Mobile mob)
			{
				if (Core.AOS)
					m_AosSkillBonuses.Remove();

				if (Core.ML && IsSetItem && m_SetEquipped)
				{
					SetHelper.RemoveSetBonus(mob, SetID, this);
				}

				RemoveStatBonuses(mob);

				mob.CheckStatTimers();
			}

			base.OnRemoved(parent);
		}

		public virtual int OnHit(BaseWeapon weapon, int damageTaken)
		{
			int Absorbed = Utility.RandomMinMax(1, 4);

			damageTaken -= Absorbed;

			if (damageTaken < 0)
				damageTaken = 0;

			if (25 > Utility.Random(100)) // 25% chance to lower durability
			{
				if (Core.AOS && m_AosClothingAttributes.SelfRepair > Utility.Random(10))
				{
					HitPoints += 2;
				}
				else
				{
					int wear;

					if (weapon.Type == WeaponType.Bashing)
						wear = Absorbed / 2;
					else
						wear = Utility.Random(2);

					if (wear > 0 && m_MaxHitPoints > 0)
					{
						if (m_HitPoints >= wear)
						{
							HitPoints -= wear;
							wear = 0;
						}
						else
						{
							wear -= HitPoints;
							HitPoints = 0;
						}

						if (wear > 0)
						{
							if (m_MaxHitPoints > wear)
							{
								MaxHitPoints -= wear;

								if (Parent is Mobile mob)
									mob.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
							}
							else
							{
								Delete();
							}
						}
					}
				}
			}

			return damageTaken;
		}

		public BaseClothing(int itemID, Layer layer) : this(itemID, layer, 0)
		{
		}

		public BaseClothing(int itemID, Layer layer, int hue) : base(itemID)
		{
			Layer = layer;
			Hue = hue;
			m_Resource = DefaultResource;
			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
			m_AosClothingAttributes = new AosArmorAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
			m_AosResistances = new AosElementAttributes(this);
			m_AosWeaponAttributes = new AosWeaponAttributes(this);
			m_SetAttributes = new AosAttributes(this);
			m_SetSkillBonuses = new AosSkillBonuses(this);
		}

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem != null && newItem is BaseClothing clothing)
			{
				clothing.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
				clothing.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
				clothing.m_AosClothingAttributes = new AosArmorAttributes(newItem, m_AosClothingAttributes);
				clothing.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
				clothing.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
				clothing.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
			}
		}

		public BaseClothing(Serial serial) : base(serial)
		{
		}

		public void UnscaleDurability()
		{
			int scale = 100 + m_AosClothingAttributes.DurabilityBonus;

			m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
			m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;

			InvalidateProperties();
		}

		public void ScaleDurability()
		{
			int scale = 100 + m_AosClothingAttributes.DurabilityBonus;

			m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
			m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;

			InvalidateProperties();
		}

		public override bool CheckPropertyConfliction(Mobile m)
		{
			if (base.CheckPropertyConfliction(m))
				return true;

			if (Layer == Layer.Pants)
				return (m.FindItemOnLayer(Layer.InnerLegs) != null);

			if (Layer == Layer.Shirt)
				return (m.FindItemOnLayer(Layer.InnerTorso) != null);

			return false;
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			int oreType = CraftResources.GetResourceLabel(m_Resource);

			if (oreType != 0)
				list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
			else if (Name == null)
				list.Add(LabelNumber);
			else
				list.Add(Name);
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
			}

			if (Quality == ItemQuality.Exceptional)
			{
				list.Add(1018303); // Exceptional
			}

			if (Altered)
			{
				list.Add(1111880); // Altered
			}
		}

		public override void AddWeightProperty(ObjectPropertyList list)
		{
			base.AddWeightProperty(list);

			if (!string.IsNullOrEmpty(EngravedText))
			{
				list.Add(1158847, Utility.FixHtml(EngravedText)); // Embroidered: ~1_MESSAGE~	
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			#region Factions
			if (FactionItemState != null)
				list.Add(1041350); // faction item
			#endregion

			#region Mondain's Legacy Sets
			if (IsSetItem)
			{
				if (MixedSet)
				{
					list.Add(1073491, Pieces.ToString()); // Part of a Weapon/Armor Set (~1_val~ pieces)
				}
				else
				{
					list.Add(1072376, Pieces.ToString()); // Part of an Armor Set (~1_val~ pieces)
				}

				if (BardMasteryBonus)
				{
					list.Add(1151553); // Activate: Bard Mastery Bonus x2<br>(Effect: 1 min. Cooldown: 30 min.)
				}

				if (m_SetEquipped)
				{
					if (MixedSet)
					{
						list.Add(1073492); // Full Weapon/Armor Set Present
					}
					else
					{
						list.Add(1072377); // Full Armor Set Present
					}

					GetSetProperties(list);
				}
			}
			#endregion

			if (RequiredRace == Race.Elf)
			{
				list.Add(1075086); // Elves Only
			}
			#region Stygian Abyss
			else if (RequiredRace == Race.Gargoyle)
			{
				list.Add(1111709); // Gargoyles Only
			}
			#endregion

			if (m_AosSkillBonuses != null)
			{
				m_AosSkillBonuses.GetProperties(list);
			}

			int prop;

			if ((prop = ArtifactRarity) > 0)
			{
				list.Add(1061078, prop.ToString()); // artifact rarity ~1_val~
			}

			if ((prop = m_AosWeaponAttributes.HitColdArea) != 0)
			{
				list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitDispel) != 0)
			{
				list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitEnergyArea) != 0)
			{
				list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitFireArea) != 0)
			{
				list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitFireball) != 0)
			{
				list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitHarm) != 0)
			{
				list.Add(1060421, prop.ToString()); // hit harm ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitLeechHits) != 0)
			{
				list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitLightning) != 0)
			{
				list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitLowerAttack) != 0)
			{
				list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitLowerDefend) != 0)
			{
				list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitMagicArrow) != 0)
			{
				list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitLeechMana) != 0)
			{
				list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitPhysicalArea) != 0)
			{
				list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitPoisonArea) != 0)
			{
				list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%
			}

			if ((prop = m_AosWeaponAttributes.HitLeechStam) != 0)
			{
				list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%
			}

			if (Attributes.SpellChanneling != 0)
			{
				list.Add(1060482); // spell channeling
			}

			if ((prop = m_AosClothingAttributes.SelfRepair) != 0)
			{
				list.Add(1060450, prop.ToString()); // self repair ~1_val~
			}

			if (Attributes.NightSight != 0)
			{
				list.Add(1060441); // night sight
			}

			if ((prop = Attributes.BonusStr) != 0)
			{
				list.Add(1060485, prop.ToString()); // strength bonus ~1_val~
			}

			if ((prop = Attributes.BonusDex) != 0)
			{
				list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~
			}

			if ((prop = Attributes.BonusInt) != 0)
			{
				list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~
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

			if ((prop = Attributes.Luck) != 0)
			{
				list.Add(1060436, prop.ToString()); // luck ~1_val~
			}

			if ((prop = Attributes.EnhancePotions) != 0)
			{
				list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%
			}

			if ((prop = Attributes.ReflectPhysical) != 0)
			{
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%
			}

			if ((prop = Attributes.AttackChance) != 0)
			{
				list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%
			}

			if ((prop = Attributes.WeaponSpeed) != 0)
			{
				list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%
			}

			if ((prop = Attributes.WeaponDamage) != 0)
			{
				list.Add(1060401, prop.ToString()); // damage increase ~1_val~%
			}

			if ((prop = Attributes.DefendChance) != 0)
			{
				list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%
			}

			if ((prop = Attributes.CastRecovery) != 0)
			{
				list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~		
			}

			if ((prop = Attributes.CastSpeed) != 0)
			{
				list.Add(1060413, prop.ToString()); // faster casting ~1_val~
			}

			if ((prop = Attributes.SpellDamage) != 0)
			{
				list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%
			}

			if ((prop = Attributes.LowerManaCost) != 0)
			{
				list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%
			}

			if ((prop = Attributes.LowerRegCost) != 0)
			{
				list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%
			}

			if ((prop = Attributes.LowerAmmoCost) != 0)
			{
				list.Add(1075208, prop.ToString()); // Lower Ammo Cost ~1_Percentage~%
			}

			if (Core.ML && (prop = Attributes.IncreasedKarmaLoss) != 0)
			{
				list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%
			}

			base.AddResistanceProperties(list);

			if (m_AosClothingAttributes.MageArmor != 0)
			{
				list.Add(1060437); // mage armor
			}

			if ((prop = m_AosClothingAttributes.LowerStatReq) != 0)
			{
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%
			}

			if ((prop = ComputeStatReq(StatType.Str)) > 0)
			{
				list.Add(1061170, prop.ToString()); // strength requirement ~1_val~
			}

			if ((prop = m_AosClothingAttributes.DurabilityBonus) > 0)
			{
				list.Add(1151780, prop.ToString()); // durability +~1_VAL~%
			}

			if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
			{
				list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
			}

			if (Core.ML && IsSetItem && !m_SetEquipped)
			{
				list.Add(1072378); // <br>Only when full set is present:				
				GetSetProperties(list);
			}
		}

		public override void AddItemPowerProperties(ObjectPropertyList list)
		{
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

			if (Quality == ItemQuality.Exceptional)
				list.Add(1060636); // exceptional

			if (RequiredRace == Race.Elf)
				list.Add(1075086); // Elves Only

			if (m_AosSkillBonuses != null)
				m_AosSkillBonuses.GetProperties(list);

			int prop;

			if ((prop = ArtifactRarity) > 0)
				list.Add(1061078, prop.ToString()); // artifact rarity ~1_val~

			if ((prop = Attributes.WeaponDamage) != 0)
				list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

			if ((prop = Attributes.DefendChance) != 0)
				list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

			if ((prop = Attributes.BonusDex) != 0)
				list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

			if ((prop = Attributes.EnhancePotions) != 0)
				list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

			if ((prop = Attributes.CastRecovery) != 0)
				list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

			if ((prop = Attributes.CastSpeed) != 0)
				list.Add(1060413, prop.ToString()); // faster casting ~1_val~

			if ((prop = Attributes.AttackChance) != 0)
				list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

			if ((prop = Attributes.BonusHits) != 0)
				list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

			if ((prop = Attributes.BonusInt) != 0)
				list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

			if ((prop = Attributes.LowerManaCost) != 0)
				list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

			if ((prop = Attributes.LowerRegCost) != 0)
				list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

			if ((prop = m_AosClothingAttributes.LowerStatReq) != 0)
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

			if ((prop = Attributes.Luck) != 0)
				list.Add(1060436, prop.ToString()); // luck ~1_val~

			if ((prop = m_AosClothingAttributes.MageArmor) != 0)
				list.Add(1060437); // mage armor

			if ((prop = Attributes.BonusMana) != 0)
				list.Add(1060439, prop.ToString()); // mana increase ~1_val~

			if ((prop = Attributes.RegenMana) != 0)
				list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

			if ((prop = Attributes.NightSight) != 0)
				list.Add(1060441); // night sight

			if ((prop = Attributes.ReflectPhysical) != 0)
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

			if ((prop = Attributes.RegenStam) != 0)
				list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

			if ((prop = Attributes.RegenHits) != 0)
				list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

			if ((prop = m_AosClothingAttributes.SelfRepair) != 0)
				list.Add(1060450, prop.ToString()); // self repair ~1_val~

			if ((prop = Attributes.SpellChanneling) != 0)
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

			base.AddResistanceProperties(list);

			if ((prop = m_AosClothingAttributes.DurabilityBonus) > 0)
				list.Add(1060410, prop.ToString()); // durability ~1_val~%

			if ((prop = ComputeStatReq(StatType.Str)) > 0)
				list.Add(1061170, prop.ToString()); // strength requirement ~1_val~

			if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
				list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
		}*/

		#region Serialization
		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Resource = 0x00000001,
			Attributes = 0x00000002,
			ClothingAttributes = 0x00000004,
			SkillBonuses = 0x00000008,
			Resistances = 0x00000010,
			MaxHitPoints = 0x00000020,
			HitPoints = 0x00000040,
			StrReq = 0x00000080,
			xWeaponAttributes = 0x00000100
		}

		private static void SetSaveFlag(ref SetFlag flags, SetFlag toSet, bool setIf)
		{
			if (setIf)
			{
				flags |= toSet;
			}
		}

		private static bool GetSaveFlag(SetFlag flags, SetFlag toGet)
		{
			return (flags & toGet) != 0;
		}

		[Flags]
		private enum SetFlag
		{
			None = 0x00000000,
			Attributes = 0x00000001,
			ArmorAttributes = 0x00000002,
			SkillBonuses = 0x00000004,
			PhysicalBonus = 0x00000008,
			FireBonus = 0x00000010,
			ColdBonus = 0x00000020,
			PoisonBonus = 0x00000040,
			EnergyBonus = 0x00000080,
			SetHue = 0x00000100,
			LastEquipped = 0x00000200,
			SetEquipped = 0x00000400,
			SetSelfRepair = 0x00000800,
		}

		public void XWeaponAttributesDeserializeHelper(GenericReader reader, BaseClothing item)
		{
			SaveFlag flags = (SaveFlag)reader.ReadInt();

			if (flags != SaveFlag.None)
			{
				flags = SaveFlag.xWeaponAttributes;
			}

			if (flags.HasFlag(SaveFlag.xWeaponAttributes))
			{
				m_AosWeaponAttributes = new AosWeaponAttributes(item, reader);
			}
			else
			{
				m_AosWeaponAttributes = new AosWeaponAttributes(item);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			#region Mondain's Legacy Sets
			SetFlag sflags = SetFlag.None;

			SetSaveFlag(ref sflags, SetFlag.Attributes, !m_SetAttributes.IsEmpty);
			SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !m_SetSkillBonuses.IsEmpty);
			SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, m_SetPhysicalBonus != 0);
			SetSaveFlag(ref sflags, SetFlag.FireBonus, m_SetFireBonus != 0);
			SetSaveFlag(ref sflags, SetFlag.ColdBonus, m_SetColdBonus != 0);
			SetSaveFlag(ref sflags, SetFlag.PoisonBonus, m_SetPoisonBonus != 0);
			SetSaveFlag(ref sflags, SetFlag.EnergyBonus, m_SetEnergyBonus != 0);
			SetSaveFlag(ref sflags, SetFlag.SetHue, m_SetHue != 0);
			SetSaveFlag(ref sflags, SetFlag.LastEquipped, m_LastEquipped);
			SetSaveFlag(ref sflags, SetFlag.SetEquipped, m_SetEquipped);
			SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, m_SetSelfRepair != 0);

			writer.WriteEncodedInt((int)sflags);

			if (GetSaveFlag(sflags, SetFlag.Attributes))
			{
				m_SetAttributes.Serialize(writer);
			}

			if (GetSaveFlag(sflags, SetFlag.SkillBonuses))
			{
				m_SetSkillBonuses.Serialize(writer);
			}

			if (GetSaveFlag(sflags, SetFlag.PhysicalBonus))
			{
				writer.WriteEncodedInt(m_SetPhysicalBonus);
			}

			if (GetSaveFlag(sflags, SetFlag.FireBonus))
			{
				writer.WriteEncodedInt(m_SetFireBonus);
			}

			if (GetSaveFlag(sflags, SetFlag.ColdBonus))
			{
				writer.WriteEncodedInt(m_SetColdBonus);
			}

			if (GetSaveFlag(sflags, SetFlag.PoisonBonus))
			{
				writer.WriteEncodedInt(m_SetPoisonBonus);
			}

			if (GetSaveFlag(sflags, SetFlag.EnergyBonus))
			{
				writer.WriteEncodedInt(m_SetEnergyBonus);
			}

			if (GetSaveFlag(sflags, SetFlag.SetHue))
			{
				writer.WriteEncodedInt(m_SetHue);
			}

			if (GetSaveFlag(sflags, SetFlag.LastEquipped))
			{
				writer.Write(m_LastEquipped);
			}

			if (GetSaveFlag(sflags, SetFlag.SetEquipped))
			{
				writer.Write(m_SetEquipped);
			}

			if (GetSaveFlag(sflags, SetFlag.SetSelfRepair))
			{
				writer.WriteEncodedInt(m_SetSelfRepair);
			}
			#endregion

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != DefaultResource);
			Utility.SetSaveFlag(ref flags, SaveFlag.ClothingAttributes, !m_AosClothingAttributes.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.Resistances, !m_AosResistances.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, m_MaxHitPoints != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.HitPoints, m_HitPoints != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.xWeaponAttributes))
			{
				m_AosWeaponAttributes.Serialize(writer);
			}

			if (flags.HasFlag(SaveFlag.Resource))
				writer.WriteEncodedInt((int)m_Resource);

			if (flags.HasFlag(SaveFlag.ClothingAttributes))
				m_AosClothingAttributes.Serialize(writer);

			if (flags.HasFlag(SaveFlag.SkillBonuses))
				m_AosSkillBonuses.Serialize(writer);

			if (flags.HasFlag(SaveFlag.Resistances))
				m_AosResistances.Serialize(writer);

			if (flags.HasFlag(SaveFlag.MaxHitPoints))
				writer.WriteEncodedInt(m_MaxHitPoints);

			if (flags.HasFlag(SaveFlag.HitPoints))
				writer.WriteEncodedInt(m_HitPoints);

			if (flags.HasFlag(SaveFlag.StrReq))
				writer.WriteEncodedInt(m_StrReq);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						#region Mondain's Legacy Sets
						SetFlag sflags = (SetFlag)reader.ReadEncodedInt();

						if (GetSaveFlag(sflags, SetFlag.Attributes))
						{
							m_SetAttributes = new AosAttributes(this, reader);
						}
						else
						{
							m_SetAttributes = new AosAttributes(this);
						}

						if (GetSaveFlag(sflags, SetFlag.ArmorAttributes))
						{
							m_SetSelfRepair = (new AosArmorAttributes(this, reader)).SelfRepair;
						}

						if (GetSaveFlag(sflags, SetFlag.SkillBonuses))
						{
							m_SetSkillBonuses = new AosSkillBonuses(this, reader);
						}
						else
						{
							m_SetSkillBonuses = new AosSkillBonuses(this);
						}

						if (GetSaveFlag(sflags, SetFlag.PhysicalBonus))
						{
							m_SetPhysicalBonus = reader.ReadEncodedInt();
						}

						if (GetSaveFlag(sflags, SetFlag.FireBonus))
						{
							m_SetFireBonus = reader.ReadEncodedInt();
						}

						if (GetSaveFlag(sflags, SetFlag.ColdBonus))
						{
							m_SetColdBonus = reader.ReadEncodedInt();
						}

						if (GetSaveFlag(sflags, SetFlag.PoisonBonus))
						{
							m_SetPoisonBonus = reader.ReadEncodedInt();
						}

						if (GetSaveFlag(sflags, SetFlag.EnergyBonus))
						{
							m_SetEnergyBonus = reader.ReadEncodedInt();
						}

						if (GetSaveFlag(sflags, SetFlag.SetHue))
						{
							m_SetHue = reader.ReadEncodedInt();
						}

						if (GetSaveFlag(sflags, SetFlag.LastEquipped))
						{
							m_LastEquipped = reader.ReadBool();
						}

						if (GetSaveFlag(sflags, SetFlag.SetEquipped))
						{
							m_SetEquipped = reader.ReadBool();
						}

						if (GetSaveFlag(sflags, SetFlag.SetSelfRepair))
						{
							m_SetSelfRepair = reader.ReadEncodedInt();
						}
						#endregion

						SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.xWeaponAttributes))
						{
							m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						}
						else
						{
							m_AosWeaponAttributes = new AosWeaponAttributes(this);
						}

						if (flags.HasFlag(SaveFlag.Resource))
							m_Resource = (CraftResource)reader.ReadEncodedInt();
						else
							m_Resource = DefaultResource;

						if (flags.HasFlag(SaveFlag.ClothingAttributes))
							m_AosClothingAttributes = new AosArmorAttributes(this, reader);
						else
							m_AosClothingAttributes = new AosArmorAttributes(this);

						if (flags.HasFlag(SaveFlag.SkillBonuses))
							m_AosSkillBonuses = new AosSkillBonuses(this, reader);
						else
							m_AosSkillBonuses = new AosSkillBonuses(this);

						if (flags.HasFlag(SaveFlag.Resistances))
							m_AosResistances = new AosElementAttributes(this, reader);
						else
							m_AosResistances = new AosElementAttributes(this);

						if (flags.HasFlag(SaveFlag.MaxHitPoints))
							m_MaxHitPoints = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.HitPoints))
							m_HitPoints = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.StrReq))
							m_StrReq = reader.ReadEncodedInt();
						else
							m_StrReq = -1;

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

			if (m_AosWeaponAttributes == null)
			{
				m_AosWeaponAttributes = new AosWeaponAttributes(this);
			}

			if (m_MaxHitPoints == 0 && m_HitPoints == 0)
				m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);

			if (Parent is Mobile parent)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(parent);

				AddStatBonuses(parent);
				parent.CheckStatTimers();
			}
		}
		#endregion

		public virtual bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
				return false;
			else if (RootParent is Mobile && from != RootParent)
				return false;

			Hue = sender.DyedHue;

			return true;
		}

		public virtual bool Scissor(Mobile from, Scissors scissors)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(502437); // Items you wish to cut must be in your backpack.
				return false;
			}

			if (Ethics.Ethic.IsImbued(this))
			{
				from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
				return false;
			}

			CraftSystem system = DefTailoring.CraftSystem;

			CraftItem item = system.CraftItems.SearchFor(GetType());

			if (item != null && item.Resources.Count == 1 && item.Resources.GetAt(0).Amount >= 2)
			{
				try
				{
					Type resourceType = null;

					CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

					if (info != null && info.ResourceTypes.Length > 0)
						resourceType = info.ResourceTypes[0];

					if (resourceType == null)
						resourceType = item.Resources.GetAt(0).ItemType;

					Item res = (Item)Activator.CreateInstance(resourceType);

					ScissorHelper(from, res, PlayerConstructed ? (item.Resources.GetAt(0).Amount / 2) : 1);

					res.LootType = LootType.Regular;

					return true;
				}
				catch
				{
				}
			}

			from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
			return false;
		}

		public void DistributeBonuses(int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				switch (Utility.Random(5))
				{
					case 0: ++m_AosResistances.Physical; break;
					case 1: ++m_AosResistances.Fire; break;
					case 2: ++m_AosResistances.Cold; break;
					case 3: ++m_AosResistances.Poison; break;
					case 4: ++m_AosResistances.Energy; break;
				}
			}

			InvalidateProperties();
		}

		#region ICraftable Members
		public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
		{
			Quality = (ItemQuality)quality;

			if (makersMark)
			{
				Crafter = from;
			}

			#region Mondain's Legacy
			if (!craftItem.ForceNonExceptional)
			{
				if (DefaultResource != CraftResource.None)
				{
					Type resourceType = typeRes;

					if (resourceType == null)
					{
						resourceType = craftItem.Resources.GetAt(0).ItemType;
					}

					Resource = CraftResources.GetFromType(resourceType);
				}
				else
				{
					Hue = resHue;
				}
			}
			#endregion

			PlayerConstructed = true;

			return quality;
		}

		#endregion

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

		public virtual bool MixedSet => false;

		public bool IsSetItem => SetID == SetItem.None ? false : true;

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

		public bool SetEquipped
		{
			get => m_SetEquipped;
			set => m_SetEquipped = value;
		}

		public bool LastEquipped
		{
			get => m_LastEquipped;
			set => m_LastEquipped = value;
		}

		private AosAttributes m_SetAttributes;
		private AosSkillBonuses m_SetSkillBonuses;
		private int m_SetSelfRepair;

		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes SetAttributes
		{
			get => m_SetAttributes;
			set
			{
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SetSkillBonuses
		{
			get => m_SetSkillBonuses;
			set
			{
			}
		}

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

		private int m_SetPhysicalBonus, m_SetFireBonus, m_SetColdBonus, m_SetPoisonBonus, m_SetEnergyBonus;

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

			if (!m_SetEquipped)
			{
				if (SetID == SetItem.Virtuoso)
				{
					list.Add(1151571); // Mastery Bonus Cooldown: 15 min.
				}

				if (m_SetPhysicalBonus != 0)
				{
					list.Add(1072382, m_SetPhysicalBonus.ToString()); // physical resist +~1_val~%
				}

				if (m_SetFireBonus != 0)
				{
					list.Add(1072383, m_SetFireBonus.ToString()); // fire resist +~1_val~%
				}

				if (m_SetColdBonus != 0)
				{
					list.Add(1072384, m_SetColdBonus.ToString()); // cold resist +~1_val~%
				}

				if (m_SetPoisonBonus != 0)
				{
					list.Add(1072385, m_SetPoisonBonus.ToString()); // poison resist +~1_val~%
				}

				if (m_SetEnergyBonus != 0)
				{
					list.Add(1072386, m_SetEnergyBonus.ToString()); // energy resist +~1_val~%			
				}
			}
			else if (m_SetEquipped && SetHelper.ResistsBonusPerPiece(this) && RootParentEntity is Mobile mobile)
			{

				if (m_SetPhysicalBonus != 0)
				{
					list.Add(1080361, SetHelper.GetSetTotalResist(mobile, ResistanceType.Physical).ToString()); // physical resist ~1_val~% (total)
				}

				if (m_SetFireBonus != 0)
				{
					list.Add(1080362, SetHelper.GetSetTotalResist(mobile, ResistanceType.Fire).ToString()); // fire resist ~1_val~% (total)
				}

				if (m_SetColdBonus != 0)
				{
					list.Add(1080363, SetHelper.GetSetTotalResist(mobile, ResistanceType.Cold).ToString()); // cold resist ~1_val~% (total)
				}

				if (m_SetPoisonBonus != 0)
				{
					list.Add(1080364, SetHelper.GetSetTotalResist(mobile, ResistanceType.Poison).ToString()); // poison resist ~1_val~% (total)
				}

				if (m_SetEnergyBonus != 0)
				{
					list.Add(1080365, SetHelper.GetSetTotalResist(mobile, ResistanceType.Energy).ToString()); // energy resist ~1_val~% (total)
				}
			}

			if ((prop = m_SetSelfRepair) != 0)
			{
				list.Add(1060450, prop.ToString()); // self repair ~1_val~		
			}

			SetHelper.GetSetProperties(list, this);
		}

		public int SetResistBonus(ResistanceType resist)
		{
			if (SetHelper.ResistsBonusPerPiece(this))
			{
				switch (resist)
				{
					case ResistanceType.Physical: return m_SetEquipped ? PhysicalResistance + m_SetPhysicalBonus : PhysicalResistance;
					case ResistanceType.Fire: return m_SetEquipped ? FireResistance + m_SetFireBonus : FireResistance;
					case ResistanceType.Cold: return m_SetEquipped ? ColdResistance + m_SetColdBonus : ColdResistance;
					case ResistanceType.Poison: return m_SetEquipped ? PoisonResistance + m_SetPoisonBonus : PoisonResistance;
					case ResistanceType.Energy: return m_SetEquipped ? EnergyResistance + m_SetEnergyBonus : EnergyResistance;
				}
			}
			else
			{
				switch (resist)
				{
					case ResistanceType.Physical: return m_SetEquipped ? LastEquipped ? (PhysicalResistance * Pieces) + m_SetPhysicalBonus : 0 : PhysicalResistance;
					case ResistanceType.Fire: return m_SetEquipped ? LastEquipped ? (FireResistance * Pieces) + m_SetFireBonus : 0 : FireResistance;
					case ResistanceType.Cold: return m_SetEquipped ? LastEquipped ? (ColdResistance * Pieces) + m_SetColdBonus : 0 : ColdResistance;
					case ResistanceType.Poison: return m_SetEquipped ? LastEquipped ? (PoisonResistance * Pieces) + m_SetPoisonBonus : 0 : PoisonResistance;
					case ResistanceType.Energy: return m_SetEquipped ? LastEquipped ? (EnergyResistance * Pieces) + m_SetEnergyBonus : 0 : EnergyResistance;
				}
			}

			return 0;
		}
		#endregion
	}
}
