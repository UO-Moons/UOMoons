namespace Server.Items;

public class Wasabi : BaseItem
{
	[Constructable]
	public Wasabi() : base(0x24E8)
	{
		Weight = 1.0;
	}

	public Wasabi(Serial serial) : base(serial)
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
