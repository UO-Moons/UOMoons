namespace Server.Items;

public class GrobusFur : BaseItem
{
	public override int LabelNumber => 1074676;  // Grobu's Fur

	[Constructable]
	public GrobusFur() : base(0x11F4)
	{
		LootType = LootType.Blessed;
		Hue = 0x455;
	}

	public GrobusFur(Serial serial) : base(serial)
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
