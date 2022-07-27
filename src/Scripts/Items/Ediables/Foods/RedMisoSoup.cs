namespace Server.Items;

public class RedMisoSoup : Food
{
	[Constructable]
	public RedMisoSoup() : base(0x284F)
	{
		Stackable = false;
		Weight = 4.0;
		FillFactor = 2;
	}

	public RedMisoSoup(Serial serial) : base(serial)
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
