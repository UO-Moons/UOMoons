namespace Server.Items;

public class GreenTea : Food
{
	[Constructable]
	public GreenTea() : base(0x284C)
	{
		Stackable = false;
		Weight = 4.0;
		FillFactor = 2;
	}

	public GreenTea(Serial serial) : base(serial)
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
