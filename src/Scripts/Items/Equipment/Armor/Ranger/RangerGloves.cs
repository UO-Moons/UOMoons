namespace Server.Items;

public class RangerGloves : StuddedGloves
{
	public override int LabelNumber => 1041494;  // studded gloves, ranger armor

	[Constructable]
	public RangerGloves() : base()
	{
		Hue = 0x59C;
	}

	public RangerGloves(Serial serial) : base(serial)
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
