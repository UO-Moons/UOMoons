namespace Server.Items;

public class GoldBeadNecklace : BaseNecklace
{
	[Constructable]
	public GoldBeadNecklace() : base(0x1089)
	{
		Weight = 0.1;
	}

	public GoldBeadNecklace(Serial serial) : base(serial)
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
