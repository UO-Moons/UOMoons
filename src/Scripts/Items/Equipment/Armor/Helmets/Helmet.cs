namespace Server.Items;

public class Helmet : BaseArmor
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 4;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(45, 60);
	public override int StrReq => Core.AOS ? 45 : 40;
	public override int ArmorBase => 30;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public Helmet() : base(0x140A)
	{
		Weight = 5.0;
	}

	public Helmet(Serial serial) : base(serial)
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
