namespace Server.Items;

public class KnightsBascinet : Bascinet
{
	public override int LabelNumber => 1080159;  // Knight's Bascinet
	public override SetItem SetId => SetItem.Knights;
	public override int Pieces => 6;
	public override int BasePhysicalResistance => 7;
	public override int BaseFireResistance => 7;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 7;
	public override int BaseEnergyResistance => 7;
	public override int InitMinHits => 255;
	public override int InitMaxHits => 255;
	public override bool IsArtifact => true;

	[Constructable]
	public KnightsBascinet() : base()
	{
		Hue = 1150;
		Attributes.BonusHits = 1;
		SetAttributes.BonusHits = 6;
		SetAttributes.RegenHits = 2;
		SetAttributes.RegenMana = 2;
		SetAttributes.AttackChance = 10;
		SetAttributes.DefendChance = 10;
		SetHue = 1150;
		SetPhysicalBonus = 28;
		SetFireBonus = 28;
		SetColdBonus = 28;
		SetPoisonBonus = 28;
		SetEnergyBonus = 28;
	}

	public KnightsBascinet(Serial serial) : base(serial)
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
