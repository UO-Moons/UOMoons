namespace Server.Items;

[Flipable(0x2B70, 0x3167)]
public class GemmedCirclet : BaseArmor
{
	public override Race RequiredRace => Race.Elf;

	public override int BasePhysicalResistance => 1;
	public override int BaseFireResistance => 5;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 2;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(20, 35);
	public override int StrReq => Core.AOS ? 10 : 10;
	public override int ArmorBase => 30;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

	[Constructable]
	public GemmedCirclet() : base(0x2B70)
	{
		Weight = 2.0;
	}

	public GemmedCirclet(Serial serial) : base(serial)
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
