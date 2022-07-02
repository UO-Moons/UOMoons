using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Clehin : MondainQuester
{
	[Constructable]
	public Clehin()
		: base("Clehin", "the Soil Nurturer")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Clehin(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(CreepyCrawliesQuest),
		typeof(MongbatMenaceQuest),
		typeof(SpecimensQuest),
		typeof(ThinningTheHerdQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x8362;
		HairItemId = 0x2FC2;
		HairHue = 0x324;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots());
		AddItem(new LeafTonlet());
		AddItem(new ElvenShirt());
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