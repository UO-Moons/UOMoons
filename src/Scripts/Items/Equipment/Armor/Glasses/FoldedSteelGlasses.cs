namespace Server.Items;

public class FoldedSteelGlasses : BaseGlasses
{
	public override int LabelNumber => 1073380;  //Folded Steel Reading Glasses
	public override int BasePhysicalResistance => 20;
	public override int BaseFireResistance => 10;
	public override int BaseColdResistance => 10;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 10;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public FoldedSteelGlasses()
	{
		Hue = 0x47E;
		Attributes.BonusStr = 8;
		Attributes.NightSight = 1;
		Attributes.DefendChance = 15;
	}
	public FoldedSteelGlasses(Serial serial) : base(serial)
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
