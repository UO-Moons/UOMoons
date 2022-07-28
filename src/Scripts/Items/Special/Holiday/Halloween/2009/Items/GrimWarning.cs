namespace Server.Items;

public class GrimWarning : BaseItem
{
	public override double DefaultWeight => 1;

	[Constructable]
	public GrimWarning()
		: base(0x42BD)
	{
	}

	public GrimWarning(Serial serial)
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
