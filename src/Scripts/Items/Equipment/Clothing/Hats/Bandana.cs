namespace Server.Items;

public class Bandana : BaseHat
{
	public override int BasePhysicalResistance => 0;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 5;
	public override int BasePoisonResistance => 8;
	public override int BaseEnergyResistance => 8;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public Bandana() : this(0)
	{
	}

	[Constructable]
	public Bandana(int hue) : base(0x1540, hue)
	{
		Weight = 1.0;
	}

	public Bandana(Serial serial) : base(serial)
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
