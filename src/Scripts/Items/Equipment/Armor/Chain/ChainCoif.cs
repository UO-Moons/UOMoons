namespace Server.Items;

[Flipable(0x13BB, 0x13C0)]
public class ChainCoif : BaseArmor
{
	public override int BasePhysicalResistance => 4;
	public override int BaseFireResistance => 4;
	public override int BaseColdResistance => 4;
	public override int BasePoisonResistance => 1;
	public override int BaseEnergyResistance => 2;
	public override int InitHits => Utility.RandomMinMax(35, 60);
	public override int StrReq => Core.AOS ? 60 : 20;
	public override int ArmorBase => 28;

	public override ArmorMaterialType MaterialType => ArmorMaterialType.Chainmail;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public ChainCoif() : base(0x13BB)
	{
		Weight = 1.0;
	}

	public ChainCoif(Serial serial) : base(serial)
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
