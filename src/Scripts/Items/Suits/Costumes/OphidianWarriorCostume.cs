namespace Server.Items;

public class OphidianWarriorCostume : BaseCostume
{
	[Constructable]
	public OphidianWarriorCostume() : base("ophidian warrior", 0x0, 86)
	{
	}

	public OphidianWarriorCostume(Serial serial) : base(serial)
	{
	}

	public override int LabelNumber => 1114229;// ophidian warrior costume

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
