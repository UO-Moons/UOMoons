using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Mugg : MondainQuester
{
	[Constructable]
	public Mugg()
		: base("Mugg", "the Miner")
	{
	}

	public Mugg(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(MoreOrePleaseQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x840A;
		HairItemId = 0x2047;
		HairHue = 0x0;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Pickaxe());
		AddItem(new Boots(0x901));
		AddItem(new ShortPants(0x3B2));
		AddItem(new Shirt(0x22B));
		AddItem(new SkullCap(0x177));
		AddItem(new HalfApron(0x5F1));

		Item item;

		item = new PlateGloves
		{
			Hue = 0x21E
		};
		AddItem(item);
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