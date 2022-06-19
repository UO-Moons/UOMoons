using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Ahie : MondainQuester
{
	[Constructable]
	public Ahie()
		: base("Ahie", "the Cloth Weaver")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Ahie(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(TheKingOfClothingQuest),
		typeof(ThePuffyShirtQuest),
		typeof(FromTheGaultierCollectionQuest),
		typeof(HuteCoutureQuest)
		//Scale Armor missing Quest https://www.uoguide.com/Scale_Armor
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x853F;
		HairItemID = 0x2FCD;
		HairHue = 0x90;
	}

	public override void InitOutfit()
	{
		AddItem(new ThighBoots(0x901));
		AddItem(new FancyShirt(0x72B));
		AddItem(new Cloak(0x1C));
		AddItem(new Skirt(0x62));
		AddItem(new Circlet());
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}