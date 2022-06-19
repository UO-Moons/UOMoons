using System;

namespace Server.Items;

public class LegendaryDetectiveBoots : Boots
{
	public override int LabelNumber => 1094894;// Legendary Detective of the Royal Guard [Replica]
	public override int InitMinHits => 150;
	public override int InitMaxHits => 150;
	public override bool CanFortify => false;

	[Constructable]
	public LegendaryDetectiveBoots()
	{
		Hue = 0x455;
		Attributes.BonusInt = 2;
	}

	public LegendaryDetectiveBoots(Serial serial) : base(serial)
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

public class ElderDetectiveBoots : Boots
{
	public override int LabelNumber => 1094894;// Elder Detective of the Royal Guard [Replica]
	public override int InitMinHits => 150;
	public override int InitMaxHits => 150;
	public override bool CanFortify => false;

	[Constructable]
	public ElderDetectiveBoots()
	{
		Hue = 0x455;
		Attributes.BonusInt = 3;
	}

	public ElderDetectiveBoots(Serial serial) : base(serial)
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

public class MythicalDetectiveBoots : Boots
{
	public override int LabelNumber => 1094894;// Mythical Detective of the Royal Guard [Replica]
	public override int InitMinHits => 150;
	public override int InitMaxHits => 150;
	public override bool CanFortify => false;

	[Constructable]
	public MythicalDetectiveBoots()
	{
		Hue = 0x455;
		Attributes.BonusInt = 4;
	}

	public MythicalDetectiveBoots(Serial serial) : base(serial)
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
