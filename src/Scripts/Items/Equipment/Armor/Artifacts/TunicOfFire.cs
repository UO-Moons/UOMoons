namespace Server.Items;

public class TunicOfFire : ChainChest
{
	public override int LabelNumber => 1061099;  // Tunic of Fire
	public override int ArtifactRarity => 11;
	public override int BasePhysicalResistance => 24;
	public override int BaseFireResistance => 34;
	public override int InitHits => Utility.RandomMinMax(255, 255);

	[Constructable]
	public TunicOfFire()
	{
		Hue = 0x54F;
		ArmorAttributes.SelfRepair = 5;
		Attributes.NightSight = 1;
		Attributes.ReflectPhysical = 15;
	}

	public TunicOfFire(Serial serial) : base(serial)
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
