namespace Server.Items;

public class PlateHelm : BaseArmor
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 80 : 40;
	public override int DexBonusValue => Core.AOS ? 0 : -1;
	public override int ArmorBase => 40;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public PlateHelm() : base(0x1412)
	{
		Weight = 5.0;
	}

	public PlateHelm(Serial serial) : base(serial)
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
