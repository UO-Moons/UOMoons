namespace Server.Items;

public class Sake : BaseItem
{
	[Constructable]
	public Sake()
		: base(9442)
	{
	}

	public Sake(Serial serial)
		: base(serial)
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
