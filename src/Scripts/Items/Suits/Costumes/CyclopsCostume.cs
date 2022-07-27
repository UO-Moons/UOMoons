namespace Server.Items;

public class CyclopsCostume : BaseCostume
{
	[Constructable]
	public CyclopsCostume() : base("cyclops", 0x0, 75)
	{
	}

	public override int LabelNumber => 1114234;// cyclops costume

	public CyclopsCostume(Serial serial) : base(serial)
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
