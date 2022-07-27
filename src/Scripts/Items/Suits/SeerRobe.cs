namespace Server.Items;

public class SeerRobe : BaseSuit
{
	[Constructable]
	public SeerRobe() : base(AccessLevel.Seer, 0x1D3, 0x204F)
	{
	}

	public SeerRobe(Serial serial) : base(serial)
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
