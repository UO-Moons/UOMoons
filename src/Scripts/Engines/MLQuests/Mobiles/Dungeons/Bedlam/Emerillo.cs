using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Emerillo : MondainQuester
{
	[Constructable]
	public Emerillo()
		: base("Emerillo", "the Cook")
	{
	}

	public Emerillo(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(CulinaryCrisisQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;

		Hue = 0x83F4;
		HairItemId = 0x203C;
		HairHue = 0x454;
		FacialHairItemId = 0x204C;
		FacialHairHue = 0x454;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Sandals(0x75D));
		AddItem(new LongPants(0x529));
		AddItem(new Shirt(0x38B));
		AddItem(new HalfApron(0x8FD));
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