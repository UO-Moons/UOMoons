using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Mithneral : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(ScholarlyTaskQuest)
	};

	[Constructable]
	public Mithneral()
		: base("Mithneral", "the Evaluating Intelligence Instructor")
	{
		SetSkill(SkillName.EvalInt, 120.0, 120.0);
		SetSkill(SkillName.Inscribe, 120.0, 120.0);
		SetSkill(SkillName.Magery, 120.0, 120.0);
		SetSkill(SkillName.MagicResist, 120.0, 120.0);
		SetSkill(SkillName.Wrestling, 120.0, 120.0);
		SetSkill(SkillName.Meditation, 120.0, 120.0);
	}

	public Mithneral(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078127); // Want to maximize your spell damage? I have a scholarly task for you!
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
		AddItem(new HoodedShroudOfShadows(0x51C));
		AddItem(new Sandals());
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