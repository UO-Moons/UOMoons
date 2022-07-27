namespace Server.Items;

public class SushiRolls : Food
{
	[Constructable]
	public SushiRolls() : base(0x283E)
	{
		Stackable = false;
		Weight = 3.0;
		FillFactor = 2;
	}

	public SushiRolls(Serial serial) : base(serial)
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
