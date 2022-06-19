namespace Server.Items;

[Flipable(0x2B12, 0x2B13)]
public class SacrificeSollerets : BaseClothing
{
	public override int LabelNumber => 1075194;// Sollerets of Sacrifice (Virtue Armor Set)
	public override SetItem SetId => SetItem.Virtue;
	public override int Pieces => 8;
	public override int InitMinHits => 0;
	public override int InitMaxHits => 0;
	public override int StrReq => 10;
	public override bool IsArtifact => true;

	[Constructable]
	public SacrificeSollerets()
		: base(0x2B13, Layer.Shoes)
	{
		LootType = LootType.Blessed;
		Weight = 1.0;
		Hue = 0x226;
		SetSelfRepair = 5;
		SetPhysicalBonus = 5;
		SetFireBonus = 5;
		SetColdBonus = 5;
		SetPoisonBonus = 5;
		SetEnergyBonus = 5;
	}

	public SacrificeSollerets(Serial serial)
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
