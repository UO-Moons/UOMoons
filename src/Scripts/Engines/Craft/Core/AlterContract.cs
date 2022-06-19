using Server.Engines.Craft;

namespace Server.Items;

public class AlterContract : BaseItem
{
	private RepairSkillType _mType;
	private string _mCrafterName;

	[CommandProperty(AccessLevel.GameMaster)]
	public RepairSkillType Type
	{
		get => _mType;

		set
		{
			_mType = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string CrafterName
	{
		get => _mCrafterName;

		set
		{
			_mCrafterName = value;
			InvalidateProperties();
		}
	}

	[Constructable]
	public AlterContract(RepairSkillType type, Mobile crafter)
		: base(0x14F0)
	{
		_mCrafterName = crafter.Name;
		Type = type;

		Hue = 0x1BC;
		Weight = 1.0;
	}

	public AlterContract(Serial serial)
		: base(serial)
	{
	}

	public string GetTitle()
	{
		return _mType switch
		{
			RepairSkillType.Smithing => "Blacksmithing",
			RepairSkillType.Carpentry => "Carpentry",
			RepairSkillType.Tailoring => "Tailoring",
			RepairSkillType.Tinkering => "Tinkering",
			_ => null
		};
	}

	public CraftSystem GetCraftSystem()
	{
		return _mType switch
		{
			RepairSkillType.Smithing => DefBlacksmithy.CraftSystem,
			RepairSkillType.Carpentry => DefCarpentry.CraftSystem,
			RepairSkillType.Tailoring => DefTailoring.CraftSystem,
			RepairSkillType.Tinkering => DefTinkering.CraftSystem,
			_ => null
		};
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		LabelTo(from, 1094795, GetTitle()); // An alter service contract (~1_SKILL_NAME~)
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1050043, _mCrafterName); // crafted by ~1_NAME~
		list.Add(1060636); // exceptional
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		list.Add(1094795, GetTitle());
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!IsChildOf(from.Backpack))
		{
			// The contract must be in your backpack to use it.
			from.SendLocalizedMessage(1047012);
		}
		else
		{
			CraftSystem cs = GetCraftSystem();

			AlterItem.BeginTarget(from, cs, this);
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write((int)_mType);
		writer.Write(_mCrafterName);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		_mType = (RepairSkillType)reader.ReadInt();
		_mCrafterName = reader.ReadString();
	}
}
