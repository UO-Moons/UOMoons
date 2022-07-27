namespace Server.Items;

public class MongbatCostume : BaseCostume
{
	[Constructable]
	public MongbatCostume() : base("mongbat", 0x0, 39)
	{
	}

	public override int LabelNumber => 1114223;// mongbat costume

	public MongbatCostume(Serial serial) : base(serial)
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
