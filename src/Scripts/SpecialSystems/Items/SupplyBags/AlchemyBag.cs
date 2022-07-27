namespace Server.Items;

public class AlchemyBag : Bag
{
	public override string DefaultName => "an Alchemy Kit";

	[Constructable]
	public AlchemyBag() : this(1)
	{
		Movable = true;
		Hue = 0x250;
	}

	[Constructable]
	private AlchemyBag(int amount)
	{
		DropItem(new MortarPestle(5));
		DropItem(new BagOfReagents(5000));
		DropItem(new EmptyBottle(5000));
	}

	public AlchemyBag(Serial serial) : base(serial)
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
