namespace Server.Items;

public class ArtsGlasses : BaseGlasses
{
	public override int LabelNumber => 1073363;  //Reading Glasses of the Arts
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 8;
	public override int BaseColdResistance => 8;
	public override int BasePoisonResistance => 4;
	public override int BaseEnergyResistance => 10;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public ArtsGlasses()
	{
		Hue = 0x73;
		Attributes.BonusStr = 5;
		Attributes.BonusInt = 5;
		Attributes.BonusHits = 15;
	}

	public ArtsGlasses(Serial serial) : base(serial)
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
