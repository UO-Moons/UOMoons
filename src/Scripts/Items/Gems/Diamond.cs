namespace Server.Items;

public class Diamond : BaseItem
{
	[Constructable]
	public Diamond() : this(1)
	{
	}

	[Constructable]
	public Diamond(int amount) : base(0xF26)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public Diamond(Serial serial) : base(serial)
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
