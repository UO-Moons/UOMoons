namespace Server.Items;

public class MisoSoup : Food
{
	[Constructable]
	public MisoSoup() : base(0x284D)
	{
		Stackable = false;
		Weight = 4.0;
		FillFactor = 2;
	}

	public MisoSoup(Serial serial) : base(serial)
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
