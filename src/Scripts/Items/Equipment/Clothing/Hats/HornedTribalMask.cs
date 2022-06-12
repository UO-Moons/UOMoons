namespace Server.Items;

public class HornedTribalMask : BaseHat
{
	public override int BasePhysicalResistance => 6;
	public override int BaseFireResistance => 9;
	public override int BaseColdResistance => 0;
	public override int BasePoisonResistance => 4;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public HornedTribalMask() : this(0)
	{
	}

	[Constructable]
	public HornedTribalMask(int hue) : base(0x1549, hue)
	{
		Weight = 2.0;
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	public HornedTribalMask(Serial serial) : base(serial)
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
