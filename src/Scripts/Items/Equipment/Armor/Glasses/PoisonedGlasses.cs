namespace Server.Items;

public class PoisonedGlasses : BaseGlasses
{
	public override int LabelNumber => 1073376;  //Poisoned Reading Glasses
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 30;
	public override int BaseEnergyResistance => 10;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public PoisonedGlasses()
	{
		Hue = 0x113;
		Attributes.BonusStam = 3;
		Attributes.RegenStam = 4;
	}
	public PoisonedGlasses(Serial serial) : base(serial)
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
