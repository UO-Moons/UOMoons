using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Walker : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(EyesOfRangerQuest)
	};

	[Constructable]
	public Walker()
		: base("Walker", "the Tracking Instructor")
	{
		SetSkill(SkillName.Hiding, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Tracking, 120.0, 120.0);
		SetSkill(SkillName.Fencing, 120.0, 120.0);
		SetSkill(SkillName.Wrestling, 120.0, 120.0);
		SetSkill(SkillName.Stealth, 120.0, 120.0);
		SetSkill(SkillName.Ninjitsu, 120.0, 120.0);
	}

	public Walker(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(Utility.RandomMinMax(1078212, 1078214));
	}

	public override void OnOfferFailed()
	{
		Say(1077772); // I cannot teach you, for you know all I can teach!
	}

	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		base.InitBody();
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Boots(0x455));
		AddItem(new LongPants(0x455));
		AddItem(new FancyShirt(0x47D));
		AddItem(new FloppyHat(0x455));
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