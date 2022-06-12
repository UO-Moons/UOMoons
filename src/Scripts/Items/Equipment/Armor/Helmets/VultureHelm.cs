namespace Server.Items;

[Flipable(0x2B72, 0x3169)]
public class VultureHelm : BaseArmor
{
	public override Race RequiredRace => Race.Elf;

	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 1;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 2;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(50, 65);
	public override int StrReq => Core.AOS ? 25 : 25;
	public override int ArmorBase => 40;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public VultureHelm() : base(0x2B72)
	{
		Weight = 5.0;
	}

	public VultureHelm(Serial serial) : base(serial)
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
