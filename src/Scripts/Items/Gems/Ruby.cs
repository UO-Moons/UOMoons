namespace Server.Items;

public class Ruby : BaseItem
{
	[Constructable]
	public Ruby() : this(1)
	{
	}

	[Constructable]
	public Ruby(int amount) : base(0xF13)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public Ruby(Serial serial) : base(serial)
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
