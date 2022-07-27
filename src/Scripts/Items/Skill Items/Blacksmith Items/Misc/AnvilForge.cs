using Server.Engines.Craft;

namespace Server.Items;

[Flipable(0xFAF, 0xFB0)]
[Anvil]
public class Anvil : BaseItem
{
	[Constructable]
	public Anvil() : base(0xFAF)
	{
		Movable = false;
	}

	public Anvil(Serial serial) : base(serial)
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

[Forge]
public class Forge : BaseItem
{
	[Constructable]
	public Forge() : base(0xFB1)
	{
		Movable = false;
	}

	public Forge(Serial serial) : base(serial)
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
