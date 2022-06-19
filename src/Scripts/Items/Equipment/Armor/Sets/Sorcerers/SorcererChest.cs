namespace Server.Items;

public class SorcererChest : LeatherChest
{
	public override int LabelNumber => 1080468;  // Sorcerer's Tunic
	public override SetItem SetId => SetItem.Sorcerer;
	public override int Pieces => 6;
	public override int BasePhysicalResistance => 7;
	public override int BaseFireResistance => 7;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 7;
	public override int BaseEnergyResistance => 7;
	public override int InitHits => Utility.RandomMinMax(255, 255);
	public override bool IsArtifact => true;

	[Constructable]
	public SorcererChest() : base()
	{
		Hue = 1165;
		Attributes.BonusInt = 1;
		Attributes.LowerRegCost = 10;
		SetAttributes.BonusInt = 6;
		SetAttributes.RegenMana = 2;
		SetAttributes.DefendChance = 10;
		SetAttributes.LowerManaCost = 5;
		SetAttributes.LowerRegCost = 40;
		SetHue = 1165;
		SetPhysicalBonus = 28;
		SetFireBonus = 28;
		SetColdBonus = 28;
		SetPoisonBonus = 28;
		SetEnergyBonus = 28;
	}

	public SorcererChest(Serial serial) : base(serial)
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
