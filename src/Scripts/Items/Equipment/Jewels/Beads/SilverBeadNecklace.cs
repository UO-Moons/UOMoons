namespace Server.Items;

public class SilverBeadNecklace : BaseNecklace
{
	[Constructable]
	public SilverBeadNecklace() : base(0x1F05)
	{
		Weight = 0.1;
	}

	public SilverBeadNecklace(Serial serial) : base(serial)
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
