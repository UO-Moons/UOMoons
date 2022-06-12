namespace Server.Items;

public class BearMask : BaseHat
{
	public override int BasePhysicalResistance => 5;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 8;
	public override int BasePoisonResistance => 4;
	public override int BaseEnergyResistance => 4;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	[Constructable]
	public BearMask() : this(0)
	{
	}

	[Constructable]
	public BearMask(int hue) : base(0x1545, hue)
	{
		Weight = 5.0;
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	public BearMask(Serial serial) : base(serial)
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
