namespace Server.Items;

public class ScoutGloves : StuddedGloves
{
	public override int LabelNumber => 1080477;  // Scout's Studded Gloves
	public override SetItem SetID => SetItem.Scout;
	public override int Pieces => 6;
	public override int BasePhysicalResistance => 7;
	public override int BaseFireResistance => 7;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 7;
	public override int BaseEnergyResistance => 7;
	public override int InitHits => Utility.RandomMinMax(255, 255);
	public override bool IsArtifact => true;

	[Constructable]
	public ScoutGloves() : base()
	{
		Hue = 1148;
		Attributes.BonusDex = 1;
		ArmorAttributes.MageArmor = 1;
		SetAttributes.BonusDex = 6;
		SetAttributes.RegenHits = 2;
		SetAttributes.RegenMana = 2;
		SetAttributes.AttackChance = 10;
		SetAttributes.DefendChance = 10;
		SetHue = 1148;
		SetPhysicalBonus = 28;
		SetFireBonus = 28;
		SetColdBonus = 28;
		SetPoisonBonus = 28;
		SetEnergyBonus = 28;
	}

	public ScoutGloves(Serial serial) : base(serial)
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
