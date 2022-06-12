namespace Server.Items;

public class Sapphire : BaseItem
{
	[Constructable]
	public Sapphire() : this(1)
	{
	}

	[Constructable]
	public Sapphire(int amount) : base(0xF19)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public Sapphire(Serial serial) : base(serial)
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
