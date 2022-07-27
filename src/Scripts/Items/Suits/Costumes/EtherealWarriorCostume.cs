namespace Server.Items;

public class EtherealWarriorCostume : BaseCostume
{
	[Constructable]
	public EtherealWarriorCostume() : base("ethereal warrior", 0x0, 123)
	{
	}

	public override int LabelNumber => 1114243;// ethereal warrior costume

	public EtherealWarriorCostume(Serial serial) : base(serial)
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
