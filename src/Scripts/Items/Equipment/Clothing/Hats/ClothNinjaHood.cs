namespace Server.Items;

[Flipable(0x278F, 0x27DA)]
public class ClothNinjaHood : BaseHat
{
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 6;
	public override int BasePoisonResistance => 9;
	public override int BaseEnergyResistance => 9;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public ClothNinjaHood() : this(0)
	{
	}

	[Constructable]
	public ClothNinjaHood(int hue) : base(0x278F, hue)
	{
		Weight = 2.0;
	}

	public ClothNinjaHood(Serial serial) : base(serial)
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
