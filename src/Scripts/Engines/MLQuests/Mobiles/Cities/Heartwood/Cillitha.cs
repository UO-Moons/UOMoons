using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Cillitha : MondainQuester
{
	[Constructable]
	public Cillitha()
		: base("Cillitha", "the Bowcrafter")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Cillitha(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(LethalDartsQuest),
		typeof(SimpleBowQuest),
		typeof(IngeniousArcheryPartOneQuest),
		typeof(IngeniousArcheryPartTwoQuest),
		typeof(IngeniousArcheryPartThreeQuest),
		typeof(StopHarpingOnMeQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x83E6;
		HairItemID = 0x2FC2;
		HairHue = 0x8E;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x901));
		AddItem(new ElvenShirt(0x714));
		AddItem(new LeafLegs());
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