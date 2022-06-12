namespace Server.Items;

[Flipable(0x2B77, 0x316E)]
public class HidePauldrons : BaseArmor
{
	public override Race RequiredRace => Race.Elf;
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 4;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(35, 45);
	public override int StrReq => Core.AOS ? 20 : 20;
	public override int ArmorBase => 15;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Hide;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.Half;

	[Constructable]
	public HidePauldrons() : base(0x2B77)
	{
		Weight = 4.0;
	}

	public HidePauldrons(Serial serial) : base(serial)
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
