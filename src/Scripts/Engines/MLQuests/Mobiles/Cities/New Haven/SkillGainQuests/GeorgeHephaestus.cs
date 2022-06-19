using System;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class GeorgeHephaestus : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(ItsHammerTimeQuest)
	};

	public override void InitSbInfo()
	{
		SbInfos.Add(new SbBlacksmith());
	}

	[Constructable]
	public GeorgeHephaestus()
		: base("George Hephaestus", "the Blacksmith Instructor")
	{
		SetSkill(SkillName.ArmsLore, 120.0, 120.0);
		SetSkill(SkillName.Blacksmith, 120.0, 120.0);
		SetSkill(SkillName.Magery, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Swords, 120.0, 120.0);
		SetSkill(SkillName.Tinkering, 120.0, 120.0);
		SetSkill(SkillName.Mining, 120.0, 120.0);
	}

	public GeorgeHephaestus(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078122); // Wanna learn how to make powerful weapons and armor? Talk to me.
	}

	public override void OnOfferFailed()
	{
		Say(1077772); // I cannot teach you, for you know all I can teach!
	}

	public override void InitBody()
	{
		Female = false;
		CantWalk = true;
		Race = Race.Human;

		base.InitBody();
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Boots(0x973));
		AddItem(new LongPants());
		AddItem(new Bascinet());
		AddItem(new FullApron(0x8AB));

		Item item;

		item = new SmithHammer();
		item.Hue = 0x8AB;
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

		int version = reader.ReadInt();
	}
}