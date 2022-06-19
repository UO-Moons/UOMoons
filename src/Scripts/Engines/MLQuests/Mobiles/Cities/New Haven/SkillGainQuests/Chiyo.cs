using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Chiyo : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(BecomingOneWithTheShadowsQuest)
	};

	[Constructable]
	public Chiyo()
		: base("Chiyo", "the Hiding Instructor")
	{
		SetSkill(SkillName.Hiding, 120.0, 120.0);
		SetSkill(SkillName.Tracking, 120.0, 120.0);
		SetSkill(SkillName.Healing, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Fencing, 120.0, 120.0);
		SetSkill(SkillName.Stealth, 120.0, 120.0);
		SetSkill(SkillName.Ninjitsu, 120.0, 120.0);
	}

	public Chiyo(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078165); // To be undetected means you cannot be harmed.
	}

	public override void OnOfferFailed()
	{
		Say(1077772); // I cannot teach you, for you know all I can teach!
	}

	public override void InitBody()
	{
		Female = false;
		CantWalk = true;
		Body = 247;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
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