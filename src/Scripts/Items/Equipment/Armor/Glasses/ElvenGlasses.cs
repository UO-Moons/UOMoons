namespace Server.Items;

public class ElvenGlasses : BaseGlasses
{
	public override int LabelNumber => 1032216;  // elven glasses

	[Constructable]
	public ElvenGlasses()
		: base()
	{
	}

	public ElvenGlasses(Serial serial)
		: base(serial)
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
