namespace Server.Items;

public class NecromanticGlasses : BaseGlasses
{
	public override int LabelNumber => 1073377;  //Necromantic Reading Glasses
	public override int BasePhysicalResistance => 0;
	public override int BaseFireResistance => 0;
	public override int BaseColdResistance => 0;
	public override int BasePoisonResistance => 0;
	public override int BaseEnergyResistance => 0;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public NecromanticGlasses()
	{
		Hue = 0x22D;
		Attributes.LowerManaCost = 15;
		Attributes.LowerRegCost = 30;
	}
	public NecromanticGlasses(Serial serial) : base(serial)
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
