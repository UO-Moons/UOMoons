namespace Server.Items;

[Furniture]
public class PieSafe : FurnitureContainer, IFlipable
{
	public override int LabelNumber => 1158973;  // Pie Safe
	public override int DefaultGumpID => 0x4F;

	[Constructable]
	public PieSafe()
		: base(0xA29B)
	{
		Hue = 448;
	}

	public void OnFlip(Mobile from)
	{
		ItemId = ItemId switch
		{
			0xA29B => 0xA29D,
			0xA29D => 0xA29B,
			0xA29C => 0xA29E,
			0xA29E => 0xA29C,
			_ => ItemId
		};
	}

	public override void DisplayTo(Mobile m)
	{
		if (ItemId is 0xA29B or 0xA29D)
			ItemId++;
		else
			ItemId--;

		if (DynamicFurniture.Open(this, m))
			base.DisplayTo(m);
	}

	public PieSafe(Serial serial)
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
