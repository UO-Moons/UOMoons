namespace Server.Items;

public class SabreToothedTigerCostume : BaseCostume
{

	[Constructable]
	public SabreToothedTigerCostume() : base("sabre-toothed tiger", 0x0, 0x588)
	{
	}

	public override string DefaultName => "a sabre-toothed tiger costume";

	public SabreToothedTigerCostume(Serial serial)
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
		reader.ReadInt();
	}
}