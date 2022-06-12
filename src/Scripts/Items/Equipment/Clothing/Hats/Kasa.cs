namespace Server.Items;

[Flipable(0x2798, 0x27E3)]
public class Kasa : BaseHat
{
	public override int BasePhysicalResistance => 0;
	public override int BaseFireResistance => 5;
	public override int BaseColdResistance => 9;
	public override int BasePoisonResistance => 5;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public Kasa() : this(0)
	{
	}

	[Constructable]
	public Kasa(int hue) : base(0x2798, hue)
	{
		Weight = 3.0;
	}

	public Kasa(Serial serial) : base(serial)
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
