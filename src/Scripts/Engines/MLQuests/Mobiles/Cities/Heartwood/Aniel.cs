using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Aniel : MondainQuester
{
	[Constructable]
	public Aniel()
		: base("Aniel", "the Aborist")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Aniel(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(GlassyFoeQuest),
		typeof(CircleOfLifeQuest),
		typeof(DustToDustQuest),
		typeof(ArchSupportQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x8384;
		HairItemId = 0x2FC2;
		HairHue = 0x36;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x901));
		AddItem(new HalfApron(0x759));
		AddItem(new ElvenPants(0x901));
		AddItem(new LeafChest());
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