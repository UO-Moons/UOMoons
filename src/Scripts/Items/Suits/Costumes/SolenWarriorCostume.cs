namespace Server.Items;

public class SolenWarriorCostume : BaseCostume
{
	[Constructable]
	public SolenWarriorCostume() : base("solen warrior", 0x0, 782)
	{
	}

	public override int LabelNumber => 1114231;// solen warrior costume

	public SolenWarriorCostume(Serial serial) : base(serial)
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
