namespace Server.Items;

public class SavageMask : BaseHat
{
	public override int BasePhysicalResistance => 3;
	public override int BaseFireResistance => 0;
	public override int BaseColdResistance => 6;
	public override int BasePoisonResistance => 10;
	public override int BaseEnergyResistance => 5;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	public static int GetRandomHue()
	{
		int v = Utility.RandomBirdHue();

		if (v == 2101)
			v = 0;

		return v;
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	[Constructable]
	public SavageMask() : this(GetRandomHue())
	{
	}

	[Constructable]
	public SavageMask(int hue) : base(0x154B, hue)
	{
		Weight = 2.0;
	}

	public SavageMask(Serial serial) : base(serial)
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
