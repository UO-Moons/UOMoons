namespace Server.Items;

public class SatyrCostume : BaseCostume
{
	[Constructable]
	public SatyrCostume() : base("satyr", 0x0, 271)
	{
	}

	public override int LabelNumber => 1114287;// satyr costume

	public SatyrCostume(Serial serial) : base(serial)
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
