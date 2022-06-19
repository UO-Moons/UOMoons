using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Gnosos : MondainQuester
{
	public override bool ConvertsMageArmor => true;

	[Constructable]
	public Gnosos()
		: base("Master Gnosos", "the Necromancer")
	{
		SetSkill(SkillName.Focus, 60.0, 83.0);
		SetSkill(SkillName.EvalInt, 65.0, 88.0);
		SetSkill(SkillName.Inscribe, 60.0, 83.0);
		SetSkill(SkillName.Necromancy, 64.0, 100.0);
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.MagicResist, 65.0, 88.0);
		SetSkill(SkillName.SpiritSpeak, 36.0, 68.0);
	}

	public Gnosos(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(CommonBrigandsQuest),
		typeof(GoneNativeQuest),
		typeof(PointyEarsQuest),
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;

		Hue = 0x83E8;
		HairItemID = 0x203B;
		FacialHairItemID = 0x2040;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x485));
		AddItem(new Robe(0x497));
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