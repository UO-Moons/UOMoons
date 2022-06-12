using Server.Engines.Craft;
using Server.Engines.XmlSpawner2;
using Server.Factions;
using Server.Misc;
using Server.Network;
using System;
using AMA = Server.Items.ArmorMeditationAllowance;
using AMT = Server.Items.ArmorMaterialType;

namespace Server.Items;

public abstract class BaseArmor : BaseEquipment, IScissorable, IFactionItem, ICraftable, IWearableDurability, IResource, ISetItem
{
	private static readonly bool UseNewHits = true;
	private int m_MaxHitPoints;
	private int m_HitPoints;
	private DurabilityLevel m_Durability;
	private ArmorProtectionLevel m_Protection;
	private int m_PhysicalBonus, m_FireBonus, m_ColdBonus, m_PoisonBonus, m_EnergyBonus;
	private AosArmorAttributes m_AosArmorAttributes;
	private AosSkillBonuses m_AosSkillBonuses;
	private AosWeaponAttributes m_AosWeaponAttributes;
	private TalismanAttribute m_TalismanProtection;
	private int m_ArmorBase = -1;
	private int m_StrBonus = -1, m_DexBonus = -1, m_IntBonus = -1;
	private int m_StrReq = -1, m_DexReq = -1, m_IntReq = -1;
	private AMA m_Meditate = (AMA)(-1);
	public abstract AMT MaterialType { get; }
	public virtual int ArmorBase => 0;
	public virtual AMA DefMedAllowance => AMA.None;
	public virtual int StrReq => 0;
	public virtual int DexReq => 0;
	public virtual int IntReq => 0;
	public virtual int StrBonusValue => 0;
	public virtual int DexBonusValue => 0;
	public virtual int IntBonusValue => 0;
	public static bool ShowDexandInt => false;

