using Server.Engines.Craft;
using Server.Engines.XmlSpawner2;
using Server.Factions;
using System;

namespace Server.Items;

public abstract class BaseJewel : BaseEquipment, IScissorable, ICraftable, IFactionItem, IResource, ISetItem
{
	private static readonly bool UseNewHits = true;
	private int m_MaxHitPoints;
	private int m_HitPoints;
	private AosElementAttributes m_AosResistances;
	private AosSkillBonuses m_AosSkillBonuses;
	private TalismanAttribute m_TalismanProtection;
	private GemType m_GemType;

	[CommandProperty(AccessLevel.GameMaster)]
	public TalismanAttribute Protection
	{
		get => m_TalismanProtection;
		set { m_TalismanProtection = value; InvalidateProperties(); }
	}

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
	public AosElementAttributes Resistances
	{
		get => m_AosResistances;
		set { }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosSkillBonuses SkillBonuses
	{
		get => m_AosSkillBonuses;
		set { }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public override CraftResource Resource
	{
		get => base.Resource;
		set { base.Resource = value; Hue = CraftResources.GetHue(Resource); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public GemType GemType
	{
		get => m_GemType;
		set
		{
			var old = m_GemType;
			m_GemType = value;
			OnGemTypeChange(old);
			InvalidateProperties();
		}
	}

	public override int PhysicalResistance => m_AosResistances.Physical;
	public override int FireResistance => m_AosResistances.Fire;
	public override int ColdResistance => m_AosResistances.Cold;
	public override int PoisonResistance => m_AosResistances.Poison;
	public override int EnergyResistance => m_AosResistances.Energy;
	public virtual int BaseGemTypeNumber => 0;

	public virtual int InitMinHits => 0;
	public virtual int InitMaxHits => 0;
	public virtual int InitHits => Utility.RandomMinMax(0, 0);

	public override int LabelNumber => m_GemType == GemType.None ? base.LabelNumber : BaseGemTypeNumber + (int)m_GemType - 1;

	public override void OnAfterDuped(Item newItem)
	{
		base.OnAfterDuped(newItem);

		if (newItem != null && newItem is BaseJewel jewel)
		{
			jewel.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
			jewel.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
			jewel.m_TalismanProtection = new TalismanAttribute(m_TalismanProtection);
			jewel.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
			jewel.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
		}
	}

	public virtual CraftResource DefaultResource => CraftResource.Iron;

	public BaseJewel(int itemID, Layer layer) : base(itemID)
	{
		Layer = layer;
		m_AosResistances = new AosElementAttributes(this);
		m_AosSkillBonuses = new AosSkillBonuses(this);
		m_SetAttributes = new AosAttributes(this);
		m_SetSkillBonuses = new AosSkillBonuses(this);
		m_TalismanProtection = new TalismanAttribute();
		base.Resource = DefaultResource;
		m_GemType = GemType.None;
		if (UseNewHits)
		{
			m_HitPoints = m_MaxHitPoints = InitHits;
		}
		else
			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
	}

	public override void OnAdded(IEntity parent)
	{
		if (Core.AOS && parent is Mobile mobile)
		{
			Mobile from = mobile;

			m_AosSkillBonuses.AddTo(from);

			int strBonus = Attributes.BonusStr;
			int dexBonus = Attributes.BonusDex;
			int intBonus = Attributes.BonusInt;

			if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
			{
				string modName = Serial.ToString();

				if (strBonus != 0)
					from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

				if (dexBonus != 0)
					from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

				if (intBonus != 0)
					from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			}

			from.CheckStatTimers();
			#region Mondain's Legacy Sets
			if (IsSetItem)
			{
				m_SetEquipped = SetHelper.FullSetEquipped(from, SetID, Pieces);

				if (m_SetEquipped)
				{
					m_LastEquipped = true;
					SetHelper.AddSetBonus(from, SetID);
				}
			}
			#endregion
		}

		if (parent is Mobile mobile1)
		{
			if (XmlAttach.CheckCanEquip(this, mobile1))
			{
				XmlAttach.CheckOnEquip(this, mobile1);
			}
			else
			{
				mobile1.AddToBackpack(this);
			}
		}
	}

	public override void OnRemoved(IEntity parent)
	{
		if (Core.AOS && parent is Mobile mobile)
		{
			Mobile from = mobile;

			m_AosSkillBonuses.Remove();

			RemoveStatBonuses(from);

			from.CheckStatTimers();
			#region Mondain's Legacy Sets
			if (IsSetItem && m_SetEquipped)
			{
				SetHelper.RemoveSetBonus(from, SetID, this);
			}
			#endregion
		}
		XmlAttach.CheckOnRemoved(this, parent);
	}

	public virtual void SetProtection(Type type, TextDefinition name, int amount)
	{
		m_TalismanProtection = new TalismanAttribute(type, name, amount);
	}

	public BaseJewel(Serial serial) : base(serial)
	{
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
			list.Add(1063341); // exceptional
		}
	}

	public override void AddWeightProperty(ObjectPropertyList list)
	{
		base.AddWeightProperty(list);
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
			list.Add(1080240, Pieces.ToString()); // Part of a Jewelry Set (~1_val~ pieces)

			if (BardMasteryBonus)
			{
				list.Add(1151553); // Activate: Bard Mastery Bonus x2<br>(Effect: 1 min. Cooldown: 30 min.)
			}

			if (m_SetEquipped)
			{
				list.Add(1080241); // Full Jewelry Set Present					
				SetHelper.GetSetProperties(list, this);
			}
		}
		#endregion

		m_AosSkillBonuses.GetProperties(list);

		int prop;

		#region Stygian Abyss
		if (RequiredRace == Race.Elf)
		{
			list.Add(1075086); // Elves Only
		}
		else if (RequiredRace == Race.Gargoyle)
		{
			list.Add(1111709); // Gargoyles Only
		}
		#endregion

		if ((prop = ArtifactRarity) > 0)
		{
			list.Add(1061078, prop.ToString()); // artifact rarity ~1_val~
		}

		if (m_TalismanProtection != null && !m_TalismanProtection.IsEmpty && m_TalismanProtection.Amount > 0)
		{
			list.Add(1072387, "{0}\t{1}", m_TalismanProtection.Name != null ? m_TalismanProtection.Name.ToString() : "Unknown", m_TalismanProtection.Amount); // ~1_NAME~ Protection: +~2_val~%
		}

		if (Attributes.SpellChanneling != 0)
		{
			list.Add(1060482); // spell channeling
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

		if (Core.ML && (prop = Attributes.IncreasedKarmaLoss) != 0)
		{
			list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%
		}

		base.AddResistanceProperties(list);

		XmlAttach.AddAttachmentProperties(this, list);

		if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
		{
			list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
		}

		if (Core.ML && IsSetItem && !m_SetEquipped)
		{
			list.Add(1072378); // <br>Only when full set is present:				
			SetHelper.GetSetProperties(list, this);
		}
	}

	public override void AddItemPowerProperties(ObjectPropertyList list)
	{
	}

	public virtual void OnGemTypeChange(GemType old)
	{
	}

	public int GemLocalization()
	{
		return m_GemType switch
		{
			GemType.StarSapphire => 1023867,
			GemType.Emerald => 1023887,
			GemType.Sapphire => 1023887,
			GemType.Ruby => 1023868,
			GemType.Citrine => 1023875,
			GemType.Amethyst => 1023863,
			GemType.Tourmaline => 1023872,
			GemType.Amber => 1062607,
			GemType.Diamond => 1062608,
			_ => 0,
		};
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		writer.Write(m_SetPhysicalBonus);
		writer.Write(m_SetFireBonus);
		writer.Write(m_SetColdBonus);
		writer.Write(m_SetPoisonBonus);
		writer.Write(m_SetEnergyBonus);
		writer.WriteEncodedInt(m_MaxHitPoints);
		writer.WriteEncodedInt(m_HitPoints);
		writer.WriteEncodedInt((int)m_GemType);
		m_AosResistances.Serialize(writer);
		m_AosSkillBonuses.Serialize(writer);
		m_SetAttributes.Serialize(writer);
		m_SetSkillBonuses.Serialize(writer);
		m_TalismanProtection.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				{
					m_SetPhysicalBonus = reader.ReadInt();
					m_SetFireBonus = reader.ReadInt();
					m_SetColdBonus = reader.ReadInt();
					m_SetPoisonBonus = reader.ReadInt();
					m_SetEnergyBonus = reader.ReadInt();
					m_TalismanProtection = new TalismanAttribute(reader);
					m_SetEquipped = reader.ReadBool();
					m_SetHue = reader.ReadEncodedInt();
					m_SetAttributes = new AosAttributes(this, reader);
					m_SetSkillBonuses = new AosSkillBonuses(this, reader);
					m_MaxHitPoints = reader.ReadEncodedInt();
					m_HitPoints = reader.ReadEncodedInt();
					m_GemType = (GemType)reader.ReadEncodedInt();
					m_AosResistances = new AosElementAttributes(this, reader);
					m_AosSkillBonuses = new AosSkillBonuses(this, reader);

					if (Core.AOS && Parent is Mobile mobile)
						m_AosSkillBonuses.AddTo(mobile);

					int strBonus = Attributes.BonusStr;
					int dexBonus = Attributes.BonusDex;
					int intBonus = Attributes.BonusInt;

					if (Parent is Mobile mobile1 && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
					{

						string modName = Serial.ToString();

						if (strBonus != 0)
							mobile1.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

						if (dexBonus != 0)
							mobile1.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

						if (intBonus != 0)
							mobile1.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
					}

					if (Parent is Mobile mobile2)
						mobile2.CheckStatTimers();

					break;
				}
		}

		if (m_TalismanProtection == null)
		{
			m_TalismanProtection = new TalismanAttribute();
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
	}

	public bool Scissor(Mobile from, Scissors scissors)
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
				Item res = (Item)Activator.CreateInstance(CraftResources.GetInfo(Resource).ResourceTypes[0]);

				ScissorHelper(from, res, PlayerConstructed ? (item.Resources.GetAt(0).Amount / 2) : 1);
				return true;
			}
			catch
			{
			}
		}

		from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
		return false;
	}

	#region ICraftable Members
	public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		PlayerConstructed = true;

		Type resourceType = typeRes;

		if (resourceType == null)
		{
			resourceType = craftItem.Resources.GetAt(0).ItemType;
		}

		if (!craftItem.ForceNonExceptional)
		{
			Resource = CraftResources.GetFromType(resourceType);
		}

		if (1 < craftItem.Resources.Count)
		{
			resourceType = craftItem.Resources.GetAt(1).ItemType;

			if (resourceType == typeof(StarSapphire))
			{
				GemType = GemType.StarSapphire;
			}
			else if (resourceType == typeof(Emerald))
			{
				GemType = GemType.Emerald;
			}
			else if (resourceType == typeof(Sapphire))
			{
				GemType = GemType.Sapphire;
			}
			else if (resourceType == typeof(Ruby))
			{
				GemType = GemType.Ruby;
			}
			else if (resourceType == typeof(Citrine))
			{
				GemType = GemType.Citrine;
			}
			else if (resourceType == typeof(Amethyst))
			{
				GemType = GemType.Amethyst;
			}
			else if (resourceType == typeof(Tourmaline))
			{
				GemType = GemType.Tourmaline;
			}
			else if (resourceType == typeof(Amber))
			{
				GemType = GemType.Amber;
			}
			else if (resourceType == typeof(Diamond))
			{
				GemType = GemType.Diamond;
			}
		}

		#region Mondain's Legacy
		Quality = (ItemQuality)quality;

		if (makersMark)
		{
			Crafter = from;
		}
		#endregion

		return 1;
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

	public virtual bool BardMasteryBonus => SetID == SetItem.Virtuoso;

	public virtual bool MixedSet => false;

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

	private AosAttributes m_SetAttributes;
	private AosSkillBonuses m_SetSkillBonuses;

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

	public int SetResistBonus(ResistanceType resist)
	{
		return resist switch
		{
			ResistanceType.Physical => PhysicalResistance,
			ResistanceType.Fire => FireResistance,
			ResistanceType.Cold => ColdResistance,
			ResistanceType.Poison => PoisonResistance,
			ResistanceType.Energy => EnergyResistance,
			_ => 0,
		};
	}
	#endregion
}
