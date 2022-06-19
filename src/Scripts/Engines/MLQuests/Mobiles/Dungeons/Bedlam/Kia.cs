using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Kia : MondainQuester
{
	[Constructable]
	public Kia()
		: base("Kia", "the Student")
	{
	}

	public Kia(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(MomentoQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Human;

		Hue = 0x8418;
		HairItemID = 0x2046;
		HairHue = 0x466;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x743));
		AddItem(new Robe(0x485));
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