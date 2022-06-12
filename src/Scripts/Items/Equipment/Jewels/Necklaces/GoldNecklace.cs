namespace Server.Items;

public class GoldNecklace : BaseNecklace
{
	[Constructable]
	public GoldNecklace() : base(0x1088)
	{
		Weight = 0.1;
	}

	public GoldNecklace(Serial serial) : base(serial)
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
