namespace Server.Items;

public class LeatherMempo : BaseArmor
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 3;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 3;
	public override int InitHits => Utility.RandomMinMax(35, 45);
	public override int StrReq => Core.AOS ? 30 : 30;
	public override int ArmorBase => 3;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

	[Constructable]
	public LeatherMempo() : base(0x277A)
	{
		Weight = 2.0;
	}

	public LeatherMempo(Serial serial) : base(serial)
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
