using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Koole : MondainQuester
{
	[Constructable]
	public Koole()
		: base("Koole", "the Arcanist")
	{
	}

	public Koole(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(DishBestServedColdQuest),
		typeof(TroubleOnTheWingQuest),
		typeof(MaraudersQuest),
		typeof(DisciplineQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x83E5;
		HairItemId = 0x2FBF;
		HairHue = 0x386;
	}

	public override void InitOutfit()
	{
		AddItem(new Boots(0x901));
		AddItem(new RoyalCirclet());
		AddItem(new LeafTonlet());

		Item item;

		item = new LeafChest
		{
			Hue = 0x1BB
		};
		AddItem(item);

		item = new LeafArms
		{
			Hue = 0x1BB
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