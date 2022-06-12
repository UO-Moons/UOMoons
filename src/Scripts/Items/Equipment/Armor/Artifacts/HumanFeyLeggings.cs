namespace Server.Items;

public class HumanFeyLeggings : ChainLegs
{
	public override int LabelNumber => 1075041;// Fey Leggings
	public override bool IsArtifact => true;
	public override int BasePhysicalResistance => 12;
	public override int BaseFireResistance => 8;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 4;
	public override int BaseEnergyResistance => 19;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public HumanFeyLeggings()
	{
		Attributes.BonusHits = 6;
		Attributes.DefendChance = 20;
		ArmorAttributes.MageArmor = 1;
	}

	public HumanFeyLeggings(Serial serial)
		: base(serial)
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
