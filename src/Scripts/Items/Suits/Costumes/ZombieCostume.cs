namespace Server.Items;

public class ZombieCostume : BaseCostume
{
	[Constructable]
	public ZombieCostume() : base("zombie", 0x0, 3)
	{
	}

	public override int LabelNumber => 1114222;// zombie costume

	public ZombieCostume(Serial serial) : base(serial)
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
