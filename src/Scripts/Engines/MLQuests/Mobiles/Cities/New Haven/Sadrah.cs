using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Sadrah : MondainQuester
{
	[Constructable]
	public Sadrah()
		: base("Sadrah", "the Courier")
	{
	}

	public Sadrah(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(FleeAndFatigueQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x8406;
		HairItemID = 0x203D;
		HairHue = 0x901;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Longsword());
		AddItem(new Boots(0x901));
		AddItem(new Shirt(0x127));
		AddItem(new Cloak(0x65));
		AddItem(new Skirt(0x52));
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}