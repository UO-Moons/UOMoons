namespace Server.Items;

public class AwaseMisoSoup : Food
{
	[Constructable]
	public AwaseMisoSoup() : base(0x2850)
	{
		Stackable = false;
		Weight = 4.0;
		FillFactor = 2;
	}

	public AwaseMisoSoup(Serial serial) : base(serial)
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
