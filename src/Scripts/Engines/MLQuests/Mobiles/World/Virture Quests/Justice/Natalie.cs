using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Natalie : MondainQuester
{
	[Constructable]
	public Natalie()
		: base("Natalie", "the Lady of Skara Brae")
	{
	}

	public Natalie(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(GuiltyQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x8400;
		HairItemId = 0x2045;
		HairHue = 0x740;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x727));
		AddItem(new FancyShirt(0x53C));
		AddItem(new Skirt(0x534));
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}