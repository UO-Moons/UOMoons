using Server.Engines.Craft;
using Server.Factions;
using Server.Misc;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items;

public abstract partial class BaseEquipment : BaseItem, IAosAttribute, IOwnerRestricted
{
	[Flags]
	private enum SaveFlag
	{
		None = 0x00000000,
		Attributes = 0x00000001,
		Altered = 0x00000002
	}

	private int m_TimesImbued;
	[CommandProperty(AccessLevel.GameMaster)]
	public int TimesImbued
	{
		get => m_TimesImbued;
		set { m_TimesImbued = value; InvalidateProperties(); }
	}

	private bool m_IsImbued;
	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsImbued
	{
		get
		{
			if (TimesImbued >= 1 && !m_IsImbued)
				m_IsImbued = true;

			return m_IsImbued;
		}
		set
		{
			m_IsImbued = TimesImbued >= 1 || value;
			InvalidateProperties();
		}
	}

	private AosAttributes m_AosAttributes;
	[CommandProperty(AccessLevel.GameMaster)]
	public AosAttributes Attributes
	{
		get => m_AosAttributes;
		set { }
	}

	private bool m_Altered;
	[CommandProperty(AccessLevel.GameMaster)]
	public bool Altered
	{
		get => m_Altered;
		set
		{
			m_Altered = value;
			InvalidateProperties();
		}
	}

	private Mobile m_Owner;
	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner
	{
		get => m_Owner;
		set { m_Owner = value; if (m_Owner != null) { m_OwnerName = m_Owner.Name; } InvalidateProperties(); }
	}

	private string m_OwnerName;
	public virtual string OwnerName
	{
		get => m_OwnerName;
		set { m_OwnerName = value; InvalidateProperties(); }
	}

	private FactionItem m_FactionState;
	public FactionItem FactionItemState
	{
		get => m_FactionState;
		set
		{
			m_FactionState = value;

			LootType = m_FactionState == null ? LootType.Regular : LootType.Blessed;
		}
	}

	public virtual Race RequiredRace => null;
	public virtual bool CanBeWornByGargoyles => false;
	public virtual bool AllowMaleWearer => true;
	public virtual bool AllowFemaleWearer => true;
	public virtual bool CanFortify => true;
	public virtual bool CanRepair => true;
	public virtual bool CanAlter => true;
	public virtual int ArtifactRarity => 0;
	public virtual int[] BaseResists => new[] { 0, 0, 0, 0, 0 };

	public BaseEquipment(int itemId) : base(itemId)
	{
		m_AosAttributes = new AosAttributes(this);
	}

	public BaseEquipment(Serial serial) : base(serial)
	{
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		List<EquipInfoAttribute> attrs = new();

		#region Factions
		if (this is IFactionItem factionItem && factionItem != null && factionItem.FactionItemState != null)
			attrs.Add(new EquipInfoAttribute(1041350)); // faction item
		#endregion

		//Quality
		if (Quality == ItemQuality.Exceptional)
			attrs.Add(new EquipInfoAttribute(1018305 - (int)Quality));

		if (Identified || from.AccessLevel >= AccessLevel.GameMaster)
		{
			//Slayer
			if (this is ISlayer slayerItem)
			{
				if (slayerItem.Slayer != SlayerName.None)
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName(slayerItem.Slayer);
					if (entry != null)
						attrs.Add(new EquipInfoAttribute(entry.Title));
				}

				if (slayerItem.Slayer2 != SlayerName.None)
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName(slayerItem.Slayer2);
					if (entry != null)
						attrs.Add(new EquipInfoAttribute(entry.Title));
				}
			}

