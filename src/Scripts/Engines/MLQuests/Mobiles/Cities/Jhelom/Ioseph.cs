using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Ioseph : MondainQuester
{
	[Constructable]
	public Ioseph()
		: base("Ioseph", "the Exporter")
	{
	}

	public Ioseph(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(EmbracingHumanityQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;

		Hue = 0x8404;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x737));
		AddItem(new LongPants(0x1BB));
		AddItem(new FancyShirt(0x535));
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