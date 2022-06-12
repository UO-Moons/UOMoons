namespace Server.Items;

[Flipable(0x2FCB, 0x3181)]
public class FemaleLeafChest : BaseArmor
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 4;
	public override int BaseEnergyResistance => 4;
	public override int InitHits => Utility.RandomMinMax(30, 40);
	public override int StrReq => Core.AOS ? 20 : 20;
	public override int ArmorBase => 13;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;
	public override bool AllowMaleWearer => false;

	[Constructable]
	public FemaleLeafChest() : base(0x2FCB)
	{
		Weight = 2.0;
	}

	public FemaleLeafChest(Serial serial) : base(serial)
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