			if (this is BaseArmor armor)
			{
				if (armor.Durability != DurabilityLevel.Regular)
					attrs.Add(new EquipInfoAttribute(1038000 + (int)armor.Durability));

				if (armor.ProtectionLevel > ArmorProtectionLevel.Regular && armor.ProtectionLevel <= ArmorProtectionLevel.Invulnerability)
					attrs.Add(new EquipInfoAttribute(1038005 + (int)armor.ProtectionLevel));
			}
			else if (this is BaseWeapon weapon)
			{
				if (weapon.DurabilityLevel != DurabilityLevel.Regular)
					attrs.Add(new EquipInfoAttribute(1038000 + (int)weapon.DurabilityLevel));

				if (weapon.DamageLevel != WeaponDamageLevel.Regular)
					attrs.Add(new EquipInfoAttribute(1038015 + (int)weapon.DamageLevel));

				if (weapon.AccuracyLevel != WeaponAccuracyLevel.Regular)
					attrs.Add(new EquipInfoAttribute(1038010 + (int)weapon.AccuracyLevel));
			}
		}
		else
		{
			//Maybe need to improve this
			if (this is BaseArmor armor && (armor.Durability != DurabilityLevel.Regular || (armor.ProtectionLevel > ArmorProtectionLevel.Regular && armor.ProtectionLevel <= ArmorProtectionLevel.Invulnerability)))
			{
				attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
			}
			else if (this is BaseWeapon weapon && (weapon.Slayer != SlayerName.None || weapon.Slayer2 != SlayerName.None || weapon.DurabilityLevel != DurabilityLevel.Regular || weapon.DamageLevel != WeaponDamageLevel.Regular || weapon.AccuracyLevel != WeaponAccuracyLevel.Regular))
			{
				attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
			}
		}

		if (this is BaseWeapon poisonWeapon && poisonWeapon.Poison != null && poisonWeapon.PoisonCharges > 0)
			attrs.Add(new EquipInfoAttribute(1017383, poisonWeapon.PoisonCharges));

		Mobile crafter = null;
		if (this is ICraftable craftable)
			crafter = craftable.Crafter;

		if (attrs.Count == 0 && crafter != null && Name != null)
			return;

		EquipmentInfo eqInfo = new(1041000, crafter, false, attrs.ToArray());
		_ = from.Send(new DisplayEquipmentInfo(this, eqInfo));
	}

	public virtual int GetLuckBonus()
	{
		return m_AosAttributes.Luck;
	}

	public virtual int GetLowerStatReq()
	{
		return 0;
	}

	public virtual int ComputeStatReq(StatType type)
	{
		return 0;
	}

	public virtual int ComputeStatBonus(StatType type)
	{
		return 0;
	}

	public virtual void AddStatBonuses(Mobile parent)
	{
		if (parent == null)
			return;

		int strBonus = ComputeStatBonus(StatType.Str);
		int dexBonus = ComputeStatBonus(StatType.Dex);
		int intBonus = ComputeStatBonus(StatType.Int);

		if (strBonus == 0 && dexBonus == 0 && intBonus == 0)
			return;

		string modName = Serial.ToString();

		if (strBonus != 0)
			parent.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

		if (dexBonus != 0)
			parent.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

		if (intBonus != 0)
			parent.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
	}

	public virtual void RemoveStatBonuses(Mobile parent)
	{
		if (parent == null)
			return;

		string modName = Serial.ToString();

		_ = parent.RemoveStatMod(modName + "Str");
		_ = parent.RemoveStatMod(modName + "Dex");
		_ = parent.RemoveStatMod(modName + "Int");
	}

	public virtual void OnAfterImbued(Mobile m, int mod, int value)
	{
	}

	public override bool AllowEquipedCast(Mobile from)
	{
		if (base.AllowEquipedCast(from))
			return true;

		return m_AosAttributes.SpellChanneling != 0;
	}

	public override void OnAfterDuped(Item newItem)
	{
		base.OnAfterDuped(newItem);

		if (newItem is BaseEquipment newEquipItem)
		{
			newEquipItem.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
		}
	}

	public static void ValidateMobile(Mobile m)
	{
		for (int i = m.Items.Count - 1; i >= 0; --i)
		{
			if (i >= m.Items.Count)
			{
				continue;
			}

			Item item = m.Items[i];

			if (item is BaseArmor armor)
			{
				if (Core.SA && !RaceDefinitions.ValidateEquipment(m, item))
				{
					m.AddToBackpack(armor);
				}
				else if (!armor.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
				{
					if (armor.AllowFemaleWearer)
					{
						m.SendLocalizedMessage(1010388); // Only females can wear this
					}

					m.AddToBackpack(armor);
				}
				else if (!armor.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
				{
					if (armor.AllowMaleWearer)
					{
						m.SendLocalizedMessage(1063343); // Only males can wear 
					}

					m.AddToBackpack(armor);
				}
			}
			else if (item is BaseClothing clothing)
			{
				if (Core.SA && !RaceDefinitions.ValidateEquipment(m, clothing))
				{
					m.AddToBackpack(clothing);
				}
				else if (!clothing.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
				{
					m.SendLocalizedMessage(clothing.AllowFemaleWearer ? 1010388 : 1071936);

					m.AddToBackpack(clothing);
				}
				else if (!clothing.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
				{
					m.SendLocalizedMessage(clothing.AllowMaleWearer ? 1063343 : 1071936);

					m.AddToBackpack(clothing);
				}
			}
		}
	}

	public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
	{
		if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
			return false;

		return base.AllowSecureTrade(from, to, newOwner, accepted);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
		writer.Write(m_IsImbued);
		writer.Write(m_TimesImbued);
		writer.Write(m_Owner);
		writer.Write(m_OwnerName);

		SaveFlag flags = SaveFlag.None;
		Utility.SetSaveFlag(ref flags, SaveFlag.Attributes, !m_AosAttributes.IsEmpty);
		Utility.SetSaveFlag(ref flags, SaveFlag.Altered, m_Altered);
		writer.WriteEncodedInt((int)flags);

		if (flags.HasFlag(SaveFlag.Attributes))
			m_AosAttributes.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				{
					m_IsImbued = reader.ReadBool();
					m_TimesImbued = reader.ReadInt();
					m_Owner = reader.ReadMobile();
					m_OwnerName = reader.ReadString();

					SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

					m_AosAttributes = flags.HasFlag(SaveFlag.Attributes) ? new AosAttributes(this, reader) : new AosAttributes(this);

					if (flags.HasFlag(SaveFlag.Altered))
					{
						m_Altered = true;
					}

					break;
				}
		}
	}
}
