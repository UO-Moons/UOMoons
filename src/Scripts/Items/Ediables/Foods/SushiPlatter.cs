namespace Server.Items;

public class SushiPlatter : Food
{
	[Constructable]
	public SushiPlatter() : base(0x2840)
	{
		Stackable = Core.ML;
		Weight = 3.0;
		FillFactor = 2;
	}

	public SushiPlatter(Serial serial) : base(serial)
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
