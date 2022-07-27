namespace Server.Items;

public class EmptyBentoBox : BaseItem
{
	[Constructable]
	public EmptyBentoBox() : base(0x2834)
	{
		Weight = 5.0;
	}

	public EmptyBentoBox(Serial serial) : base(serial)
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
