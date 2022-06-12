namespace Server.Items;

public class BrambleCoat : WoodlandChest
{
	public override int LabelNumber => 1072925;  // Bramble Coat
	public override int BasePhysicalResistance => 10;
	public override int BaseFireResistance => 8;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 8;
	public override int BaseEnergyResistance => 7;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public BrambleCoat()
	{
		Hue = 0x1;
		ArmorAttributes.SelfRepair = 3;
		Attributes.BonusHits = 4;
		Attributes.Luck = 150;
		Attributes.ReflectPhysical = 25;
		Attributes.DefendChance = 15;
	}

	public BrambleCoat(Serial serial) : base(serial)
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
