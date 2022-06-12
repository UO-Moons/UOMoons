namespace Server.Items;

public class PreAgeDaemonLegs : BoneLegs
{
	public override int LabelNumber => 1041375;  // daemon bone leggings
	public override int BasePhysicalResistance => Utility.RandomMinMax(7, 9);
	public override int BaseFireResistance => Utility.RandomMinMax(3, 5);
	public override int BaseColdResistance => Utility.RandomMinMax(4, 6);
	public override int BasePoisonResistance => Utility.RandomMinMax(2, 4);
	public override int BaseEnergyResistance => Utility.RandomMinMax(4, 6);
	public override int InitHits => Core.AOS ? Utility.RandomMinMax(255, 255) : Utility.RandomMinMax(25, 30);
	public override int StrReq => Core.AOS ? 55 : 40;
	public override int DexBonusValue => Core.AOS ? 0 : -4;
	public override int ArmorBase => 46;

	[Constructable]
	public PreAgeDaemonLegs() : base()
	{
		Hue = 0x648;
	}

	public PreAgeDaemonLegs(Serial serial) : base(serial)
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
