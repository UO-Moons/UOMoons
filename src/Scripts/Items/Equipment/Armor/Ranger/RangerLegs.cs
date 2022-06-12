namespace Server.Items;

public class RangerLegs : StuddedLegs
{
	public override int LabelNumber => 1041496;  // studded leggings, ranger armor

	[Constructable]
	public RangerLegs() : base()
	{
		Hue = 0x59C;
	}

	public RangerLegs(Serial serial) : base(serial)
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
