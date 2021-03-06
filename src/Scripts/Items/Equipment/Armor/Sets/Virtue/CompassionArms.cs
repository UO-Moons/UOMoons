namespace Server.Items;

[Flipable(0x2B0A, 0x2B0B)]
public class CompassionArms : BaseArmor
{
	public override int LabelNumber => 1075191;// Arms of Compassion (Virtue Armor Set)
	public override SetItem SetId => SetItem.Virtue;
	public override int Pieces => 8;
	public override int BasePhysicalResistance => 8;
	public override int BaseFireResistance => 11;
	public override int BaseColdResistance => 6;
	public override int BasePoisonResistance => 8;
	public override int BaseEnergyResistance => 7;
	public override int InitHits => Utility.RandomMinMax(255, 255);
	public override int StrReq => 60;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;
	public override bool IsArtifact => true;

	[Constructable]
	public CompassionArms()
		: base(0x2B0A)
	{
		LootType = LootType.Blessed;
		Weight = 3.0;
		SetHue = 0;
		Hue = 0x226;
		SetSelfRepair = 5;
		SetPhysicalBonus = 5;
		SetFireBonus = 5;
		SetColdBonus = 5;
		SetPoisonBonus = 5;
		SetEnergyBonus = 5;
	}

	public CompassionArms(Serial serial)
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
