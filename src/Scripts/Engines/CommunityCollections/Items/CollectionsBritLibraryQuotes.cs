using Server.Network;

namespace Server.Items;

[Flipable(0xFBD, 0xFBE)]
public class SherryTheMouseQuotes : BaseStatuette
{
	public override bool IsArtifact => true;
	[Constructable]
	public SherryTheMouseQuotes()
		: base(0xFBD)
	{
		LootType = LootType.Blessed;
		Weight = 5.0;
	}

	public SherryTheMouseQuotes(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073300;// Library Friends - Quotes from the pen of Sherry the Mouse
	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.RandomMinMax(1073301, 1073309));

			Effects.PlaySound(Location, Map,
				Utility.RandomBool() ? Utility.RandomMinMax(0x30A, 0x313) : Utility.RandomMinMax(0x419, 0x422));
		}

		base.OnMovement(m, oldLocation);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

[Flipable(0xFBD, 0xFBE)]
public class WyrdBeastmasterQuotes : BaseStatuette
{
	public override bool IsArtifact => true;
	[Constructable]
	public WyrdBeastmasterQuotes()
		: base(0xFBD)
	{
		LootType = LootType.Blessed;
		Weight = 5.0;
	}

	public WyrdBeastmasterQuotes(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073310;// Library Friends - Quotes from the pen of Wyrd Beastmaster
	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.RandomMinMax(1073311, 1073316));

			Effects.PlaySound(Location, Map,
				Utility.RandomBool() ? Utility.RandomMinMax(0x30A, 0x313) : Utility.RandomMinMax(0x419, 0x422));
		}

		base.OnMovement(m, oldLocation);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

[Flipable(0xFBD, 0xFBE)]
public class MercenaryJustinQuotes : BaseStatuette
{
	public override bool IsArtifact => true;
	[Constructable]
	public MercenaryJustinQuotes()
		: base(0xFBD)
	{
		LootType = LootType.Blessed;
		Weight = 5.0;
	}

	public MercenaryJustinQuotes(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073317;// Library Friends - Quotes from the pen of Mercenary Justin
	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.RandomMinMax(1073318, 1073325));

			Effects.PlaySound(Location, Map,
				Utility.RandomBool() ? Utility.RandomMinMax(0x30A, 0x313) : Utility.RandomMinMax(0x419, 0x422));
		}

		base.OnMovement(m, oldLocation);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

[Flipable(0xFBD, 0xFBE)]
public class HeigelOfMoonglowQuotes : BaseStatuette
{
	public override bool IsArtifact => true;
	[Constructable]
	public HeigelOfMoonglowQuotes()
		: base(0xFBD)
	{
		LootType = LootType.Blessed;
		Weight = 5.0;
	}

	public HeigelOfMoonglowQuotes(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073327;// Library Friends - Quotes from the pen of Heigel of Moonglow
	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.RandomList(1073326, 1073328, 1073329, 1073330, 1073331));

			Effects.PlaySound(Location, Map,
				Utility.RandomBool() ? Utility.RandomMinMax(0x30A, 0x313) : Utility.RandomMinMax(0x419, 0x422));
		}

		base.OnMovement(m, oldLocation);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

[Flipable(0xFBD, 0xFBE)]
public class TraderHoraceQuotes : BaseStatuette
{
	public override bool IsArtifact => true;
	[Constructable]
	public TraderHoraceQuotes()
		: base(0xFBD)
	{
		LootType = LootType.Blessed;
		Weight = 5.0;
	}

	public TraderHoraceQuotes(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073338;// Library Friends - Quotes from the pen of Horace, Trader
	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.RandomMinMax(1073332, 1073337));

			Effects.PlaySound(Location, Map,
				Utility.RandomBool() ? Utility.RandomMinMax(0x30A, 0x313) : Utility.RandomMinMax(0x419, 0x422));
		}

		base.OnMovement(m, oldLocation);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
