namespace Server.Items;

public class SkullPole : BaseItem
{
	[Constructable]
	public SkullPole() : base(0x2204)
	{
		Weight = 5;
	}

	public SkullPole(Serial serial) : base(serial)
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
