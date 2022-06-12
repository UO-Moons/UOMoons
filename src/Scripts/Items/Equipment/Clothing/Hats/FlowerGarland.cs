namespace Server.Items;

[Flipable(0x2306, 0x2305)]
public class FlowerGarland : BaseHat
{
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 6;
	public override int BasePoisonResistance => 9;
	public override int BaseEnergyResistance => 9;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public FlowerGarland() : this(0)
	{
	}

	[Constructable]
	public FlowerGarland(int hue) : base(0x2306, hue)
	{
		Weight = 1.0;
	}

	public FlowerGarland(Serial serial) : base(serial)
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
