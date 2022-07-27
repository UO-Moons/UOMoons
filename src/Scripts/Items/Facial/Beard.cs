namespace Server.Items;

public abstract class Beard : BaseItem
{
	protected Beard(int itemId, int hue = 0) : base(itemId)
	{
		LootType = LootType.Blessed;
		Layer = Layer.FacialHair;
		Hue = hue;
	}

	public Beard(Serial serial) : base(serial)
	{
	}

	public override bool DisplayLootType => false;

	public override bool VerifyMove(Mobile from)
	{
		return (from.AccessLevel >= AccessLevel.GameMaster);
	}

	public override DeathMoveResult OnParentDeath(Mobile parent)
	{
		parent.FacialHairItemId = ItemId;
		parent.FacialHairHue = Hue;

		return DeathMoveResult.MoveToCorpse;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		LootType = LootType.Blessed;

		reader.ReadInt();
	}
}

public class GenericBeard : Beard
{
	private GenericBeard(int itemId, int hue = 0) : base(itemId, hue)
	{
	}

	public GenericBeard(Serial serial) : base(serial)
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

public class LongBeard : Beard
{

	private LongBeard()
		: this(0)
	{
	}

	private LongBeard(int hue)
		: base(0x203E, hue)
	{
	}

	public LongBeard(Serial serial) : base(serial)
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

public class ShortBeard : Beard
{

	private ShortBeard()
		: this(0)
	{
	}


	private ShortBeard(int hue)
		: base(0x203f, hue)
	{
	}

	public ShortBeard(Serial serial) : base(serial)
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

public class Goatee : Beard
{

	private Goatee()
		: this(0)
	{
	}


	private Goatee(int hue)
		: base(0x2040, hue)
	{
	}

	public Goatee(Serial serial) : base(serial)
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

public class Mustache : Beard
{

	private Mustache()
		: this(0)
	{
	}


	private Mustache(int hue)
		: base(0x2041, hue)
	{
	}

	public Mustache(Serial serial) : base(serial)
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

public class MediumShortBeard : Beard
{

	private MediumShortBeard()
		: this(0)
	{
	}


	private MediumShortBeard(int hue)
		: base(0x204B, hue)
	{
	}

	public MediumShortBeard(Serial serial) : base(serial)
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

public class MediumLongBeard : Beard
{

	private MediumLongBeard()
		: this(0)
	{
	}


	private MediumLongBeard(int hue)
		: base(0x204C, hue)
	{
	}

	public MediumLongBeard(Serial serial) : base(serial)
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

public class Vandyke : Beard
{

	private Vandyke()
		: this(0)
	{
	}


	private Vandyke(int hue)
		: base(0x204D, hue)
	{
	}

	public Vandyke(Serial serial) : base(serial)
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
