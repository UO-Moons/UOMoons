namespace Server.Items;

public class RangerArms : StuddedArms
{
	public override int LabelNumber => 1041493;  // studded sleeves, ranger armor

	[Constructable]
	public RangerArms() : base()
	{
		Hue = 0x59C;
	}

	public RangerArms(Serial serial) : base(serial)
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
