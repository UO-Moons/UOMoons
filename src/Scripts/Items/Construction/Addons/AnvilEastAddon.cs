namespace Server.Items;

public class AnvilEastAddon : BaseAddon
{
	public override BaseAddonDeed Deed => new AnvilEastDeed();

	[Constructable]
	public AnvilEastAddon()
	{
		AddComponent(new AnvilComponent(0xFAF), 0, 0, 0);
	}

	public AnvilEastAddon(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class AnvilEastDeed : BaseAddonDeed
{
	public override BaseAddon Addon => new AnvilEastAddon();
	public override int LabelNumber => 1044333;  // anvil (east)

	[Constructable]
	public AnvilEastDeed()
	{
	}

	public AnvilEastDeed(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
