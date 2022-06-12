namespace Server.Items;

public class StarSapphire : BaseItem
{
	[Constructable]
	public StarSapphire() : this(1)
	{
	}

	[Constructable]
	public StarSapphire(int amount) : base(0xF21)
	{
		Weight = 0.1;
		Stackable = true;
		Amount = amount;
	}

	public StarSapphire(Serial serial) : base(serial)
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
