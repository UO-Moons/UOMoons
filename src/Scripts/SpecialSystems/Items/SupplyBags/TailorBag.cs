namespace Server.Items;

public class TailorBag : Bag
{
	public override string DefaultName => "a Tailoring Kit";

	[Constructable]
	public TailorBag() : this(1)
	{
		Movable = true;
		Hue = 0x315;
	}

	[Constructable]
	private TailorBag(int amount)
	{
		DropItem(new SewingKit(5));
		DropItem(new Scissors());
		DropItem(new Hides(500));
		DropItem(new BoltOfCloth(20));
		DropItem(new DyeTub());
		DropItem(new DyeTub());
		DropItem(new BlackDyeTub());
		DropItem(new Dyes());
	}

	public TailorBag(Serial serial) : base(serial)
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
