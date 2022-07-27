namespace Server.Items;

public class BentoBox : Food
{
	[Constructable]
	public BentoBox() : base(0x2836)
	{
		Stackable = false;
		Weight = 5.0;
		FillFactor = 2;
	}

	public override bool Eat(Mobile from)
	{
		if (!base.Eat(from))
			return false;

		from.AddToBackpack(new EmptyBentoBox());
		return true;
	}

	public BentoBox(Serial serial) : base(serial)
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
