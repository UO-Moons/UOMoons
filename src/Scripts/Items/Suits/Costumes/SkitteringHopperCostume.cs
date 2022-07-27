namespace Server.Items;

public class SkitteringHopperCostume : BaseCostume
{
	[Constructable]
	public SkitteringHopperCostume() : base("skittering hopper", 0x0, 302)
	{
	}

	public override int LabelNumber => 1114240;// skittering hopper costume

	public SkitteringHopperCostume(Serial serial) : base(serial)
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
