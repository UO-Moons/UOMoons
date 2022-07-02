using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Cloorne : MondainQuester
{
	[Constructable]
	public Cloorne()
		: base("Cloorne", "the Expeditionist")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Cloorne(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(SquishyQuest),
		typeof(BigJobQuest),
		typeof(TrollingForTrollsQuest),
		typeof(OrcSlayingQuest),
		typeof(ColdHeartedQuest),
		typeof(CreepyCrawliesQuest),
		typeof(ForkedTonguesQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x8376;
		HairItemId = 0x2FBF;
		HairHue = 0x386;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x3B3));
		AddItem(new WingedHelm());
		AddItem(new RadiantScimitar());

		Item item;

		item = new WoodlandLegs
		{
			Hue = 0x732
		};
		AddItem(item);

		item = new HideChest
		{
			Hue = 0x727
		};
		AddItem(item);

		item = new LeafArms
		{
			Hue = 0x749
		};
		AddItem(item);
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