namespace Server.Items;

public class TitanCostume : BaseCostume
{
	[Constructable]
	public TitanCostume() : base("titan", 0x0, 76)
	{
	}

	public override int LabelNumber => 1114238;// titan costume

	public TitanCostume(Serial serial) : base(serial)
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
