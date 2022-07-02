using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Gorrow : MondainQuester
{
	[Constructable]
	public Gorrow()
		: base("Gorrow", "the Mayor")
	{
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Gorrow(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(CommonBrigandsQuest),
		typeof(ForkedTongueQuest),
		typeof(PointyEarsQuest),
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;

		Hue = 0x8412;
		HairItemId = 0x2047;
		HairHue = 0x465;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x1BB));
		AddItem(new LongPants(0x901));
		AddItem(new Tunic(0x70A));
		AddItem(new Cloak(0x675));
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