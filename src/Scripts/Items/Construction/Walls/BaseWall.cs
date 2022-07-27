namespace Server.Items;

public abstract class BaseWall : BaseItem
{
	public BaseWall(int itemId) : base(itemId)
	{
		Movable = false;
	}

	public BaseWall(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
