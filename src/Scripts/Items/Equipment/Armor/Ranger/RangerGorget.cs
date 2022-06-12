namespace Server.Items;

public class RangerGorget : StuddedGorget
{
	public override int LabelNumber => 1041495;  // studded gorget, ranger armor

	[Constructable]
	public RangerGorget() : base()
	{
		Hue = 0x59C;
	}

	public RangerGorget(Serial serial) : base(serial)
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
