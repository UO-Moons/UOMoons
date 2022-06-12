namespace Server.Items;

public class Tourmaline : BaseItem
{
	[Constructable]
	public Tourmaline() : this(1)
	{
	}

	[Constructable]
	public Tourmaline(int amount) : base(0xF2D)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public Tourmaline(Serial serial) : base(serial)
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
