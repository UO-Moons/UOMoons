namespace Server.Items;

public class ButcherBlock : BaseItem
{
	public override int LabelNumber => 1125659;  // butcher block

	[Constructable]
	public ButcherBlock()
		: base(0xA2A3)
	{
		LootType = LootType.Blessed;
	}

	public ButcherBlock(Serial serial)
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
