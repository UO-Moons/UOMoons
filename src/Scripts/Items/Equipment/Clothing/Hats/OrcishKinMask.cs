namespace Server.Items;

public class OrcishKinMask : BaseHat
{
	public override int BasePhysicalResistance => 1;
	public override int BaseFireResistance => 1;
	public override int BaseColdResistance => 7;
	public override int BasePoisonResistance => 7;
	public override int BaseEnergyResistance => 8;
	public override int InitHits => Utility.RandomMinMax(20, 30);

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	public override string DefaultName => "a mask of orcish kin";

	[Constructable]
	public OrcishKinMask() : this(0x8A4)
	{
	}

	[Constructable]
	public OrcishKinMask(int hue) : base(0x141B, hue)
	{
		Weight = 2.0;
	}

	public override bool CanEquip(Mobile m)
	{
		if (!base.CanEquip(m))
			return false;

		if (m.BodyMod == 183 || m.BodyMod == 184)
		{
			m.SendLocalizedMessage(1061629); // You can't do that while wearing savage kin paint.
			return false;
		}

		return true;
	}

	public override void OnAdded(IEntity parent)
	{
		base.OnAdded(parent);

		if (parent is Mobile mobile)
			Misc.Titles.AwardKarma(mobile, -20, true);
	}

	public OrcishKinMask(Serial serial) : base(serial)
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
