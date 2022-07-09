using Server.Items;

namespace Server.Engines.ConPVP;

public enum DuelTeleporterType
{
	Squares = 6095,
	Buds = 6104,
	Flowers = 6113,
	Spikes = 6122,
	Arrows = 6140,
	Links = 6149
}

public class DuelTeleporterAddon : BaseAddon
{
	[CommandProperty(AccessLevel.GameMaster)]
	public DuelTeleporterType Type
	{
		get
		{
			if (Components.Count > 0)
				return (DuelTeleporterType)(Components[0].ItemId);

			return DuelTeleporterType.Squares;
		}
		set
		{
			for (int i = 0; i < Components.Count && i < 9; ++i)
				Components[i].ItemId = i + (int)value;
		}
	}

	[Constructable]
	public DuelTeleporterAddon() : this(DuelTeleporterType.Squares)
	{
	}

	[Constructable]
	public DuelTeleporterAddon(DuelTeleporterType type)
	{
		int itemId = (int)type;

		AddComponent(new AddonComponent(itemId + 0), -1, -1, 5);
		AddComponent(new AddonComponent(itemId + 1), -1, 0, 5);
		AddComponent(new AddonComponent(itemId + 2), 0, -1, 5);
		AddComponent(new AddonComponent(itemId + 3), -1, +1, 5);
		AddComponent(new AddonComponent(itemId + 4), 0, 0, 5);
		AddComponent(new AddonComponent(itemId + 5), +1, -1, 5);
		AddComponent(new AddonComponent(itemId + 6), 0, +1, 5);
		AddComponent(new AddonComponent(itemId + 7), +1, 0, 5);
		AddComponent(new AddonComponent(itemId + 8), +1, +1, 5);

		AddComponent(new AddonComponent(0x759), -2, -2, 0);
		AddComponent(new AddonComponent(0x75A), +2, +2, 0);
		AddComponent(new AddonComponent(0x75B), -2, +2, 0);
		AddComponent(new AddonComponent(0x75C), +2, -2, 0);

		AddComponent(new AddonComponent(0x751), -1, +2, 0);
		AddComponent(new AddonComponent(0x751), 0, +2, 0);
		AddComponent(new AddonComponent(0x751), +1, +2, 0);

		AddComponent(new AddonComponent(0x752), +2, -1, 0);
		AddComponent(new AddonComponent(0x752), +2, 0, 0);
		AddComponent(new AddonComponent(0x752), +2, +1, 0);

		AddComponent(new AddonComponent(0x753), -1, -2, 0);
		AddComponent(new AddonComponent(0x753), 0, -2, 0);
		AddComponent(new AddonComponent(0x753), +1, -2, 0);

		AddComponent(new AddonComponent(0x754), -2, -1, 0);
		AddComponent(new AddonComponent(0x754), -2, 0, 0);
		AddComponent(new AddonComponent(0x754), -2, +1, 0);
	}

	public DuelTeleporterAddon(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}
