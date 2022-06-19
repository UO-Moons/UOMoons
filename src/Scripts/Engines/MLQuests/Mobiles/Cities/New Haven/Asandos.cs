using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Asandos : MondainQuester
{
	[Constructable]
	public Asandos()
		: base("Asandos", "the Chef")
	{
	}

	public Asandos(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(BakersDozenQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x83FF;
		HairItemID = 0x2044;
		HairHue = 0x1;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Boots(0x901));
		AddItem(new ShortPants());
		AddItem(new Shirt());
		AddItem(new Cap());
		AddItem(new HalfApron(0x28));
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();
	}
}