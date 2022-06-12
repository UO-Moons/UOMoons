namespace Server.Items;

public class IndecipherableMap : MapItem
{
	public override int LabelNumber => 1070799;  // indecipherable map

	[Constructable]
	public IndecipherableMap()
	{
		if (Utility.RandomDouble() < 0.2)
			Hue = 0x965;
		else
			Hue = 0x961;
	}

	public IndecipherableMap(Serial serial) : base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		from.SendLocalizedMessage(1070801); // You cannot decipher this ruined map.
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
