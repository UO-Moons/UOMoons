namespace Server.Items;

[Flipable(0x230A, 0x2309)]
public class FurCape : BaseCloak
{
	[Constructable]
	public FurCape() : this(0)
	{
	}

	[Constructable]
	public FurCape(int hue) : base(0x230A, hue)
	{
		Weight = 4.0;
	}

	public FurCape(Serial serial) : base(serial)
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
