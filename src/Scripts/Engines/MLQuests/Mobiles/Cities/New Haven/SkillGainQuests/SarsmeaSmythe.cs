using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class SarsmeaSmythe : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(TheInnerWarriorQuest)
	};

	[Constructable]
	public SarsmeaSmythe()
		: base("Sarsmea Smythe", "the Focus Instructor")
	{
		SetSkill(SkillName.Anatomy, 120.0, 120.0);
		SetSkill(SkillName.Parry, 120.0, 120.0);
		SetSkill(SkillName.Healing, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Swords, 120.0, 120.0);
		SetSkill(SkillName.Focus, 120.0, 120.0);
	}

	public SarsmeaSmythe(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078139);  // Know yourself, and you will become a true warrior.
	}

	public override void OnOfferFailed()
	{
		Say(1077772); // I cannot teach you, for you know all I can teach!
	}

	public override void InitBody()
	{
		Female = true;
		CantWalk = true;
		Race = Race.Human;

		base.InitBody();
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new LeatherLegs());
		AddItem(new ThighBoots());
		AddItem(new FemaleLeatherChest());
		AddItem(new StuddedGloves());
		AddItem(new LeatherNinjaBelt());
		AddItem(new StuddedGorget());
		AddItem(new LightPlateJingasa());
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