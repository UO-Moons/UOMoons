namespace Server.Items;

public class SongWovenMantle : LeafArms
{
	public override int LabelNumber => 1072931;  // Song Woven Mantle
	public override int BasePhysicalResistance => 14;
	public override int BaseColdResistance => 14;
	public override int BaseEnergyResistance => 16;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public SongWovenMantle()
	{
		Hue = 0x493;
		SkillBonuses.SetValues(0, SkillName.Musicianship, 10.0);
		Attributes.Luck = 100;
		Attributes.DefendChance = 5;
	}

	public SongWovenMantle(Serial serial) : base(serial)
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
