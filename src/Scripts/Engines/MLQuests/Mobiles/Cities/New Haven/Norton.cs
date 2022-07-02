using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Norton : MondainQuester
{
	[Constructable]
	public Norton()
		: base("Norton", "the Fisher")
	{
	}

	public Norton(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(DeliciousFishesQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x83F8;
		HairItemId = 0x203B;
		HairHue = 0x472;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new ThighBoots());
		AddItem(new Shirt(0x11D));
		AddItem(new LongPants(0x6C2));
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