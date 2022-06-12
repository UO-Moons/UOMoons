namespace Server.Items;

public class FeatheredHat : BaseHat
{
	public override int BasePhysicalResistance => 0;
	public override int BaseFireResistance => 5;
	public override int BaseColdResistance => 9;
	public override int BasePoisonResistance => 5;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public FeatheredHat() : this(0)
	{
	}

	[Constructable]
	public FeatheredHat(int hue) : base(0x171A, hue)
	{
		Weight = 1.0;
	}

	public FeatheredHat(Serial serial) : base(serial)
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
