namespace Server.Items;

public class WolfSpiderCostume : BaseCostume
{
	[Constructable]
	public WolfSpiderCostume() : base("wolf spider", 0x0, 736)
	{
	}

	public override int LabelNumber => 1114232;// wolf spider costume

	public WolfSpiderCostume(Serial serial) : base(serial)
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
