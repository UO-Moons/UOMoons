namespace Server.Items;

public class OphidianMatriarchCostume : BaseCostume
{
	[Constructable]
	public OphidianMatriarchCostume() : base("ophidian matriarch", 0x0, 87)
	{
	}

	public override int LabelNumber => 1114230;// ophidian matriarch costume

	public OphidianMatriarchCostume(Serial serial) : base(serial)
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
