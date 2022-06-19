namespace Server.Items;

public class SwampTile : BaseItem
{
	[Constructable]
	public SwampTile() : base(0x320D)
	{
	}

	public SwampTile(Serial serial) : base(serial)
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
