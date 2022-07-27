namespace Server.Items;

public class AlchemyStone : BaseItem
{
	public override string DefaultName => "an Alchemist Supply Stone";

	[Constructable]
	public AlchemyStone() : base(0xED4)
	{
		Movable = false;
		Hue = 0x250;
	}

	public override void OnDoubleClick(Mobile from)
	{
		AlchemyBag alcBag = new();

		if (!from.AddToBackpack(alcBag))
			alcBag.Delete();
	}

	public AlchemyStone(Serial serial) : base(serial)
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

		reader.ReadInt();
	}
}
