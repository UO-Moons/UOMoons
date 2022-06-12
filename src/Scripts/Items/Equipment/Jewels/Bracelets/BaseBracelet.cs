namespace Server.Items;

public abstract class BaseBracelet : BaseJewel
{
	public override int BaseGemTypeNumber => 1044221;  // star sapphire bracelet

	public BaseBracelet(int itemID) : base(itemID, Layer.Bracelet)
	{
	}

	public BaseBracelet(Serial serial) : base(serial)
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
