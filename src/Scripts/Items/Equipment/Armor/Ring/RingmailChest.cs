namespace Server.Items;

[Flipable(0x13ec, 0x13ed)]
public class RingmailChest : BaseArmor
{
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 1;
	public override int BasePoisonResistance => 5;
	public override int BaseEnergyResistance => 3;
	public override int InitHits => Utility.RandomMinMax(40, 50);
	public override int StrReq => Core.AOS ? 40 : 20;
	public override int DexBonusValue => Core.AOS ? 0 : -2;
	public override int ArmorBase => 22;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Ringmail;
	public override CraftResource DefaultResource => CraftResource.Iron;

	[Constructable]
	public RingmailChest() : base(0x13EC)
	{
		Weight = 15.0;
	}

	public RingmailChest(Serial serial) : base(serial)
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
