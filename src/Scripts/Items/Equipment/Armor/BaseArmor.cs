using Server.Engines.Craft;
using Server.Engines.XmlSpawner2;
using Server.Factions;
using Server.Misc;
using Server.Network;
using System;
using System.Linq;
using AMA = Server.Items.ArmorMeditationAllowance;
using AMT = Server.Items.ArmorMaterialType;

namespace Server.Items;

public abstract class BaseArmor : BaseEquipment, IScissorable, IFactionItem, ICraftable, IWearableDurability, IResource, ISetItem
{
	private const bool UseNewHits = true;
	private int _mMaxHitPoints;
	private int _mHitPoints;
	private DurabilityLevel _mDurability;
	private ArmorProtectionLevel _mProtection;
	private int _mPhysicalBonus, _mFireBonus, _mColdBonus, _mPoisonBonus, _mEnergyBonus;
	private AosArmorAttributes _mAosArmorAttributes;
	private AosSkillBonuses _mAosSkillBonuses;
	private AosWeaponAttributes _mAosWeaponAttributes;
	private TalismanAttribute _mTalismanProtection;
	private int _mArmorBase = -1;
	private int _mStrBonus = -1, _mDexBonus = -1, _mIntBonus = -1;
	private int _mStrReq = -1, _mDexReq = -1, _mIntReq = -1;
	private AMA _mMeditate = (AMA)(-1);
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
		get => _mMeditate == (AMA)(-1) ? DefMedAllowance : _mMeditate;
		set => _mMeditate = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int BaseArmorRating
	{
		get
		{
			if (_mArmorBase == -1)
			{
				return ArmorBase;
			}
			else
			{
				return _mArmorBase;
			}
		}
		set
		{
			_mArmorBase = value;
			Invalidate();
		}
	}

	public double BaseArmorRatingScaled => BaseArmorRating * ArmorScalar;
	public double ArmorRatingScaled => ArmorRating * ArmorScalar;

