using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Nibbet : MondainQuester
{
	[Constructable]
	public Nibbet()
		: base("Nibbet", "the Tinker")
	{
	}

	public Nibbet(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(ClockworkPuzzleQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x840C;
		HairItemId = 0x2044;
		HairHue = 0x1;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Boots(0x591));
		AddItem(new ShortPants(0xF8));
		AddItem(new Shirt(0x2D));
		AddItem(new FullApron(0x288));

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