using Server.Gumps;

namespace Server.Items;

public enum TrellisType
{
	Leafy,
	Rose,
	Bigflow
}

public class FloweredTrellisAddon : BaseAddon
{
	public TrellisType Type { get; set; }

	[Constructable]
	public FloweredTrellisAddon(TrellisType type, bool south)
	{
		Type = type;
		int[,] list = type switch
		{
			TrellisType.Rose => south ? m_RoseSouth : m_RoseEast,
			TrellisType.Bigflow => south ? m_BigflowSouth : m_BigflowEast,
			_ => south ? m_LeafySouth : m_LeafyEast,
		};
		for (var i = 0; i < list.Length / 4; i++)
			AddComponent(new AddonComponent(list[i, 0]), list[i, 1], list[i, 2], list[i, 3]);
	}

	private static readonly int[,] m_RoseEast = {
		{40585, 0, 0, 0}, {40586, -1, 0, 0}, {40587, -2, 0, 0}, {40588, -3, 0, 0}
	};

	private static readonly int[,] m_RoseSouth = {
		{40600, 0, 0, 0}, {40601, 0, -1, 0}, {40602, 0, -2, 0}, {40603, 0, -3, 0}
	};

	private static readonly int[,] m_LeafyEast =
	{
		{40645, 0, 0, 0}, {40646, -1, 0, 0}, {40647, -2, 0, 0}, {40648, -3, 0, 0}
	};

	private static readonly int[,] m_LeafySouth =
	{
		{40666, 0, 0, 0}, {40665, 0, -1, 0}, {40664, 0, -2, 0}, {40663, 0, -3, 0}
	};

	private static readonly int[,] m_BigflowEast =
	{
		{40613, 0, 0, 0}, {40614, -1, 0, 0}, {40615, -2, 0, 0}, {40616, -3, 0, 0},
		{40617, -4, 0, 0}
	};

	private static readonly int[,] m_BigflowSouth =
	{
		{40649, 0, 0, 0}, {40650, 0, -1, 0}, {40651, 0, -2, 0}, {40652, 0, -3, 0},
		{40653, 0, -4, 0}
	};

	public FloweredTrellisAddon(Serial serial)
		: base(serial)
	{
	}

	public override BaseAddonDeed Deed => new FloweredTrellisDeed(Type);

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write((int)Type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		Type = (TrellisType)reader.ReadInt();
	}
}

public class FloweredTrellisDeed : BaseAddonDeed, IRewardOption
{
	public override int LabelNumber => 1124637; // Flowered Trellis

	[CommandProperty(AccessLevel.GameMaster)]
	public TrellisType Type { get; set; }

	[Constructable]
	public FloweredTrellisDeed(TrellisType type)
	{
		Type = type;
		LootType = LootType.Blessed;
	}

	public FloweredTrellisDeed(Serial serial)
		: base(serial)
	{
	}

	public override BaseAddon Addon => new FloweredTrellisAddon(Type, _south);

	private bool _south;

	public void GetOptions(RewardOptionList list)
	{
		list.Add(0, 1116332); // South 
		list.Add(1, 1116333); // East
	}

	public void OnOptionSelected(Mobile from, int choice)
	{
		_south = choice == 0;

		if (!Deleted)
			base.OnDoubleClick(from);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.CloseGump(typeof(AddonOptionGump));
			from.SendGump(new AddonOptionGump(this, 1154194)); // Choose a Facing:
		}
		else
			from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.       	
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version

		writer.Write((int)Type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		Type = (TrellisType)reader.ReadInt();
	}
}
