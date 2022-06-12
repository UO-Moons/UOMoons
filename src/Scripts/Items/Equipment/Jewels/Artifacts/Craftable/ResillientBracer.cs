namespace Server.Items;

public class ResilientBracer : GoldBracelet
{
	public override int LabelNumber => 1072933;  // Resillient Bracer

	public override int PhysicalResistance => 20;

	[Constructable]
	public ResilientBracer()
	{
		Hue = 0x488;

		SkillBonuses.SetValues(0, SkillName.MagicResist, 15.0);

		Attributes.BonusHits = 5;
		Attributes.RegenHits = 2;
		Attributes.DefendChance = 10;
	}

	public ResilientBracer(Serial serial) : base(serial)
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
