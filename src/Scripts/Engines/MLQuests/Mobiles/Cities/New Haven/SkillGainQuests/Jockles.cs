using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Jockles : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(TheWayOfTheBladeQuest)
	};

	[Constructable]
	public Jockles()
		: base("Jockles", "the Swordsmanship Instructor")
	{
		SetSkill(SkillName.Anatomy, 120.0, 120.0);
		SetSkill(SkillName.Parry, 120.0, 120.0);
		SetSkill(SkillName.Healing, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Swords, 120.0, 120.0);
		SetSkill(SkillName.Focus, 120.0, 120.0);
	}

	public Jockles(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078135); // Talk to me to learn the way of the blade.
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
		AddItem(new Broadsword());
		AddItem(new PlateChest());
		AddItem(new PlateLegs());
		AddItem(new PlateGloves());
		AddItem(new PlateArms());
		AddItem(new PlateGorget());
		AddItem(new OrderShield());
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