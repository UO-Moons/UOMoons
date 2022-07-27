namespace Server.Items;

public class ShadowWyrmCostume : BaseCostume
{
	[Constructable]
	public ShadowWyrmCostume() : base("shadow wyrm", 0x0, 106)
	{
	}

	public override int LabelNumber => 1114009;// shadow wyrm halloween costume

	public ShadowWyrmCostume(Serial serial) : base(serial)
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
