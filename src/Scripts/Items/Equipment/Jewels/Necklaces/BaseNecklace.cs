namespace Server.Items;

public abstract class BaseNecklace : BaseJewel
{
	public override int BaseGemTypeNumber => 1044241;  // star sapphire necklace

	public BaseNecklace(int itemID) : base(itemID, Layer.Neck)
	{
	}

	public BaseNecklace(Serial serial) : base(serial)
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
