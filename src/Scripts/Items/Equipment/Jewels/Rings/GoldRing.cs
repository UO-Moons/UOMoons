namespace Server.Items;

public class GoldRing : BaseRing
{
	[Constructable]
	public GoldRing() : base(0x108a)
	{
		Weight = 0.1;
	}

	public GoldRing(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
	}
}
