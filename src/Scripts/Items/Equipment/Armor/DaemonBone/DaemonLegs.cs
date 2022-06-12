namespace Server.Items;

public class DaemonLegs : BoneLegs
{
	public override int LabelNumber => 1041375;  // daemon bone leggings
	public override int BasePhysicalResistance => 6;
	public override int BaseFireResistance => 6;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 5;
	public override int BaseEnergyResistance => 7;
	public override int InitHits => Core.AOS ? Utility.RandomMinMax(255, 255) : Utility.RandomMinMax(25, 30);
	public override int StrReq => Core.AOS ? 55 : 40;
	public override int DexBonusValue => Core.AOS ? 0 : -4;
	public override int ArmorBase => 46;

	[Constructable]
	public DaemonLegs() : base()
	{
		Hue = 0x648;
		if (Core.AOS)
		{
			ArmorAttributes.SelfRepair = 1;
		}
	}

	public DaemonLegs(Serial serial) : base(serial)
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
