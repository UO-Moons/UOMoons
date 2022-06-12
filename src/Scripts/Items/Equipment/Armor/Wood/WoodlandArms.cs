namespace Server.Items;

[Flipable(0x2B6C, 0x3163)]
public class WoodlandArms : BaseArmor
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 80 : 80;
	public override int ArmorBase => 40;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Wood;
	public override CraftResource DefaultResource => CraftResource.RegularWood;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.None;
	public override Race RequiredRace => Race.Elf;

	[Constructable]
	public WoodlandArms() : base(0x2B6C)
	{
		Weight = 5.0;
	}

	public WoodlandArms(Serial serial) : base(serial)
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
