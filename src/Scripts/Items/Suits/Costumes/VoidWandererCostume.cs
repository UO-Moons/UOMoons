namespace Server.Items;

public class VoidWandererCostume : BaseCostume
{
	[Constructable]
	public VoidWandererCostume() : base("wanderer of the void", 0x0, 316)
	{
	}

	public override int LabelNumber => 1114286;// void wanderer costume

	public VoidWandererCostume(Serial serial) : base(serial)
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
