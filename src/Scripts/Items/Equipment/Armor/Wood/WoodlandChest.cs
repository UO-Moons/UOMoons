namespace Server.Items;

[Flipable(0x2B67, 0x315E)]
public class WoodlandChest : BaseArmor
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 95 : 95;
	public override int ArmorBase => 40;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Wood;
	public override CraftResource DefaultResource => CraftResource.RegularWood;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.None;
	public override Race RequiredRace => Race.Elf;

	[Constructable]
	public WoodlandChest() : base(0x2B67)
	{
		Weight = 8.0;
	}

	public WoodlandChest(Serial serial) : base(serial)
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
