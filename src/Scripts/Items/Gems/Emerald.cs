namespace Server.Items;

public class Emerald : BaseItem
{
	[Constructable]
	public Emerald() : this(1)
	{
	}

	[Constructable]
	public Emerald(int amount) : base(0xF10)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public Emerald(Serial serial) : base(serial)
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
