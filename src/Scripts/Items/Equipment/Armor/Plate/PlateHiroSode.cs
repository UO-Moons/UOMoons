namespace Server.Items;

public class PlateHiroSode : BaseArmor
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 75 : 75;
	public override int ArmorBase => 3;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public PlateHiroSode() : base(0x2780)
	{
		Weight = 3.0;
	}

	public PlateHiroSode(Serial serial) : base(serial)
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
