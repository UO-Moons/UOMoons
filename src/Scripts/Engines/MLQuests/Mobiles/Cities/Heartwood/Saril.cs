using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Saril : MondainQuester
{
	[Constructable]
	public Saril()
		: base("Saril", "the Guard")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Saril(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(TheyreBreedingLikeRabbitsQuest),
		typeof(ThinningTheHerdQuest),
		typeof(TheyllEatAnythingQuest),
		typeof(NoGoodFishStealingQuest),
		typeof(HeroInTheMakingQuest),
		typeof(WildBoarCullQuest),
		typeof(ForcedMigrationQuest),
		typeof(BullfightingSortOfQuest),
		typeof(FineFeastQuest),
		typeof(OverpopulationQuest),
		typeof(DeadManWalkingQuest),
		typeof(ForkedTonguesQuest),
		typeof(TrollingForTrollsQuest),
		typeof(FilthyPestsQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x8361;
		HairItemID = 0x2FC1;
		HairHue = 0x127;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x3B2));
		AddItem(new WingedHelm());
		AddItem(new RadiantScimitar());
		AddItem(new WoodlandLegs());
		AddItem(new WoodlandArms());
		AddItem(new WoodlandChest());
		AddItem(new WoodlandBelt());
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