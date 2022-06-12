namespace Server.Items;

public class TribalMask : BaseHat
{
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 0;
	public override int BaseColdResistance => 6;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public TribalMask() : this(0)
	{
	}

	[Constructable]
	public TribalMask(int hue) : base(0x154B, hue)
	{
		Weight = 2.0;
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	public TribalMask(Serial serial) : base(serial)
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
