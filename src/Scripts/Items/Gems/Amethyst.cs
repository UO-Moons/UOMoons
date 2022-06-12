namespace Server.Items;

public class Amethyst : BaseItem
{

	[Constructable]
	public Amethyst() : this(1)
	{
	}

	[Constructable]
	public Amethyst(int amount) : base(0xF16)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public Amethyst(Serial serial) : base(serial)
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