	[CommandProperty(AccessLevel.GameMaster)]
	public AMA MeditationAllowance
	{
		get => m_Meditate == (AMA)(-1) ? DefMedAllowance : m_Meditate;
		set => m_Meditate = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int BaseArmorRating
	{
		get
		{
			if (m_ArmorBase == -1)
				return ArmorBase;
			else
				return m_ArmorBase;
		}
		set
		{
			m_ArmorBase = value; Invalidate();
		}
	}

	public double BaseArmorRatingScaled => BaseArmorRating * ArmorScalar;
	public double ArmorRatingScaled => ArmorRating * ArmorScalar;

	[CommandProperty(AccessLevel.GameMaster)]
	public int StrBonus
	{
		get => m_StrBonus == -1 ? StrBonusValue : m_StrBonus;
		set { m_StrBonus = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int DexBonus
	{
		get => m_DexBonus == -1 ? DexBonusValue : m_DexBonus;
		set { m_DexBonus = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int IntBonus
	{
		get => m_IntBonus == -1 ? IntBonusValue : m_IntBonus;
		set { m_IntBonus = value; InvalidateProperties(); }
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
		set { m_DexReq = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int IntRequirement
	{
		get => m_IntReq == -1 ? IntReq : m_IntReq;
		set { m_IntReq = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public override CraftResource Resource
	{
		get => base.Resource;
		set
		{
			if (Resource != value)
			{
				UnscaleDurability();

				base.Resource = value;

				if (CraftItem.RetainsColor(GetType()))
				{
					Hue = CraftResources.GetHue(Resource);
				}

				Invalidate();
				InvalidateProperties();

				if (Parent is Mobile mob)
					mob.UpdateResistances();

				ScaleDurability();
			}
		}
	}

	public virtual double ArmorScalar
	{
		get
		{
			int pos = (int)BodyPosition;

			if (pos >= 0 && pos < ArmorScalars.Length)
				return ArmorScalars[pos];

			return 1.0;
		}
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
	public override ItemQuality Quality
	{
		get => base.Quality;
		set
		{
			UnscaleDurability();
			base.Quality = value;
			Invalidate();
			InvalidateProperties();
			ScaleDurability();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public DurabilityLevel Durability
	{
		get => m_Durability;
		set { UnscaleDurability(); m_Durability = value; ScaleDurability(); InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public ArmorProtectionLevel ProtectionLevel
	{
		get => m_Protection;
		set
		{
			if (m_Protection != value)
			{
				m_Protection = value;

				Invalidate();
				InvalidateProperties();

				if (Parent is Mobile mobile)
					mobile.UpdateResistances();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosArmorAttributes ArmorAttributes
	{
		get => m_AosArmorAttributes;
		set { }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosWeaponAttributes WeaponAttributes { get => m_AosWeaponAttributes; set { } }

	[CommandProperty(AccessLevel.GameMaster)]
	public TalismanAttribute Protection
	{
		get => m_TalismanProtection;
		set { m_TalismanProtection = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosSkillBonuses SkillBonuses
	{
		get => m_AosSkillBonuses;
		set { }
	}

	public virtual double ArmorRating
	{
		get
		{
			int ar = BaseArmorRating;

			if (m_Protection != ArmorProtectionLevel.Regular)
				ar += 10 + (5 * (int)m_Protection);

			switch (Resource)
			{
				case CraftResource.DullCopper: ar += 2; break;
				case CraftResource.ShadowIron: ar += 4; break;
				case CraftResource.Copper: ar += 6; break;
				case CraftResource.Bronze: ar += 8; break;
				case CraftResource.Gold: ar += 10; break;
				case CraftResource.Agapite: ar += 12; break;
				case CraftResource.Verite: ar += 14; break;
				case CraftResource.Valorite: ar += 16; break;
				case CraftResource.SpinedLeather: ar += 10; break;
				case CraftResource.HornedLeather: ar += 13; break;
				case CraftResource.BarbedLeather: ar += 16; break;
			}

			ar += -8 + (8 * (int)Quality);
			return ScaleArmorByDurability(ar);
		}
	}

	public override void OnAfterDuped(Item newItem)
	{
		base.OnAfterDuped(newItem);

		if (newItem != null && newItem is BaseArmor armor)
		{
			armor.m_AosArmorAttributes = new AosArmorAttributes(newItem, m_AosArmorAttributes);
			armor.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
			armor.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
			armor.m_TalismanProtection = new TalismanAttribute(m_TalismanProtection);
			armor.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
			armor.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
		}
	}

	public override int ComputeStatReq(StatType type)
	{
		int v;

		if (type == StatType.Str)
			v = StrRequirement;
		else if (type == StatType.Dex)
			v = DexRequirement;
		else
			v = IntRequirement;

		return AOS.Scale(v, 100 - GetLowerStatReq());
	}

	public override int ComputeStatBonus(StatType type)
	{
		if (type == StatType.Str)
			return StrBonus + Attributes.BonusStr;
		else if (type == StatType.Dex)
			return DexBonus + Attributes.BonusDex;
		else
			return IntBonus + Attributes.BonusInt;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int PhysicalBonus { get => m_PhysicalBonus; set { m_PhysicalBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int FireBonus { get => m_FireBonus; set { m_FireBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ColdBonus { get => m_ColdBonus; set { m_ColdBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int PoisonBonus { get => m_PoisonBonus; set { m_PoisonBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int EnergyBonus { get => m_EnergyBonus; set { m_EnergyBonus = value; InvalidateProperties(); } }

	public virtual int BasePhysicalResistance => 0;
	public virtual int BaseFireResistance => 0;
	public virtual int BaseColdResistance => 0;
	public virtual int BasePoisonResistance => 0;
	public virtual int BaseEnergyResistance => 0;

	public override int PhysicalResistance => BasePhysicalResistance + GetProtOffset() + GetResourceAttrs().ArmorPhysicalResist + m_PhysicalBonus;
	public override int FireResistance => BaseFireResistance + GetProtOffset() + GetResourceAttrs().ArmorFireResist + m_FireBonus;
	public override int ColdResistance => BaseColdResistance + GetProtOffset() + GetResourceAttrs().ArmorColdResist + m_ColdBonus;
	public override int PoisonResistance => BasePoisonResistance + GetProtOffset() + GetResourceAttrs().ArmorPoisonResist + m_PoisonBonus;
	public override int EnergyResistance => BaseEnergyResistance + GetProtOffset() + GetResourceAttrs().ArmorEnergyResist + m_EnergyBonus;

	public virtual int InitMinHits => 0;
	public virtual int InitMaxHits => 0;
	public virtual int InitHits => Utility.RandomMinMax(0, 0);

	[CommandProperty(AccessLevel.GameMaster)]
	public ArmorBodyType BodyPosition => Layer switch
	{
		Layer.TwoHanded => ArmorBodyType.Shield,
		Layer.Gloves => ArmorBodyType.Gloves,
		Layer.Helm => ArmorBodyType.Helmet,
		Layer.Arms => ArmorBodyType.Arms,
		Layer.InnerLegs or Layer.OuterLegs or Layer.Pants => ArmorBodyType.Legs,
		Layer.InnerTorso or Layer.OuterTorso or Layer.Shirt => ArmorBodyType.Chest,
		_ => ArmorBodyType.Gorget,
	};

	public void DistributeBonuses(int amount)
	{
		for (int i = 0; i < amount; ++i)
		{
			switch (Utility.Random(5))
			{
				case 0: ++m_PhysicalBonus; break;
				case 1: ++m_FireBonus; break;
				case 2: ++m_ColdBonus; break;
				case 3: ++m_PoisonBonus; break;
				case 4: ++m_EnergyBonus; break;
			}
		}

		InvalidateProperties();
	}

	public CraftAttributeInfo GetResourceAttrs()
	{
		CraftResourceInfo info = CraftResources.GetInfo(Resource);

		if (info == null)
			return CraftAttributeInfo.Blank;

		return info.AttributeInfo;
	}

	public int GetProtOffset()
	{
		switch (m_Protection)
		{
			case ArmorProtectionLevel.Guarding: return 1;
			case ArmorProtectionLevel.Hardening: return 2;
			case ArmorProtectionLevel.Fortification: return 3;
			case ArmorProtectionLevel.Invulnerability: return 4;
			case ArmorProtectionLevel.Regular:
			case ArmorProtectionLevel.Defense:
			default:
				break;
		}

		return 0;
	}

	public void UnscaleDurability()
	{
		int scale = 100 + GetDurabilityBonus();

		m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
		m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;
		InvalidateProperties();
	}

	public void ScaleDurability()
	{
		int scale = 100 + GetDurabilityBonus();

		m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
		m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;
		InvalidateProperties();
	}

	public int GetDurabilityBonus()
	{
		int bonus = 0;

		if (Quality == ItemQuality.Exceptional)
			bonus += 20;

		switch (m_Durability)
		{
			case DurabilityLevel.Durable: bonus += 20; break;
			case DurabilityLevel.Substantial: bonus += 50; break;
			case DurabilityLevel.Massive: bonus += 70; break;
			case DurabilityLevel.Fortified: bonus += 100; break;
			case DurabilityLevel.Indestructible: bonus += 120; break;
		}

		if (Core.AOS)
		{
			bonus += m_AosArmorAttributes.DurabilityBonus;

			CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);
			CraftAttributeInfo attrInfo = null;

			if (resInfo != null)
				attrInfo = resInfo.AttributeInfo;

			if (attrInfo != null)
				bonus += attrInfo.ArmorDurability;
		}

		return bonus;
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

	public static double[] ArmorScalars { get; set; } = { 0.07, 0.07, 0.14, 0.15, 0.22, 0.35 };

	public override int GetLowerStatReq()
	{
		if (!Core.AOS)
			return 0;

		int v = m_AosArmorAttributes.LowerStatReq;

		CraftResourceInfo info = CraftResources.GetInfo(Resource);

		if (info != null)
		{
			CraftAttributeInfo attrInfo = info.AttributeInfo;

			if (attrInfo != null)
				v += attrInfo.ArmorLowerRequirements;
		}

		if (v > 100)
			v = 100;

		return v;
	}

	public override void OnAdded(IEntity parent)
	{
		if (parent is Mobile from)
		{
			if (Core.AOS)
				m_AosSkillBonuses.AddTo(from);

			#region Mondain's Legacy Sets
			if (Core.ML && IsSetItem)
			{
				SetEquipped = SetHelper.FullSetEquipped(from, SetID, Pieces);

				if (SetEquipped)
				{
					LastEquipped = true;
					SetHelper.AddSetBonus(from, SetID);
				}
			}
			#endregion

			from.Delta(MobileDelta.Armor); // Tell them armor rating has changed
		}
	}

	public virtual double ScaleArmorByDurability(double armor)
	{
		int scale = 100;

		if (m_MaxHitPoints > 0 && m_HitPoints < m_MaxHitPoints)
			scale = 50 + ((50 * m_HitPoints) / m_MaxHitPoints);

		return (armor * scale) / 100;
	}

	protected void Invalidate()
	{
		if (Parent is Mobile mobile)
		{
			mobile.Delta(MobileDelta.Armor); // Tell them armor rating has changed
		}
	}

	public BaseArmor(Serial serial) : base(serial)
	{
	}

	#region Serialize/DeSerialize
	[Flags]
	private enum SaveFlag
	{
		None = 0x00000000,
		Attributes = 0x00000001,
		ArmorAttributes = 0x00000002,
		PhysicalBonus = 0x00000004,
		FireBonus = 0x00000008,
		ColdBonus = 0x00000010,
		PoisonBonus = 0x00000020,
		EnergyBonus = 0x00000040,
		MaxHitPoints = 0x00000060,
		HitPoints = 0x00000080,
		Durability = 0x00000100,
		Protection = 0x00000200,
		BaseArmor = 0x00000400,
		StrBonus = 0x00000600,
		DexBonus = 0x00000800,
		IntBonus = 0x00001000,
		StrReq = 0x00002000,
		DexReq = 0x00004000,
		IntReq = 0x00006000,
		MedAllowance = 0x00008000,
		SkillBonuses = 0x00010000,
		xWeaponAttributes = 0x00020000,
		TalismanProtection = 0x00040000
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
		Hue = 0x00000100,
		LastEquipped = 0x00000200,
		SetEquipped = 0x00000400,
		SetSelfRepair = 0x00000800,
	}

	public void XWeaponAttributesDeserializeHelper(GenericReader reader, BaseArmor item)
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
		SetFlag sflags = SetFlag.None;

		Utility.SetSaveFlag(ref sflags, SetFlag.Attributes, !m_SetAttributes.IsEmpty);
		Utility.SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !m_SetSkillBonuses.IsEmpty);
		Utility.SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, m_SetPhysicalBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.FireBonus, m_SetFireBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.ColdBonus, m_SetColdBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.PoisonBonus, m_SetPoisonBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.EnergyBonus, m_SetEnergyBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.Hue, m_SetHue != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.LastEquipped, LastEquipped);
		Utility.SetSaveFlag(ref sflags, SetFlag.SetEquipped, SetEquipped);
		Utility.SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, m_SetSelfRepair != 0);

		writer.WriteEncodedInt((int)sflags);

		if (sflags.HasFlag(SetFlag.Attributes))
		{
			m_SetAttributes.Serialize(writer);
		}

		if (sflags.HasFlag(SetFlag.SkillBonuses))
		{
			m_SetSkillBonuses.Serialize(writer);
		}

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

		if (sflags.HasFlag(SetFlag.Hue))
		{
			writer.WriteEncodedInt(m_SetHue);
		}

		if (sflags.HasFlag(SetFlag.LastEquipped))
		{
			writer.Write(LastEquipped);
		}

		if (sflags.HasFlag(SetFlag.SetEquipped))
		{
			writer.Write(SetEquipped);
		}

		if (sflags.HasFlag(SetFlag.SetSelfRepair))
		{
			writer.WriteEncodedInt(m_SetSelfRepair);
		}

		SaveFlag flags = SaveFlag.None;

		Utility.SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
		Utility.SetSaveFlag(ref flags, SaveFlag.ArmorAttributes, !m_AosArmorAttributes.IsEmpty);
		Utility.SetSaveFlag(ref flags, SaveFlag.PhysicalBonus, m_PhysicalBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.FireBonus, m_FireBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.ColdBonus, m_ColdBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.PoisonBonus, m_PoisonBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.EnergyBonus, m_EnergyBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, m_MaxHitPoints != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.HitPoints, m_HitPoints != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.Durability, m_Durability != DurabilityLevel.Regular);
		Utility.SetSaveFlag(ref flags, SaveFlag.Protection, m_Protection != ArmorProtectionLevel.Regular);
		Utility.SetSaveFlag(ref flags, SaveFlag.BaseArmor, m_ArmorBase != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.StrBonus, m_StrBonus != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.DexBonus, m_DexBonus != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.IntBonus, m_IntBonus != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.MedAllowance, m_Meditate != (AMA)(-1));
		Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);

		writer.WriteEncodedInt((int)flags);

		if (flags.HasFlag(SaveFlag.xWeaponAttributes))
		{
			m_AosWeaponAttributes.Serialize(writer);
		}

		if (flags.HasFlag(SaveFlag.TalismanProtection))
		{
			m_TalismanProtection.Serialize(writer);
		}

		if (flags.HasFlag(SaveFlag.ArmorAttributes))
			m_AosArmorAttributes.Serialize(writer);

		if (flags.HasFlag(SaveFlag.PhysicalBonus))
			writer.WriteEncodedInt(m_PhysicalBonus);

		if (flags.HasFlag(SaveFlag.FireBonus))
			writer.WriteEncodedInt(m_FireBonus);

		if (flags.HasFlag(SaveFlag.ColdBonus))
			writer.WriteEncodedInt(m_ColdBonus);

		if (flags.HasFlag(SaveFlag.PoisonBonus))
			writer.WriteEncodedInt(m_PoisonBonus);

		if (flags.HasFlag(SaveFlag.EnergyBonus))
			writer.WriteEncodedInt(m_EnergyBonus);

		if (flags.HasFlag(SaveFlag.MaxHitPoints))
			writer.WriteEncodedInt(m_MaxHitPoints);

		if (flags.HasFlag(SaveFlag.HitPoints))
			writer.WriteEncodedInt(m_HitPoints);

		if (flags.HasFlag(SaveFlag.Durability))
			writer.WriteEncodedInt((int)m_Durability);

		if (flags.HasFlag(SaveFlag.Protection))
			writer.WriteEncodedInt((int)m_Protection);

		if (flags.HasFlag(SaveFlag.BaseArmor))
			writer.WriteEncodedInt(m_ArmorBase);

		if (flags.HasFlag(SaveFlag.StrBonus))
			writer.WriteEncodedInt(m_StrBonus);

		if (flags.HasFlag(SaveFlag.DexBonus))
			writer.WriteEncodedInt(m_DexBonus);

		if (flags.HasFlag(SaveFlag.IntBonus))
			writer.WriteEncodedInt(m_IntBonus);

		if (flags.HasFlag(SaveFlag.StrReq))
			writer.WriteEncodedInt(m_StrReq);

		if (flags.HasFlag(SaveFlag.DexReq))
			writer.WriteEncodedInt(m_DexReq);

		if (flags.HasFlag(SaveFlag.IntReq))
			writer.WriteEncodedInt(m_IntReq);

		if (flags.HasFlag(SaveFlag.MedAllowance))
			writer.WriteEncodedInt((int)m_Meditate);

		if (flags.HasFlag(SaveFlag.SkillBonuses))
			m_AosSkillBonuses.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				{
					SetFlag sflags = (SetFlag)reader.ReadEncodedInt();

					if (sflags.HasFlag(SetFlag.Attributes))
					{
						m_SetAttributes = new AosAttributes(this, reader);
					}
					else
					{
						m_SetAttributes = new AosAttributes(this);
					}

					if (sflags.HasFlag(SetFlag.ArmorAttributes))
					{
						m_SetSelfRepair = (new AosArmorAttributes(this, reader)).SelfRepair;
					}

					if (sflags.HasFlag(SetFlag.SkillBonuses))
					{
						m_SetSkillBonuses = new AosSkillBonuses(this, reader);
					}
					else
					{
						m_SetSkillBonuses = new AosSkillBonuses(this);
					}

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

					if (sflags.HasFlag(SetFlag.Hue))
					{
						m_SetHue = reader.ReadEncodedInt();
					}

					if (sflags.HasFlag(SetFlag.LastEquipped))
					{
						LastEquipped = reader.ReadBool();
					}

					if (sflags.HasFlag(SetFlag.SetEquipped))
					{
						SetEquipped = reader.ReadBool();
					}

					if (sflags.HasFlag(SetFlag.SetSelfRepair))
					{
						m_SetSelfRepair = reader.ReadEncodedInt();
					}

					SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.xWeaponAttributes))
					{
						m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
					}
					else
					{
						m_AosWeaponAttributes = new AosWeaponAttributes(this);
					}

					if (flags.HasFlag(SaveFlag.TalismanProtection))
					{
						m_TalismanProtection = new TalismanAttribute(reader);
					}
					else
					{
						m_TalismanProtection = new TalismanAttribute();
					}

					if (flags.HasFlag(SaveFlag.ArmorAttributes))
						m_AosArmorAttributes = new AosArmorAttributes(this, reader);
					else
						m_AosArmorAttributes = new AosArmorAttributes(this);

					if (flags.HasFlag(SaveFlag.PhysicalBonus))
						m_PhysicalBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.FireBonus))
						m_FireBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.ColdBonus))
						m_ColdBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.PoisonBonus))
						m_PoisonBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.EnergyBonus))
						m_EnergyBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.MaxHitPoints))
						m_MaxHitPoints = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.HitPoints))
						m_HitPoints = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.Durability))
					{
						m_Durability = (DurabilityLevel)reader.ReadEncodedInt();

						if (m_Durability > DurabilityLevel.Indestructible)
							m_Durability = DurabilityLevel.Durable;
					}

					if (flags.HasFlag(SaveFlag.Protection))
					{
						m_Protection = (ArmorProtectionLevel)reader.ReadEncodedInt();

						if (m_Protection > ArmorProtectionLevel.Invulnerability)
							m_Protection = ArmorProtectionLevel.Defense;
					}

					if (flags.HasFlag(SaveFlag.BaseArmor))
						m_ArmorBase = reader.ReadEncodedInt();
					else
						m_ArmorBase = -1;

					if (flags.HasFlag(SaveFlag.StrBonus))
						m_StrBonus = reader.ReadEncodedInt();
					else
						m_StrBonus = -1;

					if (flags.HasFlag(SaveFlag.DexBonus))
						m_DexBonus = reader.ReadEncodedInt();
					else
						m_DexBonus = -1;

					if (flags.HasFlag(SaveFlag.IntBonus))
						m_IntBonus = reader.ReadEncodedInt();
					else
						m_IntBonus = -1;

					if (flags.HasFlag(SaveFlag.StrReq))
						m_StrReq = reader.ReadEncodedInt();
					else
						m_StrReq = -1;

					if (flags.HasFlag(SaveFlag.DexReq))
						m_DexReq = reader.ReadEncodedInt();
					else
						m_DexReq = -1;

					if (flags.HasFlag(SaveFlag.IntReq))
						m_IntReq = reader.ReadEncodedInt();
					else
						m_IntReq = -1;

					if (flags.HasFlag(SaveFlag.MedAllowance))
						m_Meditate = (AMA)reader.ReadEncodedInt();
					else
						m_Meditate = (AMA)(-1);

					if (flags.HasFlag(SaveFlag.SkillBonuses))
						m_AosSkillBonuses = new AosSkillBonuses(this, reader);

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

		if (m_AosSkillBonuses == null)
			m_AosSkillBonuses = new AosSkillBonuses(this);

		if (Core.AOS && Parent is Mobile mobile)
			m_AosSkillBonuses.AddTo(mobile);

		if (Parent is Mobile mob)
		{
			AddStatBonuses(mob);
			mob.CheckStatTimers();
		}
	}
	#endregion

	public virtual CraftResource DefaultResource => CraftResource.Iron;

	public BaseArmor(int itemID) : base(itemID)
	{
		Layer = (Layer)ItemData.Quality;
		m_Durability = DurabilityLevel.Regular;
		base.Resource = DefaultResource;
		Hue = CraftResources.GetHue(Resource);
		if (UseNewHits)
		{
			m_HitPoints = m_MaxHitPoints = InitHits;
		}
		else
			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
		m_AosArmorAttributes = new AosArmorAttributes(this);
		m_AosSkillBonuses = new AosSkillBonuses(this);
		m_SetAttributes = new AosAttributes(this);
		m_SetSkillBonuses = new AosSkillBonuses(this);
		m_AosWeaponAttributes = new AosWeaponAttributes(this);
		m_TalismanProtection = new TalismanAttribute();
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

			if (this is BaseGlasses && from.NetState != null && !from.NetState.SupportsExpansion(Expansion.ML))
			{
				from.SendLocalizedMessage(1072791); // You must upgrade to Mondain's Legacy in order to use that item.
				return false;
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
				int strBonus = ComputeStatBonus(StatType.Str), strReq = ComputeStatReq(StatType.Str);
				int dexBonus = ComputeStatBonus(StatType.Dex), dexReq = ComputeStatReq(StatType.Dex);
				int intBonus = ComputeStatBonus(StatType.Int), intReq = ComputeStatReq(StatType.Int);

				if (from.Dex < dexReq || (from.Dex + dexBonus) < 1)
				{
					from.SendLocalizedMessage(502077); // You do not have enough dexterity to equip this item.
					return false;
				}
				else if (from.Str < strReq || (from.Str + strBonus) < 1)
				{
					from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
					return false;
				}
				else if (from.Int < intReq || (from.Int + intBonus) < 1)
				{
					from.SendMessage("You are not smart enough to equip that.");
					return false;
				}
			}
		}

		if (!XmlAttach.CheckCanEquip(this, from))
		{
			return false;
		}
		else
		{
			return base.CanEquip(from);
		}
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

	public override bool OnEquip(Mobile from)
	{
		from.CheckStatTimers();

		AddStatBonuses(from);

		XmlAttach.CheckOnEquip(this, from);
		return base.OnEquip(from);
	}

	public override void OnRemoved(IEntity parent)
	{
		if (parent is Mobile m)
		{
			RemoveStatBonuses(m);

			if (Core.AOS)
				m_AosSkillBonuses.Remove();

			((Mobile)parent).Delta(MobileDelta.Armor); // Tell them armor rating has changed
			m.CheckStatTimers();

			#region Mondain's Legacy Sets
			if (Core.ML && IsSetItem && SetEquipped)
			{
				SetHelper.RemoveSetBonus(m, SetID, this);
			}
			#endregion
		}
		XmlAttach.CheckOnRemoved(this, parent);
		base.OnRemoved(parent);
	}

	public virtual int OnHit(BaseWeapon weapon, int damageTaken)
	{
		double HalfAr = ArmorRating / 2.0;
		int Absorbed = (int)(HalfAr + HalfAr * Utility.RandomDouble());

		damageTaken -= Absorbed;
		if (damageTaken < 0)
			damageTaken = 0;

		if (Absorbed < 2)
			Absorbed = 2;

		if (25 > Utility.Random(100)) // 25% chance to lower durability
		{
			if (Core.AOS && m_AosArmorAttributes.SelfRepair > Utility.Random(10))
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

							if (Parent is Mobile mobile)
								mobile.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
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

	[Hue, CommandProperty(AccessLevel.GameMaster)]
	public override int Hue
	{
		get => base.Hue;
		set { base.Hue = value; InvalidateProperties(); }
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		int oreType = CraftResources.GetResourceLabel(Resource);

		if (Quality == ItemQuality.Exceptional)
		{
			if (oreType != 0)
				list.Add(1053100, "#{0}\t{1}", oreType, GetNameString()); // exceptional ~1_oretype~ ~2_armortype~
			else
				list.Add(1050040, GetNameString()); // exceptional ~1_ITEMNAME~
		}
		else
		{
			if (oreType != 0)
				list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
			else if (Name == null)
				list.Add(LabelNumber);
			else
				list.Add(Name);
		}
	}

	public override int GetLuckBonus()
	{
		CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

		if (resInfo == null)
			return 0;

		CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

		if (attrInfo == null)
			return 0;

		return attrInfo.ArmorLuck;
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

	public virtual void AddDamageTypeProperty(ObjectPropertyList list)
	{
	}

	public override void AddNameProperties(ObjectPropertyList list)
	{
		base.AddNameProperties(list);
		if (this is WhiteDaemonArms || this is WhiteDaemonChest || this is WhiteDaemonGloves || this is WhiteDaemonLegs || this is WhiteDaemonHelm)
		{
			list.Add(1041372);// deamon bone armor
		}

		if (!Core.ML && this is NecromanticGlasses)
		{
			list.Add(1075085);// Requirement: Mondain's Legacy
		}

		#region Factions
		if (FactionItemState != null)
			list.Add(1041350); // faction item
		#endregion

		#region Mondain's Legacy Sets
		if (Core.ML && IsSetItem)
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

			if (SetEquipped)
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

		AddDamageTypeProperty(list);

		if (Core.ML && RequiredRace == Race.Elf)
		{
			list.Add(1075086); // Elves Only
		}
		else if (Core.SA && RequiredRace == Race.Gargoyle)
		{
			list.Add(1111709); // Gargoyles Only
		}

		m_AosSkillBonuses.GetProperties(list);

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

		if ((prop = m_AosArmorAttributes.DurabilityBonus) != 0)
		{
			list.Add(1151780, prop.ToString()); // durability +~1_VAL~%
		}

		if (m_TalismanProtection != null && !m_TalismanProtection.IsEmpty && m_TalismanProtection.Amount > 0)
		{
			list.Add(1072387, "{0}\t{1}", m_TalismanProtection.Name != null ? m_TalismanProtection.Name.ToString() : "Unknown", m_TalismanProtection.Amount); // ~1_NAME~ Protection: +~2_val~%
		}

		if (Attributes.SpellChanneling != 0)
		{
			list.Add(1060482); // spell channeling
		}

		if ((prop = m_AosArmorAttributes.SelfRepair) != 0)
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

		if ((prop = (GetLuckBonus() + Attributes.Luck)) != 0)
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

		AddResistanceProperties(list);

		if (m_AosArmorAttributes.MageArmor != 0)
		{
			list.Add(1060437); // mage armor
		}

		if ((prop = GetLowerStatReq()) != 0)
		{
			list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%
		}

		if ((prop = ComputeStatReq(StatType.Str)) > 0)
		{
			list.Add(1061170, prop.ToString()); // strength requirement ~1_val~
		}

		if (ShowDexandInt)
		{
			if ((prop = ComputeStatReq(StatType.Dex)) > 0)
				list.Add(1060658, "{0}\t{1}", "dexterity requirement", prop.ToString());

			if ((prop = ComputeStatReq(StatType.Int)) > 0)
				list.Add(1060662, "{0}\t{1}", "intelligence requirement", prop.ToString());
		}

		if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
		{
			list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
		}

		XmlAttach.AddAttachmentProperties(this, list);

		if (IsSetItem && !SetEquipped)
		{
			list.Add(1072378); // <br>Only when full set is present:				
			GetSetProperties(list);
		}
	}

	public override void AddItemPowerProperties(ObjectPropertyList list)
	{
	}

	#region ICraftable Members
	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Quality = (ItemQuality)quality;

		if (makersMark)
			Crafter = from;

		#region Mondain's Legacy
		if (!Core.ML || !craftItem.ForceNonExceptional)
		{
			if (typeRes == null)
			{
				typeRes = craftItem.Resources.GetAt(0).ItemType;
			}

			Resource = CraftResources.GetFromType(typeRes);
		}
		#endregion

		if (typeRes == null || craftItem.ForceNonExceptional)
		{
			_ = craftItem.Resources.GetAt(0).ItemType;
		}

		PlayerConstructed = true;

		if (Quality == ItemQuality.Exceptional && !craftItem.ForceNonExceptional)
		{
			DistributeExceptionalBonuses(from, tool is BaseRunicTool ? 6 : Core.SE ? 15 : 14); // Not sure since when, but right now 15 points are added, not 14.
		}

		if (Core.AOS && tool is BaseRunicTool runicTool)
			runicTool.ApplyAttributesTo(this);

		#region Mondain's Legacy
		if (Core.ML && !craftItem.ForceNonExceptional)
		{
			CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

			if (resInfo == null)
			{
				return quality;
			}

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if (attrInfo == null)
			{
				return quality;
			}

			DistributeMaterialBonus(attrInfo);
		}
		#endregion

		return quality;
	}

	public virtual void DistributeExceptionalBonuses(Mobile from, int amount)
	{
		// Exceptional Bonus
		for (int i = 0; i < amount; ++i)
		{
			switch (Utility.Random(5))
			{
				case 0: ++m_PhysicalBonus; break;
				case 1: ++m_FireBonus; break;
				case 2: ++m_ColdBonus; break;
				case 3: ++m_PoisonBonus; break;
				case 4: ++m_EnergyBonus; break;
			}
		}

		// Arms Lore Bonus
		if (Core.ML && from != null)
		{
			double div = 20;
			int bonus = (int)(from.Skills.ArmsLore.Value / div);

			for (int i = 0; i < bonus; i++)
			{
				switch (Utility.Random(5))
				{
					case 0: m_PhysicalBonus++; break;
					case 1: m_FireBonus++; break;
					case 2: m_ColdBonus++; break;
					case 3: m_EnergyBonus++; break;
					case 4: m_PoisonBonus++; break;
				}
			}

			from.CheckSkill(SkillName.ArmsLore, 0, 100);
		}

		// Gives MageArmor property for certain armor types
		if (Core.SA && m_AosArmorAttributes.MageArmor <= 0 && IsMageArmorType(this))
		{
			m_AosArmorAttributes.MageArmor = 1;
		}

		InvalidateProperties();
	}

	protected virtual void ApplyResourceResistances(CraftResource oldResource)
	{
		CraftAttributeInfo info;

		if (oldResource > CraftResource.None)
		{
			info = GetResourceAttrs(oldResource);

			// Remove old bonus
			m_PhysicalBonus = Math.Max(0, m_PhysicalBonus - info.ArmorPhysicalResist);
			m_FireBonus = Math.Max(0, m_FireBonus - info.ArmorFireResist);
			m_ColdBonus = Math.Max(0, m_ColdBonus - info.ArmorColdResist);
			m_PoisonBonus = Math.Max(0, m_PoisonBonus - info.ArmorPoisonResist);
			m_EnergyBonus = Math.Max(0, m_EnergyBonus - info.ArmorEnergyResist);
		}

		info = GetResourceAttrs(Resource);

		// add new bonus
		m_PhysicalBonus += info.ArmorPhysicalResist;
		m_FireBonus += info.ArmorFireResist;
		m_ColdBonus += info.ArmorColdResist;
		m_PoisonBonus += info.ArmorPoisonResist;
		m_EnergyBonus += info.ArmorEnergyResist;
	}

	public virtual void DistributeMaterialBonus(CraftAttributeInfo attrInfo)
	{
		if (Resource != CraftResource.Heartwood)
		{
			Attributes.WeaponDamage += attrInfo.ArmorDamage;
			Attributes.AttackChance += attrInfo.ArmorHitChance;
			Attributes.RegenHits += attrInfo.ArmorRegenHits;
			//m_AosArmorAttributes.MageArmor += attrInfo.ArmorMage;
		}
		else
		{
			switch (Utility.Random(4))
			{
				case 0: Attributes.WeaponDamage += attrInfo.ArmorDamage; break;
				case 1: Attributes.AttackChance += attrInfo.ArmorHitChance; break;
				//case 2: m_AosArmorAttributes.MageArmor += attrInfo.ArmorMage; break;
				case 2: Attributes.Luck += attrInfo.ArmorLuck; break;
				case 3: m_AosArmorAttributes.LowerStatReq += attrInfo.ArmorLowerRequirements; break;
			}
		}
	}

	public static CraftAttributeInfo GetResourceAttrs(CraftResource res)
	{
		CraftResourceInfo info = CraftResources.GetInfo(res);

		if (info == null)
		{
			return CraftAttributeInfo.Blank;
		}

		return info.AttributeInfo;
	}

	public static bool IsMageArmorType(BaseArmor armor)
	{
		Type t = armor.GetType();

		foreach (Type type in _MageArmorTypes)
		{
			if (type == t || t.IsSubclassOf(type))
			{
				return true;
			}
		}

		return false;
	}

	public static readonly Type[] _MageArmorTypes = new Type[]
	{
		typeof(HeavyPlateJingasa),  typeof(LightPlateJingasa),
		typeof(PlateMempo),         typeof(PlateDo),
		typeof(PlateHiroSode),      typeof(PlateSuneate),
		typeof(PlateHaidate)
	};

	#endregion

	#region Mondain's Legacy Sets
	public override bool OnDragLift(Mobile from)
	{
		if (Parent is Mobile && from == Parent)
		{
			if (IsSetItem && SetEquipped)
			{
				SetHelper.RemoveSetBonus(from, SetID, this);
			}
		}

		return base.OnDragLift(from);
	}

	public virtual SetItem SetID => SetItem.None;
	public virtual bool MixedSet => false;
	public virtual int Pieces => 0;

	public virtual bool BardMasteryBonus => SetID == SetItem.Virtuoso;

	public bool IsSetItem => SetID != SetItem.None;

	private int m_SetHue;

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

	public bool SetEquipped { get; set; }

	public bool LastEquipped { get; set; }

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
		SetHelper.GetSetProperties(list, this);

		if (!SetEquipped)
		{
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
		else if (SetEquipped && SetHelper.ResistsBonusPerPiece(this) && RootParent is Mobile mobile)
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
		else
		{
			if (m_SetPhysicalBonus != 0)
			{
				list.Add(1080361, ((BasePhysicalResistance * Pieces) + m_SetPhysicalBonus).ToString()); // physical resist ~1_val~% (total)
			}

			if (m_SetFireBonus != 0)
			{
				list.Add(1080362, ((BaseFireResistance * Pieces) + m_SetFireBonus).ToString()); // fire resist ~1_val~% (total)
			}

			if (m_SetColdBonus != 0)
			{
				list.Add(1080363, ((BaseColdResistance * Pieces) + m_SetColdBonus).ToString()); // cold resist ~1_val~% (total)
			}

			if (m_SetPoisonBonus != 0)
			{
				list.Add(1080364, ((BasePoisonResistance * Pieces) + m_SetPoisonBonus).ToString()); // poison resist ~1_val~% (total)
			}

			if (m_SetEnergyBonus != 0)
			{
				list.Add(1080365, ((BaseEnergyResistance * Pieces) + m_SetEnergyBonus).ToString()); // energy resist ~1_val~% (total)
			}
		}

		int prop;

		if ((prop = m_SetSelfRepair) != 0 && m_AosArmorAttributes.SelfRepair == 0)
		{
			list.Add(1060450, prop.ToString()); // self repair ~1_val~
		}
	}

	public int SetResistBonus(ResistanceType resist)
	{
		if (SetHelper.ResistsBonusPerPiece(this))
		{
			switch (resist)
			{
				case ResistanceType.Physical: return SetEquipped ? PhysicalResistance + m_SetPhysicalBonus : PhysicalResistance;
				case ResistanceType.Fire: return SetEquipped ? FireResistance + m_SetFireBonus : FireResistance;
				case ResistanceType.Cold: return SetEquipped ? ColdResistance + m_SetColdBonus : ColdResistance;
				case ResistanceType.Poison: return SetEquipped ? PoisonResistance + m_SetPoisonBonus : PoisonResistance;
				case ResistanceType.Energy: return SetEquipped ? EnergyResistance + m_SetEnergyBonus : EnergyResistance;
			}
		}
		else
		{
			switch (resist)
			{
				case ResistanceType.Physical: return SetEquipped ? LastEquipped ? (PhysicalResistance * Pieces) + m_SetPhysicalBonus : 0 : PhysicalResistance;
				case ResistanceType.Fire: return SetEquipped ? LastEquipped ? (FireResistance * Pieces) + m_SetFireBonus : 0 : FireResistance;
				case ResistanceType.Cold: return SetEquipped ? LastEquipped ? (ColdResistance * Pieces) + m_SetColdBonus : 0 : ColdResistance;
				case ResistanceType.Poison: return SetEquipped ? LastEquipped ? (PoisonResistance * Pieces) + m_SetPoisonBonus : 0 : PoisonResistance;
				case ResistanceType.Energy: return SetEquipped ? LastEquipped ? (EnergyResistance * Pieces) + m_SetEnergyBonus : 0 : EnergyResistance;
			}
		}

		return 0;
	}
	#endregion

	public virtual void SetProtection(Type type, TextDefinition name, int amount)
	{
		m_TalismanProtection = new TalismanAttribute(type, name, amount);
	}
}
