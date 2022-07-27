namespace Server.Items;

public class EmptyBottle : BaseItem, ICommodity
{
	TextDefinition ICommodity.Description => LabelNumber;
	bool ICommodity.IsDeedable => Core.ML;

	[Constructable]
	public EmptyBottle() : this(1)
	{
	}

	[Constructable]
	public EmptyBottle(int amount) : base(0xF0E)
	{
		Stackable = true;
		Weight = 1.0;
		Amount = amount;
	}

	public EmptyBottle(Serial serial) : base(serial)
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