namespace Server.Items;

public class SilverRing : BaseRing
{
	[Constructable]
	public SilverRing() : base(0x1F09)
	{
		Weight = 0.1;
	}

	public SilverRing(Serial serial) : base(serial)
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
