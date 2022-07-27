namespace Server.Items;

public class CityMap : MapItem
{
	public override int LabelNumber => 1015231;  // city map

	[Constructable]
	public CityMap()
	{
		SetDisplay(0, 0, 5119, 4095, 400, 400);
	}

	public override void CraftInit(Mobile from)
	{
		double skillValue = from.Skills[SkillName.Cartography].Value;
		int dist = 64 + (int)(skillValue * 4);

		if (dist < 200)
			dist = 200;

		int size = 32 + (int)(skillValue * 2);

		size = size switch
		{
			< 200 => 200,
			> 400 => 400,
			_ => size
		};

		SetDisplay(from.X - dist, from.Y - dist, from.X + dist, from.Y + dist, size, size);
	}

	public CityMap(Serial serial) : base(serial)
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
		reader.ReadInt();
	}
}
