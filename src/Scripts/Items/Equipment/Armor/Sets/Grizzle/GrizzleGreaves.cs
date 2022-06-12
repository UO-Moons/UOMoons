namespace Server.Items;

public class GrizzleGreaves : BoneLegs
{
	public override int LabelNumber => 1074468;// Greaves of the Grizzle
	public override SetItem SetID => SetItem.Grizzle;
	public override int Pieces => 5;
	public override int BasePhysicalResistance => 6;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 5;
	public override int BasePoisonResistance => 7;
	public override int BaseEnergyResistance => 10;
	public override bool IsArtifact => true;

	[Constructable]
	public GrizzleGreaves()
		: base()
	{
		SetHue = 0x278;
		ArmorAttributes.MageArmor = 1;
		Attributes.BonusHits = 5;
		Attributes.NightSight = 1;
		SetAttributes.DefendChance = 10;
		SetAttributes.BonusStr = 12;
		SetSelfRepair = 3;
		SetPhysicalBonus = 3;
		SetFireBonus = 5;
		SetColdBonus = 3;
		SetPoisonBonus = 3;
		SetEnergyBonus = 5;
	}

	public GrizzleGreaves(Serial serial)
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
