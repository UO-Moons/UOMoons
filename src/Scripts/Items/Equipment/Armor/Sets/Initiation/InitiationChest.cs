namespace Server.Items;

public class InitiationChest : LeatherChest
{
	public override int LabelNumber => 1116255;  // Armor of Initiation
	public override SetItem SetId => SetItem.Initiation;
	public override int Pieces => 6;
	public override int BasePhysicalResistance => 7;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 4;
	public override int BasePoisonResistance => 6;
	public override int BaseEnergyResistance => 4;
	public override int InitMinHits => 150;
	public override int InitMaxHits => 150;
	public override bool IsArtifact => true;

	[Constructable]
	public InitiationChest() : base()
	{
		Hue = 0x9C4;
		//Attributes.Brittle = 1;
		LootType = LootType.Blessed;
		SetHue = 0x30;
		SetPhysicalBonus = 2;
		SetFireBonus = 5;
		SetColdBonus = 5;
		SetPoisonBonus = 3;
		SetEnergyBonus = 5;
	}

	public InitiationChest(Serial serial) : base(serial)
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
