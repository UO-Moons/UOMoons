namespace Server.Items;

public class DeerMask : BaseHat
{
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 6;
	public override int BaseColdResistance => 8;
	public override int BasePoisonResistance => 1;
	public override int BaseEnergyResistance => 7;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public DeerMask() : this(0)
	{
	}

	[Constructable]
	public DeerMask(int hue) : base(0x1547, hue)
	{
		Weight = 4.0;
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	public DeerMask(Serial serial) : base(serial)
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
