namespace Server.Items;

public class Amber : BaseItem
{
	[Constructable]
	public Amber() : this(1)
	{
		Weight = 0.1;
	}

	[Constructable]
	public Amber(int amount) : base(0xF25)
	{
		Stackable = true;
		Amount = amount;
	}

	public Amber(Serial serial) : base(serial)
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
