namespace Server.Items;

[Flipable(0x45A4, 0x45A5)]
public class GargishClothWingArmor : BaseClothing
{
	[Constructable]
	public GargishClothWingArmor()
		: this(0)
	{
	}

	[Constructable]
	public GargishClothWingArmor(int hue)
		: base(0x45A4, Layer.Cloak, hue)
	{
		Weight = 2.0;
	}

	public override int StrReq => 10;

	public GargishClothWingArmor(Serial serial)
		: base(serial)
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
		int version = reader.ReadInt();
	}
}

[Flipable(0x4002, 0x4003)]
public class GargishFancyRobe : BaseClothing
{
	[Constructable]
	public GargishFancyRobe()
		: this(0)
	{
	}

	[Constructable]
	public GargishFancyRobe(int hue)
		: base(0x4002, Layer.OuterTorso, hue)
	{
		Weight = 3.0;
	}

	public GargishFancyRobe(Serial serial)
		: base(serial)
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
		int version = reader.ReadInt();
	}
}

[Flipable(0x4000, 0x4001)]
public class GargishRobe : BaseClothing
{
	[Constructable]
	public GargishRobe()
		: this(0)
	{
	}

	[Constructable]
	public GargishRobe(int hue)
		: base(0x4000, Layer.OuterTorso, hue)
	{
		Weight = 3.0;
	}

	public GargishRobe(Serial serial)
		: base(serial)
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
		int version = reader.ReadInt();
	}
}