	[CommandProperty(AccessLevel.GameMaster)]
	public int StrBonus
	{
		get => _mStrBonus == -1 ? StrBonusValue : _mStrBonus;
		set { _mStrBonus = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int DexBonus
	{
		get => _mDexBonus == -1 ? DexBonusValue : _mDexBonus;
		set { _mDexBonus = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int IntBonus
	{
		get => _mIntBonus == -1 ? IntBonusValue : _mIntBonus;
		set { _mIntBonus = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int StrRequirement
	{
		get => _mStrReq == -1 ? StrReq : _mStrReq;
		set { _mStrReq = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int DexRequirement
	{
		get => _mDexReq == -1 ? DexReq : _mDexReq;
		set { _mDexReq = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int IntRequirement
	{
		get => _mIntReq == -1 ? IntReq : _mIntReq;
		set { _mIntReq = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public override CraftResource Resource
	{
		get => base.Resource;
		set
		{
			if (Resource == value) return;
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
		get => _mMaxHitPoints;
		set { _mMaxHitPoints = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitPoints
	{
		get => _mHitPoints;
		set
		{
			if (value == _mHitPoints || MaxHitPoints <= 0) return;
			_mHitPoints = value;

			if (_mHitPoints < 0)
				Delete();
			else if (_mHitPoints > MaxHitPoints)
				_mHitPoints = MaxHitPoints;

			InvalidateProperties();
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
		get => _mDurability;
		set { UnscaleDurability(); _mDurability = value; ScaleDurability(); InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public ArmorProtectionLevel ProtectionLevel
	{
		get => _mProtection;
		set
		{
			if (_mProtection == value) return;
			_mProtection = value;

			Invalidate();
			InvalidateProperties();

			if (Parent is Mobile mobile)
				mobile.UpdateResistances();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosArmorAttributes ArmorAttributes
	{
		get => _mAosArmorAttributes;
		set { }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosWeaponAttributes WeaponAttributes { get => _mAosWeaponAttributes; set { } }

	[CommandProperty(AccessLevel.GameMaster)]
	public TalismanAttribute Protection
	{
		get => _mTalismanProtection;
		set { _mTalismanProtection = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosSkillBonuses SkillBonuses
	{
		get => _mAosSkillBonuses;
		set { }
	}

	public virtual double ArmorRating
	{
		get
		{
			int ar = BaseArmorRating;

			if (_mProtection != ArmorProtectionLevel.Regular)
				ar += 10 + 5 * (int)_mProtection;

			switch (base.Resource)
			{
				case CraftResource.DullCopper:
					ar += 2;
					break;
				case CraftResource.ShadowIron:
					ar += 4;
					break;
				case CraftResource.Copper:
					ar += 6;
					break;
				case CraftResource.Bronze:
					ar += 8;
					break;
				case CraftResource.Gold:
					ar += 10;
					break;
				case CraftResource.Agapite:
					ar += 12;
					break;
				case CraftResource.Verite:
					ar += 14;
					break;
				case CraftResource.Valorite:
					ar += 16;
					break;
				case CraftResource.SpinedLeather:
					ar += 10;
					break;
				case CraftResource.HornedLeather:
					ar += 13;
					break;
				case CraftResource.BarbedLeather:
					ar += 16;
					break;
			}

			ar += -8 + 8 * (int)Quality;
			return ScaleArmorByDurability(ar);
		}
	}

	public override void OnAfterDuped(Item newItem)
	{
		base.OnAfterDuped(newItem);

		if (newItem is not BaseArmor armor) return;
		armor._mAosArmorAttributes = new AosArmorAttributes(newItem, _mAosArmorAttributes);
		armor._mAosSkillBonuses = new AosSkillBonuses(newItem, _mAosSkillBonuses);
		armor._mAosWeaponAttributes = new AosWeaponAttributes(newItem, _mAosWeaponAttributes);
		armor._mTalismanProtection = new TalismanAttribute(_mTalismanProtection);
		armor._mSetAttributes = new AosAttributes(newItem, _mSetAttributes);
		armor._mSetSkillBonuses = new AosSkillBonuses(newItem, _mSetSkillBonuses);
	}

	public override int ComputeStatReq(StatType type)
	{
		int v = type switch
		{
			StatType.Str => StrRequirement,
			StatType.Dex => DexRequirement,
			_ => IntRequirement
		};

		return AOS.Scale(v, 100 - GetLowerStatReq());
	}

	public override int ComputeStatBonus(StatType type)
	{
		return type switch
		{
			StatType.Str => StrBonus + Attributes.BonusStr,
			StatType.Dex => DexBonus + Attributes.BonusDex,
			_ => IntBonus + Attributes.BonusInt
		};
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int PhysicalBonus { get => _mPhysicalBonus; set { _mPhysicalBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int FireBonus { get => _mFireBonus; set { _mFireBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ColdBonus { get => _mColdBonus; set { _mColdBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int PoisonBonus { get => _mPoisonBonus; set { _mPoisonBonus = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int EnergyBonus { get => _mEnergyBonus; set { _mEnergyBonus = value; InvalidateProperties(); } }

	public virtual int BasePhysicalResistance => 0;
	public virtual int BaseFireResistance => 0;
	public virtual int BaseColdResistance => 0;
	public virtual int BasePoisonResistance => 0;
	public virtual int BaseEnergyResistance => 0;

	public override int PhysicalResistance => BasePhysicalResistance + GetProtOffset() + GetResourceAttrs().ArmorPhysicalResist + _mPhysicalBonus;
	public override int FireResistance => BaseFireResistance + GetProtOffset() + GetResourceAttrs().ArmorFireResist + _mFireBonus;
	public override int ColdResistance => BaseColdResistance + GetProtOffset() + GetResourceAttrs().ArmorColdResist + _mColdBonus;
	public override int PoisonResistance => BasePoisonResistance + GetProtOffset() + GetResourceAttrs().ArmorPoisonResist + _mPoisonBonus;
	public override int EnergyResistance => BaseEnergyResistance + GetProtOffset() + GetResourceAttrs().ArmorEnergyResist + _mEnergyBonus;

	public virtual int InitMinHits => 0;
	public virtual int InitMaxHits => 0;
	public virtual int InitHits => Utility.RandomMinMax(InitMinHits, InitMaxHits);

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
		for (var i = 0; i < amount; ++i)
		{
			switch (Utility.Random(5))
			{
				case 0: ++_mPhysicalBonus; break;
				case 1: ++_mFireBonus; break;
				case 2: ++_mColdBonus; break;
				case 3: ++_mPoisonBonus; break;
				case 4: ++_mEnergyBonus; break;
			}
		}

		InvalidateProperties();
	}

	public CraftAttributeInfo GetResourceAttrs()
	{
		CraftResourceInfo info = CraftResources.GetInfo(Resource);

		return info == null ? CraftAttributeInfo.Blank : info.AttributeInfo;
	}

	public int GetProtOffset()
	{
		switch (_mProtection)
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

		_mHitPoints = (_mHitPoints * 100 + (scale - 1)) / scale;
		_mMaxHitPoints = (_mMaxHitPoints * 100 + (scale - 1)) / scale;
		InvalidateProperties();
	}

	public void ScaleDurability()
	{
		int scale = 100 + GetDurabilityBonus();

		_mHitPoints = (_mHitPoints * scale + 99) / 100;
		_mMaxHitPoints = (_mMaxHitPoints * scale + 99) / 100;
		InvalidateProperties();
	}

	public int GetDurabilityBonus()
	{
		int bonus = 0;

		if (Quality == ItemQuality.Exceptional)
			bonus += 20;

		switch (_mDurability)
		{
			case DurabilityLevel.Durable: bonus += 20; break;
			case DurabilityLevel.Substantial: bonus += 50; break;
			case DurabilityLevel.Massive: bonus += 70; break;
			case DurabilityLevel.Fortified: bonus += 100; break;
			case DurabilityLevel.Indestructible: bonus += 120; break;
			case DurabilityLevel.Regular:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if (!Core.AOS) return bonus;
		bonus += _mAosArmorAttributes.DurabilityBonus;

		CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);
		CraftAttributeInfo attrInfo = null;

		if (resInfo != null)
			attrInfo = resInfo.AttributeInfo;

		if (attrInfo != null)
			bonus += attrInfo.ArmorDurability;

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
				// ignored
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

		int v = _mAosArmorAttributes.LowerStatReq;

		CraftResourceInfo info = CraftResources.GetInfo(Resource);

		CraftAttributeInfo attrInfo = info?.AttributeInfo;

		if (attrInfo != null)
			v += attrInfo.ArmorLowerRequirements;

		if (v > 100)
			v = 100;

		return v;
	}

	public override void OnAdded(IEntity parent)
	{
		if (parent is not Mobile from) return;

		if (Core.AOS)
			_mAosSkillBonuses.AddTo(from);

		#region Mondain's Legacy Sets
		if (Core.ML && IsSetItem)
		{
			SetEquipped = SetHelper.FullSetEquipped(from, SetId, Pieces);

			if (SetEquipped)
			{
				LastEquipped = true;
				SetHelper.AddSetBonus(from, SetId);
			}
		}
		#endregion

		from.Delta(MobileDelta.Armor); // Tell them armor rating has changed
	}

	public virtual double ScaleArmorByDurability(double armor)
	{
		int scale = 100;

		if (_mMaxHitPoints > 0 && _mHitPoints < _mMaxHitPoints)
			scale = 50 + 50 * _mHitPoints / _mMaxHitPoints;
		return armor * scale / 100;
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
		XWeaponAttributes = 0x00020000,
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
			flags = SaveFlag.XWeaponAttributes;
		}

		_mAosWeaponAttributes = flags.HasFlag(SaveFlag.XWeaponAttributes) ? new AosWeaponAttributes(item, reader) : new AosWeaponAttributes(item);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		SetFlag sflags = SetFlag.None;

		Utility.SetSaveFlag(ref sflags, SetFlag.Attributes, !_mSetAttributes.IsEmpty);
		Utility.SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !_mSetSkillBonuses.IsEmpty);
		Utility.SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, _mSetPhysicalBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.FireBonus, _mSetFireBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.ColdBonus, _mSetColdBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.PoisonBonus, _mSetPoisonBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.EnergyBonus, _mSetEnergyBonus != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.Hue, _mSetHue != 0);
		Utility.SetSaveFlag(ref sflags, SetFlag.LastEquipped, LastEquipped);
		Utility.SetSaveFlag(ref sflags, SetFlag.SetEquipped, SetEquipped);
		Utility.SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, _mSetSelfRepair != 0);

		writer.WriteEncodedInt((int)sflags);

		if (sflags.HasFlag(SetFlag.Attributes))
		{
			_mSetAttributes.Serialize(writer);
		}

		if (sflags.HasFlag(SetFlag.SkillBonuses))
		{
			_mSetSkillBonuses.Serialize(writer);
		}

		if (sflags.HasFlag(SetFlag.PhysicalBonus))
		{
			writer.WriteEncodedInt(_mSetPhysicalBonus);
		}

		if (sflags.HasFlag(SetFlag.FireBonus))
		{
			writer.WriteEncodedInt(_mSetFireBonus);
		}

		if (sflags.HasFlag(SetFlag.ColdBonus))
		{
			writer.WriteEncodedInt(_mSetColdBonus);
		}

		if (sflags.HasFlag(SetFlag.PoisonBonus))
		{
			writer.WriteEncodedInt(_mSetPoisonBonus);
		}

		if (sflags.HasFlag(SetFlag.EnergyBonus))
		{
			writer.WriteEncodedInt(_mSetEnergyBonus);
		}

		if (sflags.HasFlag(SetFlag.Hue))
		{
			writer.WriteEncodedInt(_mSetHue);
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
			writer.WriteEncodedInt(_mSetSelfRepair);
		}

		SaveFlag flags = SaveFlag.None;

		Utility.SetSaveFlag(ref flags, SaveFlag.XWeaponAttributes, !_mAosWeaponAttributes.IsEmpty);
		Utility.SetSaveFlag(ref flags, SaveFlag.ArmorAttributes, !_mAosArmorAttributes.IsEmpty);
		Utility.SetSaveFlag(ref flags, SaveFlag.PhysicalBonus, _mPhysicalBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.FireBonus, _mFireBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.ColdBonus, _mColdBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.PoisonBonus, _mPoisonBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.EnergyBonus, _mEnergyBonus != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, _mMaxHitPoints != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.HitPoints, _mHitPoints != 0);
		Utility.SetSaveFlag(ref flags, SaveFlag.Durability, _mDurability != DurabilityLevel.Regular);
		Utility.SetSaveFlag(ref flags, SaveFlag.Protection, _mProtection != ArmorProtectionLevel.Regular);
		Utility.SetSaveFlag(ref flags, SaveFlag.BaseArmor, _mArmorBase != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.StrBonus, _mStrBonus != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.DexBonus, _mDexBonus != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.IntBonus, _mIntBonus != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, _mStrReq != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.DexReq, _mDexReq != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.IntReq, _mIntReq != -1);
		Utility.SetSaveFlag(ref flags, SaveFlag.MedAllowance, _mMeditate != (AMA)(-1));
		Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !_mAosSkillBonuses.IsEmpty);

		writer.WriteEncodedInt((int)flags);

		if (flags.HasFlag(SaveFlag.XWeaponAttributes))
		{
			_mAosWeaponAttributes.Serialize(writer);
		}

		if (flags.HasFlag(SaveFlag.TalismanProtection))
		{
			_mTalismanProtection.Serialize(writer);
		}

		if (flags.HasFlag(SaveFlag.ArmorAttributes))
			_mAosArmorAttributes.Serialize(writer);

		if (flags.HasFlag(SaveFlag.PhysicalBonus))
			writer.WriteEncodedInt(_mPhysicalBonus);

		if (flags.HasFlag(SaveFlag.FireBonus))
			writer.WriteEncodedInt(_mFireBonus);

		if (flags.HasFlag(SaveFlag.ColdBonus))
			writer.WriteEncodedInt(_mColdBonus);

		if (flags.HasFlag(SaveFlag.PoisonBonus))
			writer.WriteEncodedInt(_mPoisonBonus);

		if (flags.HasFlag(SaveFlag.EnergyBonus))
			writer.WriteEncodedInt(_mEnergyBonus);

		if (flags.HasFlag(SaveFlag.MaxHitPoints))
			writer.WriteEncodedInt(_mMaxHitPoints);

		if (flags.HasFlag(SaveFlag.HitPoints))
			writer.WriteEncodedInt(_mHitPoints);

		if (flags.HasFlag(SaveFlag.Durability))
			writer.WriteEncodedInt((int)_mDurability);

		if (flags.HasFlag(SaveFlag.Protection))
			writer.WriteEncodedInt((int)_mProtection);

		if (flags.HasFlag(SaveFlag.BaseArmor))
			writer.WriteEncodedInt(_mArmorBase);

		if (flags.HasFlag(SaveFlag.StrBonus))
			writer.WriteEncodedInt(_mStrBonus);

		if (flags.HasFlag(SaveFlag.DexBonus))
			writer.WriteEncodedInt(_mDexBonus);

		if (flags.HasFlag(SaveFlag.IntBonus))
			writer.WriteEncodedInt(_mIntBonus);

		if (flags.HasFlag(SaveFlag.StrReq))
			writer.WriteEncodedInt(_mStrReq);

		if (flags.HasFlag(SaveFlag.DexReq))
			writer.WriteEncodedInt(_mDexReq);

		if (flags.HasFlag(SaveFlag.IntReq))
			writer.WriteEncodedInt(_mIntReq);

		if (flags.HasFlag(SaveFlag.MedAllowance))
			writer.WriteEncodedInt((int)_mMeditate);

		if (flags.HasFlag(SaveFlag.SkillBonuses))
			_mAosSkillBonuses.Serialize(writer);
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

					_mSetAttributes = sflags.HasFlag(SetFlag.Attributes) ? new AosAttributes(this, reader) : new AosAttributes(this);

					if (sflags.HasFlag(SetFlag.ArmorAttributes))
					{
						_mSetSelfRepair = (new AosArmorAttributes(this, reader)).SelfRepair;
					}

					_mSetSkillBonuses = sflags.HasFlag(SetFlag.SkillBonuses) ? new AosSkillBonuses(this, reader) : new AosSkillBonuses(this);

					if (sflags.HasFlag(SetFlag.PhysicalBonus))
					{
						_mSetPhysicalBonus = reader.ReadEncodedInt();
					}

					if (sflags.HasFlag(SetFlag.FireBonus))
					{
						_mSetFireBonus = reader.ReadEncodedInt();
					}

					if (sflags.HasFlag(SetFlag.ColdBonus))
					{
						_mSetColdBonus = reader.ReadEncodedInt();
					}

					if (sflags.HasFlag(SetFlag.PoisonBonus))
					{
						_mSetPoisonBonus = reader.ReadEncodedInt();
					}

					if (sflags.HasFlag(SetFlag.EnergyBonus))
					{
						_mSetEnergyBonus = reader.ReadEncodedInt();
					}

					if (sflags.HasFlag(SetFlag.Hue))
					{
						_mSetHue = reader.ReadEncodedInt();
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
						_mSetSelfRepair = reader.ReadEncodedInt();
					}

					SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

					_mAosWeaponAttributes = flags.HasFlag(SaveFlag.XWeaponAttributes) ? new AosWeaponAttributes(this, reader) : new AosWeaponAttributes(this);

					_mTalismanProtection = flags.HasFlag(SaveFlag.TalismanProtection) ? new TalismanAttribute(reader) : new TalismanAttribute();

					_mAosArmorAttributes = flags.HasFlag(SaveFlag.ArmorAttributes) ? new AosArmorAttributes(this, reader) : new AosArmorAttributes(this);

					if (flags.HasFlag(SaveFlag.PhysicalBonus))
						_mPhysicalBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.FireBonus))
						_mFireBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.ColdBonus))
						_mColdBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.PoisonBonus))
						_mPoisonBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.EnergyBonus))
						_mEnergyBonus = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.MaxHitPoints))
						_mMaxHitPoints = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.HitPoints))
						_mHitPoints = reader.ReadEncodedInt();

					if (flags.HasFlag(SaveFlag.Durability))
					{
						_mDurability = (DurabilityLevel)reader.ReadEncodedInt();

						if (_mDurability > DurabilityLevel.Indestructible)
							_mDurability = DurabilityLevel.Durable;
					}

					if (flags.HasFlag(SaveFlag.Protection))
					{
						_mProtection = (ArmorProtectionLevel)reader.ReadEncodedInt();

						if (_mProtection > ArmorProtectionLevel.Invulnerability)
							_mProtection = ArmorProtectionLevel.Defense;
					}

					_mArmorBase = flags.HasFlag(SaveFlag.BaseArmor) ? reader.ReadEncodedInt() : -1;

					_mStrBonus = flags.HasFlag(SaveFlag.StrBonus) ? reader.ReadEncodedInt() : -1;

					_mDexBonus = flags.HasFlag(SaveFlag.DexBonus) ? reader.ReadEncodedInt() : -1;

					_mIntBonus = flags.HasFlag(SaveFlag.IntBonus) ? reader.ReadEncodedInt() : -1;

					_mStrReq = flags.HasFlag(SaveFlag.StrReq) ? reader.ReadEncodedInt() : -1;

					_mDexReq = flags.HasFlag(SaveFlag.DexReq) ? reader.ReadEncodedInt() : -1;

					_mIntReq = flags.HasFlag(SaveFlag.IntReq) ? reader.ReadEncodedInt() : -1;

					_mMeditate = flags.HasFlag(SaveFlag.MedAllowance) ? (AMA) reader.ReadEncodedInt() : (AMA) (-1);

					if (flags.HasFlag(SaveFlag.SkillBonuses))
						_mAosSkillBonuses = new AosSkillBonuses(this, reader);

					break;
				}
		}

		#region Mondain's Legacy Sets
		_mSetAttributes ??= new AosAttributes(this);

		_mSetSkillBonuses ??= new AosSkillBonuses(this);
		#endregion

		_mAosSkillBonuses ??= new AosSkillBonuses(this);

		if (Core.AOS && Parent is Mobile mobile)
			_mAosSkillBonuses.AddTo(mobile);

		if (Parent is not Mobile mob) return;
		AddStatBonuses(mob);
		mob.CheckStatTimers();
	}
	#endregion

	public virtual CraftResource DefaultResource => CraftResource.Iron;

	public BaseArmor(int itemId) : base(itemId)
	{
		Layer = (Layer)ItemData.Quality;
		_mDurability = DurabilityLevel.Regular;
		base.Resource = DefaultResource;
		Hue = CraftResources.GetHue(Resource);
		if (UseNewHits)
		{
			_mHitPoints = _mMaxHitPoints = InitHits;
		}
		else
			_mHitPoints = _mMaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
		_mAosArmorAttributes = new AosArmorAttributes(this);
		_mAosSkillBonuses = new AosSkillBonuses(this);
		_mSetAttributes = new AosAttributes(this);
		_mSetSkillBonuses = new AosSkillBonuses(this);
		_mAosWeaponAttributes = new AosWeaponAttributes(this);
		_mTalismanProtection = new TalismanAttribute();
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
			if (this is IAccountRestricted {Account: { }} restricted)
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

			if (!AllowMaleWearer && !from.Female)
			{
				if (AllowFemaleWearer)
					from.SendLocalizedMessage(1010388); // Only females can wear this.
				else
					from.SendMessage("You may not wear this.");

				return false;
			}

			if (!AllowFemaleWearer && from.Female)
			{
				if (AllowMaleWearer)
					from.SendLocalizedMessage(1063343); // Only males can wear this.
				else
					from.SendMessage("You may not wear this.");

				return false;
			}

			int strBonus = ComputeStatBonus(StatType.Str), strReq = ComputeStatReq(StatType.Str);
			int dexBonus = ComputeStatBonus(StatType.Dex), dexReq = ComputeStatReq(StatType.Dex);
			int intBonus = ComputeStatBonus(StatType.Int), intReq = ComputeStatReq(StatType.Int);

			if (from.Dex < dexReq || from.Dex + dexBonus < 1)
			{
				from.SendLocalizedMessage(502077); // You do not have enough dexterity to equip this item.
				return false;
			}

			if (from.Str < strReq || from.Str + strBonus < 1)
			{
				from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
				return false;
			}

			if (from.Int < intReq || from.Int + intBonus < 1)
			{
				from.SendMessage("You are not smart enough to equip that.");
				return false;
			}
		}

		if (!XmlAttach.CheckCanEquip(this, from))
		{
			return false;
		}

		return base.CanEquip(from);
	}

	public override bool CheckPropertyConfliction(Mobile m)
	{
		if (base.CheckPropertyConfliction(m))
			return true;

		return Layer switch
		{
			Layer.Pants => m.FindItemOnLayer(Layer.InnerLegs) != null,
			Layer.Shirt => m.FindItemOnLayer(Layer.InnerTorso) != null,
			_ => false
		};
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
				_mAosSkillBonuses.Remove();

			m.Delta(MobileDelta.Armor); // Tell them armor rating has changed
			m.CheckStatTimers();

			#region Mondain's Legacy Sets
			if (Core.ML && IsSetItem && SetEquipped)
			{
				SetHelper.RemoveSetBonus(m, SetId, this);
			}
			#endregion
		}
		XmlAttach.CheckOnRemoved(this, parent);
		base.OnRemoved(parent);
	}

	public virtual int OnHit(BaseWeapon weapon, int damageTaken)
	{
		double halfAr = ArmorRating / 2.0;
		int absorbed = (int)(halfAr + halfAr * Utility.RandomDouble());

		damageTaken -= absorbed;
		if (damageTaken < 0)
			damageTaken = 0;

		if (absorbed < 2)
			absorbed = 2;
		double chance = 25;
		//double chance = NegativeAttributes.Antique > 0 ? 80 : 25;
		// if (chance >= Utility.Random(100)) // 25% chance to lower durability
		if (chance >= Utility.Random(100)) // 25% chance to lower durability
		{
			if (Core.AOS && _mAosArmorAttributes.SelfRepair > Utility.Random(10))
			{
				HitPoints += 2;
			}
			else
			{
				int wear;

				if (weapon.Type == WeaponType.Bashing)
					wear = absorbed / 2;
				else
					wear = Utility.Random(2);

				if (wear > 0 && _mMaxHitPoints > 0)
				{
					if (_mHitPoints >= wear)
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
						if (_mMaxHitPoints > wear)
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

		CraftAttributeInfo attrInfo = resInfo?.AttributeInfo;

		return attrInfo?.ArmorLuck ?? 0;
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
			list.Add(MixedSet ? 1073491 : 1072376, Pieces.ToString());

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

		_mAosSkillBonuses.GetProperties(list);

		int prop;

		if ((prop = ArtifactRarity) > 0)
		{
			list.Add(1061078, prop.ToString()); // artifact rarity ~1_val~
		}

		if ((prop = _mAosWeaponAttributes.HitColdArea) != 0)
		{
			list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitDispel) != 0)
		{
			list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitEnergyArea) != 0)
		{
			list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitFireArea) != 0)
		{
			list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitFireball) != 0)
		{
			list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitHarm) != 0)
		{
			list.Add(1060421, prop.ToString()); // hit harm ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitLeechHits) != 0)
		{
			list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitLightning) != 0)
		{
			list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitLowerAttack) != 0)
		{
			list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitLowerDefend) != 0)
		{
			list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitMagicArrow) != 0)
		{
			list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitLeechMana) != 0)
		{
			list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitPhysicalArea) != 0)
		{
			list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitPoisonArea) != 0)
		{
			list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%
		}

		if ((prop = _mAosWeaponAttributes.HitLeechStam) != 0)
		{
			list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%
		}

		if ((prop = _mAosArmorAttributes.DurabilityBonus) != 0)
		{
			list.Add(1151780, prop.ToString()); // durability +~1_VAL~%
		}

		if (_mTalismanProtection is {IsEmpty: false, Amount: > 0})
		{
			list.Add(1072387, "{0}\t{1}", _mTalismanProtection.Name != null ? _mTalismanProtection.Name.ToString() : "Unknown", _mTalismanProtection.Amount); // ~1_NAME~ Protection: +~2_val~%
		}

		if (Attributes.SpellChanneling != 0)
		{
			list.Add(1060482); // spell channeling
		}

		if ((prop = _mAosArmorAttributes.SelfRepair) != 0)
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

		if (_mAosArmorAttributes.MageArmor != 0)
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

		if (_mHitPoints >= 0 && _mMaxHitPoints > 0)
		{
			list.Add(1060639, "{0}\t{1}", _mHitPoints, _mMaxHitPoints); // durability ~1_val~ / ~2_val~
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
			typeRes ??= craftItem.Resources.GetAt(0).ItemType;

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
				case 0: ++_mPhysicalBonus; break;
				case 1: ++_mFireBonus; break;
				case 2: ++_mColdBonus; break;
				case 3: ++_mPoisonBonus; break;
				case 4: ++_mEnergyBonus; break;
			}
		}

		// Arms Lore Bonus
		if (Core.ML && from != null)
		{
			const double div = 20;
			int bonus = (int)(from.Skills.ArmsLore.Value / div);

			for (var i = 0; i < bonus; i++)
			{
				switch (Utility.Random(5))
				{
					case 0: _mPhysicalBonus++; break;
					case 1: _mFireBonus++; break;
					case 2: _mColdBonus++; break;
					case 3: _mEnergyBonus++; break;
					case 4: _mPoisonBonus++; break;
				}
			}

			from.CheckSkill(SkillName.ArmsLore, 0, 100);
		}

		// Gives MageArmor property for certain armor types
		if (Core.SA && _mAosArmorAttributes.MageArmor <= 0 && IsMageArmorType(this))
		{
			_mAosArmorAttributes.MageArmor = 1;
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
			_mPhysicalBonus = Math.Max(0, _mPhysicalBonus - info.ArmorPhysicalResist);
			_mFireBonus = Math.Max(0, _mFireBonus - info.ArmorFireResist);
			_mColdBonus = Math.Max(0, _mColdBonus - info.ArmorColdResist);
			_mPoisonBonus = Math.Max(0, _mPoisonBonus - info.ArmorPoisonResist);
			_mEnergyBonus = Math.Max(0, _mEnergyBonus - info.ArmorEnergyResist);
		}

		info = GetResourceAttrs(Resource);

		// add new bonus
		_mPhysicalBonus += info.ArmorPhysicalResist;
		_mFireBonus += info.ArmorFireResist;
		_mColdBonus += info.ArmorColdResist;
		_mPoisonBonus += info.ArmorPoisonResist;
		_mEnergyBonus += info.ArmorEnergyResist;
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
				case 3: _mAosArmorAttributes.LowerStatReq += attrInfo.ArmorLowerRequirements; break;
			}
		}
	}

	public static CraftAttributeInfo GetResourceAttrs(CraftResource res)
	{
		CraftResourceInfo info = CraftResources.GetInfo(res);

		return info == null ? CraftAttributeInfo.Blank : info.AttributeInfo;
	}

	public static bool IsMageArmorType(BaseArmor armor)
	{
		Type t = armor.GetType();

		return MageArmorTypes.Any(type => type == t || t.IsSubclassOf(type));
	}

	public static readonly Type[] MageArmorTypes = {
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
				SetHelper.RemoveSetBonus(from, SetId, this);
			}
		}

		return base.OnDragLift(from);
	}

	public virtual SetItem SetId => SetItem.None;
	public virtual bool MixedSet => false;
	public virtual int Pieces => 0;

	public virtual bool BardMasteryBonus => SetId == SetItem.Virtuoso;

	public bool IsSetItem => SetId != SetItem.None;

	private int _mSetHue;

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetHue
	{
		get => _mSetHue;
		set
		{
			_mSetHue = value;
			InvalidateProperties();
		}
	}

	public bool SetEquipped { get; set; }

	public bool LastEquipped { get; set; }

	private AosAttributes _mSetAttributes;
	private AosSkillBonuses _mSetSkillBonuses;
	private int _mSetSelfRepair;

	[CommandProperty(AccessLevel.GameMaster)]
	public AosAttributes SetAttributes
	{
		get => _mSetAttributes;
		set
		{
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AosSkillBonuses SetSkillBonuses
	{
		get => _mSetSkillBonuses;
		set
		{
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetSelfRepair
	{
		get => _mSetSelfRepair;
		set
		{
			_mSetSelfRepair = value;
			InvalidateProperties();
		}
	}

	private int _mSetPhysicalBonus, _mSetFireBonus, _mSetColdBonus, _mSetPoisonBonus, _mSetEnergyBonus;

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetPhysicalBonus
	{
		get => _mSetPhysicalBonus;
		set
		{
			_mSetPhysicalBonus = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetFireBonus
	{
		get => _mSetFireBonus;
		set
		{
			_mSetFireBonus = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetColdBonus
	{
		get => _mSetColdBonus;
		set
		{
			_mSetColdBonus = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetPoisonBonus
	{
		get => _mSetPoisonBonus;
		set
		{
			_mSetPoisonBonus = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SetEnergyBonus
	{
		get => _mSetEnergyBonus;
		set
		{
			_mSetEnergyBonus = value;
			InvalidateProperties();
		}
	}

	public virtual void GetSetProperties(ObjectPropertyList list)
	{
		SetHelper.GetSetProperties(list, this);

		if (!SetEquipped)
		{
			if (_mSetPhysicalBonus != 0)
			{
				list.Add(1072382, _mSetPhysicalBonus.ToString()); // physical resist +~1_val~%
			}

			if (_mSetFireBonus != 0)
			{
				list.Add(1072383, _mSetFireBonus.ToString()); // fire resist +~1_val~%
			}

			if (_mSetColdBonus != 0)
			{
				list.Add(1072384, _mSetColdBonus.ToString()); // cold resist +~1_val~%
			}

			if (_mSetPoisonBonus != 0)
			{
				list.Add(1072385, _mSetPoisonBonus.ToString()); // poison resist +~1_val~%
			}

			if (_mSetEnergyBonus != 0)
			{
				list.Add(1072386, _mSetEnergyBonus.ToString()); // energy resist +~1_val~%		
			}
		}
		else if (SetEquipped && SetHelper.ResistsBonusPerPiece(this) && RootParent is Mobile mobile)
		{
			if (_mSetPhysicalBonus != 0)
			{
				list.Add(1080361, SetHelper.GetSetTotalResist(mobile, ResistanceType.Physical).ToString()); // physical resist ~1_val~% (total)
			}

			if (_mSetFireBonus != 0)
			{
				list.Add(1080362, SetHelper.GetSetTotalResist(mobile, ResistanceType.Fire).ToString()); // fire resist ~1_val~% (total)
			}

			if (_mSetColdBonus != 0)
			{
				list.Add(1080363, SetHelper.GetSetTotalResist(mobile, ResistanceType.Cold).ToString()); // cold resist ~1_val~% (total)
			}

			if (_mSetPoisonBonus != 0)
			{
				list.Add(1080364, SetHelper.GetSetTotalResist(mobile, ResistanceType.Poison).ToString()); // poison resist ~1_val~% (total)
			}

			if (_mSetEnergyBonus != 0)
			{
				list.Add(1080365, SetHelper.GetSetTotalResist(mobile, ResistanceType.Energy).ToString()); // energy resist ~1_val~% (total)
			}
		}
		else
		{
			if (_mSetPhysicalBonus != 0)
			{
				list.Add(1080361, ((BasePhysicalResistance * Pieces) + _mSetPhysicalBonus).ToString()); // physical resist ~1_val~% (total)
			}

			if (_mSetFireBonus != 0)
			{
				list.Add(1080362, ((BaseFireResistance * Pieces) + _mSetFireBonus).ToString()); // fire resist ~1_val~% (total)
			}

			if (_mSetColdBonus != 0)
			{
				list.Add(1080363, ((BaseColdResistance * Pieces) + _mSetColdBonus).ToString()); // cold resist ~1_val~% (total)
			}

			if (_mSetPoisonBonus != 0)
			{
				list.Add(1080364, ((BasePoisonResistance * Pieces) + _mSetPoisonBonus).ToString()); // poison resist ~1_val~% (total)
			}

			if (_mSetEnergyBonus != 0)
			{
				list.Add(1080365, ((BaseEnergyResistance * Pieces) + _mSetEnergyBonus).ToString()); // energy resist ~1_val~% (total)
			}
		}

		int prop;

		if ((prop = _mSetSelfRepair) != 0 && _mAosArmorAttributes.SelfRepair == 0)
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
				case ResistanceType.Physical: return SetEquipped ? PhysicalResistance + _mSetPhysicalBonus : PhysicalResistance;
				case ResistanceType.Fire: return SetEquipped ? FireResistance + _mSetFireBonus : FireResistance;
				case ResistanceType.Cold: return SetEquipped ? ColdResistance + _mSetColdBonus : ColdResistance;
				case ResistanceType.Poison: return SetEquipped ? PoisonResistance + _mSetPoisonBonus : PoisonResistance;
				case ResistanceType.Energy: return SetEquipped ? EnergyResistance + _mSetEnergyBonus : EnergyResistance;
			}
		}
		else
		{
			switch (resist)
			{
				case ResistanceType.Physical: return SetEquipped ? LastEquipped ? (PhysicalResistance * Pieces) + _mSetPhysicalBonus : 0 : PhysicalResistance;
				case ResistanceType.Fire: return SetEquipped ? LastEquipped ? (FireResistance * Pieces) + _mSetFireBonus : 0 : FireResistance;
				case ResistanceType.Cold: return SetEquipped ? LastEquipped ? (ColdResistance * Pieces) + _mSetColdBonus : 0 : ColdResistance;
				case ResistanceType.Poison: return SetEquipped ? LastEquipped ? (PoisonResistance * Pieces) + _mSetPoisonBonus : 0 : PoisonResistance;
				case ResistanceType.Energy: return SetEquipped ? LastEquipped ? (EnergyResistance * Pieces) + _mSetEnergyBonus : 0 : EnergyResistance;
			}
		}

		return 0;
	}
	#endregion

	public virtual void SetProtection(Type type, TextDefinition name, int amount)
	{
		_mTalismanProtection = new TalismanAttribute(type, name, amount);
	}
}
