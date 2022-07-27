namespace Server.Items;

public class ExodusMinionCostume : BaseCostume
{
	[Constructable]
	public ExodusMinionCostume() : base("exodus minion", 0x0, 757)
	{
	}

	public override int LabelNumber => 1114239;// exodus minion costume

	public ExodusMinionCostume(Serial serial) : base(serial)
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
