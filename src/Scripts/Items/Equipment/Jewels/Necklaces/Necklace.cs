namespace Server.Items;

public class Necklace : BaseNecklace
{
	[Constructable]
	public Necklace() : base(0x1085)
	{
		Weight = 0.1;
	}

	public Necklace(Serial serial) : base(serial)
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
