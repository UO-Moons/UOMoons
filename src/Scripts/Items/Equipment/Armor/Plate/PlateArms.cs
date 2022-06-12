namespace Server.Items;

[Flipable(0x1410, 0x1417)]
public class PlateArms : BaseArmor
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 80 : 40;
	public override int DexBonusValue => Core.AOS ? 0 : -2;
	public override int ArmorBase => 40;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public PlateArms() : base(0x1410)
	{
		Weight = 5.0;
	}

	public PlateArms(Serial serial) : base(serial)
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
