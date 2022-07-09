namespace Server.Items;

[Furniture]
public class ChinaCabinet : FurnitureContainer, IFlipable
{
	public override int LabelNumber => 1158974;  // China Cabinet

	[Constructable]
	public ChinaCabinet()
		: base(0xA29F)
	{
		Hue = 448;
	}

	public void OnFlip(Mobile from)
	{
		ItemId = ItemId switch
		{
			0xA29F => 0xA2A1,
			0xA2A1 => 0xA29F,
			0xA2A0 => 0xA2A2,
			0xA2A2 => 0xA2A0,
			_ => ItemId
		};
	}


	public override void DisplayTo(Mobile m)
	{
		if (ItemId is 0xA29F or 0xA2A1)
			ItemId++;
		else
			ItemId--;

		if (DynamicFurniture.Open(this, m))
			base.DisplayTo(m);
	}

	public ChinaCabinet(Serial serial)
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
