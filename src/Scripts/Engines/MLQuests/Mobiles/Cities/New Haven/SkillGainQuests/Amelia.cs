using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Amelia : MondainQuester
{
	public override Type[] Quests => new[]
	{
		typeof(TheRightToolForTheJobQuest)
	};

	public override void InitSbInfo()
	{
		SbInfos.Add(new SbTinker(this));
	}

	[Constructable]
	public Amelia()
		: base("Amelia Youngstone", "the Tinkering Instructor")
	{
		SetSkill(SkillName.ArmsLore, 120.0, 120.0);
		SetSkill(SkillName.Blacksmith, 120.0, 120.0);
		SetSkill(SkillName.Magery, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Swords, 120.0, 120.0);
		SetSkill(SkillName.Tinkering, 120.0, 120.0);
		SetSkill(SkillName.Mining, 120.0, 120.0);
	}

	public Amelia(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078123); // Tinkering is very useful for a blacksmith. You can make your own tools.
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
		AddItem(new Sandals());
		AddItem(new ShortPants());
		AddItem(new HalfApron(0x8AB));
		AddItem(new Doublet());
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}