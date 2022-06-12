namespace Server.Items;

[Flipable(0x2645, 0x2646)]
public class DragonHelm : BaseArmor
{
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 3;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 3;
	public override int InitHits => Core.AOS ? Utility.RandomMinMax(55, 75) : Utility.RandomMinMax(36, 48);
	public override int StrReq => Core.AOS ? 75 : 40;
	public override int DexBonusValue => Core.AOS ? 0 : -1;
	public override int ArmorBase => 40;

	public override ArmorMaterialType MaterialType => ArmorMaterialType.Dragon;
	public override CraftResource DefaultResource => CraftResource.RedScales;

	[Constructable]
	public DragonHelm() : base(0x2645)
	{
		Weight = 5.0;
	}

	public DragonHelm(Serial serial) : base(serial)
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
