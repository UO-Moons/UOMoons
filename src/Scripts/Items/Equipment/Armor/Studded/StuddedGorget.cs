namespace Server.Items;

public class StuddedGorget : BaseArmor
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 3;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 4;
	public override int InitHits => Utility.RandomMinMax(35, 45);
	public override int StrReq => Core.AOS ? 25 : 25;
	public override int ArmorBase => 16;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Studded;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.Half;

	[Constructable]
	public StuddedGorget() : base(0x13D6)
	{
		Weight = 1.0;
	}

	public StuddedGorget(Serial serial) : base(serial)
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
