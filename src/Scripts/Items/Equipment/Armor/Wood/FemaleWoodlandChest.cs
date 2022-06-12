namespace Server.Items;

[Flipable(0x2B6D, 0x3164)]
public class FemaleElvenPlateChest : BaseArmor
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 3;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 95 : 95;
	public override bool AllowMaleWearer => false;
	public override int ArmorBase => 30;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Wood;
	public override CraftResource DefaultResource => CraftResource.RegularWood;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.None;

	[Constructable]
	public FemaleElvenPlateChest() : base(0x2B6D)
	{
		Weight = 8.0;
	}

	public FemaleElvenPlateChest(Serial serial) : base(serial)
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
