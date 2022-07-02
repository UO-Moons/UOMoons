using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Alejaha : MondainQuester
{
	[Constructable]
	public Alejaha()
		: base("Elder Alejaha", "the Wise")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Alejaha(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(ItsElementalQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x8361;
		HairItemId = 0x2FCD;
		HairHue = 0x852;
	}

	public override void InitOutfit()
	{
		AddItem(new Sandals(0x1BB));
		AddItem(new Cloak(0x59));
		AddItem(new Skirt(0x901));
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

		int version = reader.ReadInt();
	}
}