namespace Server.Items;

public class WizardsGlasses : BaseGlasses
{
	public override int LabelNumber => 1073374;  //Wizard's Crystal Reading Glasses
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 5;
	public override int BaseColdResistance => 5;
	public override int BasePoisonResistance => 5;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public WizardsGlasses()
	{
		Hue = 0x2B0;
		Attributes.BonusMana = 10;
		Attributes.RegenMana = 3;
		Attributes.SpellDamage = 15;
	}
	public WizardsGlasses(Serial serial) : base(serial)
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
