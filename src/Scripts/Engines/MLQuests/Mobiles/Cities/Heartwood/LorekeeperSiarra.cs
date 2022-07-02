using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Siarra : MondainQuester
{
	[Constructable]
	public Siarra()
		: base("Lorekeeper Siarra", "the Keeper of Tradition")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Siarra(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(AllThatGlittersIsNotGoodQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x8384;
		HairItemId = 0x2FC1;
		HairHue = 0x33;
	}

	public override void InitOutfit()
	{
		AddItem(new Sandals(0x1BB));
		AddItem(new LeafTonlet());
		AddItem(new ElvenShirt());
		AddItem(new GemmedCirclet());
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