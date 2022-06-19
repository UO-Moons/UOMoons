using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class ElderCaelas : MondainQuester
{
	[Constructable]
	public ElderCaelas()
		: base("Elder Caelas", "the Wise")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public ElderCaelas(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(WarriorCasteQuest),
		typeof(BigWormsQuest),
		typeof(ItsElementalQuest),
		typeof(OrcishEliteQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x8381;
		HairItemID = 0x2FC0;
		HairHue = 0x2C8;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x1BB));
		AddItem(new Cloak(0x71B));
		AddItem(new RoyalCirclet());
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