namespace Server.Items;

public class GmRobe : BaseSuit
{
	[Constructable]
	public GmRobe() : base(AccessLevel.GameMaster, 0x26, 0x204F)
	{
	}

	public GmRobe(Serial serial) : base(serial)
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
