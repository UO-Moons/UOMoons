namespace Server.Items;

public class TerathanWarriorCostume : BaseCostume
{
	[Constructable]
	public TerathanWarriorCostume() : base("terathan warrior", 0x0, 70)
	{
	}

	public override int LabelNumber => 1114228;// terathan warrior costume

	public TerathanWarriorCostume(Serial serial) : base(serial)
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
