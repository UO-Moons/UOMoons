using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Lowel : MondainQuester
{
	[Constructable]
	public Lowel()
		: base("Lowel", "the Carpenter")
	{
	}

	public Lowel(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(ComfortableSeatingQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x83F6;
		HairItemID = 0x203C;
		HairHue = 0x6B1;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Boots(0x543));
		AddItem(new ShortPants(0x758));
		AddItem(new FancyShirt(0x53A));
		AddItem(new HalfApron(0x6D2));
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