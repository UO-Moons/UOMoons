namespace Server.Items;

public class AdminRobe : BaseSuit
{
	[Constructable]
	public AdminRobe() : base(AccessLevel.Administrator, 0x0, 0x204F) // Blank hue
	{
	}

	public AdminRobe(Serial serial) : base(serial)
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
