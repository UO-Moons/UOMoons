namespace Server.Items;

public abstract class BaseSign : BaseItem
{
	public BaseSign(int dispId) : base(dispId)
	{
		Movable = false;
	}

	public BaseSign(Serial serial) : base(serial)
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
