namespace Server.Items;

public abstract class Hair : BaseItem
{
	protected Hair(int itemId, int hue = 0)
		: base(itemId)
	{
		LootType = LootType.Blessed;
		Layer = Layer.Hair;
		Hue = hue;
	}

	public Hair(Serial serial)
		: base(serial)
	{
	}

	public override bool DisplayLootType => false;

	public override bool VerifyMove(Mobile from)
	{
		return from.IsStaff();
	}

	public override DeathMoveResult OnParentDeath(Mobile parent)
	{
		parent.HairItemId = ItemId;
		parent.HairHue = Hue;

		return DeathMoveResult.MoveToCorpse;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		LootType = LootType.Blessed;

		reader.ReadInt();
	}
}

public class GenericHair : Hair
{
	private GenericHair(int itemId, int hue = 0)
		: base(itemId, hue)
	{
	}

	public GenericHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class Mohawk : Hair
{

	private Mohawk()
		: this(0)
	{
	}


	private Mohawk(int hue)
		: base(0x2044, hue)
	{
	}

	public Mohawk(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class PageboyHair : Hair
{

	private PageboyHair()
		: this(0)
	{
	}


	private PageboyHair(int hue)
		: base(0x2045, hue)
	{
	}

	public PageboyHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class BunsHair : Hair
{

	private BunsHair()
		: this(0)
	{
	}


	private BunsHair(int hue)
		: base(0x2046, hue)
	{
	}

	public BunsHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class LongHair : Hair
{

	private LongHair()
		: this(0)
	{
	}


	private LongHair(int hue)
		: base(0x203C, hue)
	{
	}

	public LongHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class ShortHair : Hair
{

	private ShortHair()
		: this(0)
	{
	}


	private ShortHair(int hue)
		: base(0x203B, hue)
	{
	}

	public ShortHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class PonyTail : Hair
{

	private PonyTail()
		: this(0)
	{
	}


	private PonyTail(int hue)
		: base(0x203D, hue)
	{
	}

	public PonyTail(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class Afro : Hair
{

	private Afro()
		: this(0)
	{
	}


	private Afro(int hue)
		: base(0x2047, hue)
	{
	}

	public Afro(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class ReceedingHair : Hair
{

	private ReceedingHair()
		: this(0)
	{
	}


	private ReceedingHair(int hue)
		: base(0x2048, hue)
	{
	}

	public ReceedingHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class TwoPigTails : Hair
{

	private TwoPigTails()
		: this(0)
	{
	}


	private TwoPigTails(int hue)
		: base(0x2049, hue)
	{
	}

	public TwoPigTails(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class KrisnaHair : Hair
{

	private KrisnaHair()
		: this(0)
	{
	}


	private KrisnaHair(int hue)
		: base(0x204A, hue)
	{
	}

	public KrisnaHair(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
