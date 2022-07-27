namespace Server.Items;

public class WhiteMisoSoup : Food
{
	[Constructable]
	public WhiteMisoSoup() : base(0x284E)
	{
		Stackable = false;
		Weight = 4.0;
		FillFactor = 2;
	}

	public WhiteMisoSoup(Serial serial) : base(serial)
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
