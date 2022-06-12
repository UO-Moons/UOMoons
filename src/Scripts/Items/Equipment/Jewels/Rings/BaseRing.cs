namespace Server.Items;

public abstract class BaseRing : BaseJewel
{
	public override int BaseGemTypeNumber => 1044176;  // star sapphire ring

	public BaseRing(int itemID) : base(itemID, Layer.Ring)
	{
	}

	public BaseRing(Serial serial) : base(serial)
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
