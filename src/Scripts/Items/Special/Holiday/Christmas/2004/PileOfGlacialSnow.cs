namespace Server.Items;

public class PileOfGlacialSnow : SnowPile
{
	[Constructable]
	public PileOfGlacialSnow()
	{
		Hue = 0x480;
	}

	public override int LabelNumber => 1070874;  // a Pile of Glacial Snow

	public PileOfGlacialSnow(Serial serial) : base(serial)
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

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);
		LabelTo(from, 1070880); // Winter 2004
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);
		list.Add(1070880); // Winter 2004
	}
}
