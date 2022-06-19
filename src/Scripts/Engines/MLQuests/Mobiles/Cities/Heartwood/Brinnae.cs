using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Brinnae : MondainQuester
{
	[Constructable]
	public Brinnae()
		: base("Brinnae", "the Wise")
	{
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Brinnae(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(EvilEyeQuest),
		typeof(ImpishDelightsQuest),
		typeof(StirringTheNestQuest),
		typeof(UndeadMagesQuest),
		typeof(TheAfterlifeQuest),
		typeof(FriendlyNeighborhoodSpiderkillerQuest),
		typeof(GargoylesWrathQuest),
		typeof(ThreeWishesQuest),
		typeof(ForkedTongueQuest),
		typeof(CircleOfLifeQuest),
		typeof(MongbatMenaceQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x8382;
		HairItemID = 0x2FD0;
		HairHue = 0x852;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x1BB));
		AddItem(new LeafArms());
		AddItem(new FemaleLeafChest());
		AddItem(new HidePants());
		AddItem(new ElvenCompositeLongbow());
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