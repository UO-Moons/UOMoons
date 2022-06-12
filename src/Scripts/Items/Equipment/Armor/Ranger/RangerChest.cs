namespace Server.Items;

public class RangerChest : StuddedChest
{
	public override int LabelNumber => 1041497;  // studded tunic, ranger armor

	[Constructable]
	public RangerChest() : base()
	{
		Hue = 0x59C;
	}

	public RangerChest(Serial serial) : base(serial)
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
