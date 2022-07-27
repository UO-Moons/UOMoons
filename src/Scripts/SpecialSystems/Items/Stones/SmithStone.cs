namespace Server.Items;

public class SmithStone : BaseItem
{
	public override string DefaultName => "a Blacksmith Supply Stone";

	[Constructable]
	public SmithStone() : base(0xED4)
	{
		Movable = false;
		Hue = 0x476;
	}

	public override void OnDoubleClick(Mobile from)
	{
		SmithBag smithBag = new(5000);

		if (!from.AddToBackpack(smithBag))
			smithBag.Delete();
	}

	public SmithStone(Serial serial) : base(serial)
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
