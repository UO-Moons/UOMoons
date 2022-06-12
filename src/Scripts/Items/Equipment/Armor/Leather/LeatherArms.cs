namespace Server.Items;

[Flipable(0x13cd, 0x13c5)]
public class LeatherArms : BaseArmor
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 3;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 3;
	public override int InitHits => Utility.RandomMinMax(30, 40);
	public override int StrReq => Core.AOS ? 20 : 15;
	public override int ArmorBase => 13;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

	[Constructable]
	public LeatherArms() : base(0x13CD)
	{
		Weight = 2.0;
	}

	public LeatherArms(Serial serial) : base(serial)
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
