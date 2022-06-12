namespace Server.Items;

public class PerfectEmeraldRing : GoldRing
{
	public override int LabelNumber => 1073459;// perfect emerald ring

	[Constructable]
	public PerfectEmeraldRing()
			: base()
	{
		Weight = 1.0;

		BaseRunicTool.ApplyAttributesTo(this, true, 0, Utility.RandomMinMax(2, 4), 0, 100);

		if (Utility.RandomBool())
			Resistances.Poison += 10;
		else
			Attributes.SpellDamage += 5;
	}

	public PerfectEmeraldRing(Serial serial)
		: base(serial)
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
