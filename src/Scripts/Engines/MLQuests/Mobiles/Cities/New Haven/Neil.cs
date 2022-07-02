using Server.Engines.Quests;
using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class Neil : MondainQuester
{
	[Constructable]
	public Neil()
		: base("Neil", "the Iron Worker")
	{
		SetSkill(SkillName.Blacksmith, 65.0, 88.0);
		SetSkill(SkillName.Fencing, 45.0, 68.0);
		SetSkill(SkillName.Macing, 45.0, 68.0);
		SetSkill(SkillName.Swords, 45.0, 68.0);
		SetSkill(SkillName.Tactics, 36.0, 68.0);
		SetSkill(SkillName.Parry, 61.0, 93.0);
	}

	public Neil(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(CrystallineFragmentsQuest),
		typeof(ProtectorsEssenceQuest),
		typeof(HeartOfIceQuest)
	};
	protected override List<SbInfo> SbInfos => MSbInfos;
	public override void InitSbInfo()
	{
		MSbInfos.Add(new SbBlacksmith());
	}

	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x83F5;
		HairItemId = 0x203C;
		HairHue = 0x46F;
		FacialHairItemId = 0x203F;
		FacialHairHue = 0x46F;
	}

	public override void InitOutfit()
	{
		AddItem(new SmithHammer());
		AddItem(new ShortPants(0x3A));
		AddItem(new Bandana(0x30));
		AddItem(new Doublet(0x13));
		AddItem(new RingmailChest());
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
