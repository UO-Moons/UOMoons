namespace Server.Items;

public class SilverEarrings : BaseEarrings
{
	[Constructable]
	public SilverEarrings() : base(0x1F07)
	{
		Weight = 0.1;
	}

	public SilverEarrings(Serial serial) : base(serial)
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
