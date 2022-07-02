using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Anolly : MondainQuester
{
	[Constructable]
	public Anolly()
		: base("Anolly", "the Bark Weaver")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Anolly(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(StopHarpingOnMeQuest),
		typeof(TheFarEyeQuest),
		typeof(NothingFancyQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x8835;
		HairItemId = 0x2FC0;
		HairHue = 0x325;
	}

	public override void InitOutfit()
	{
		AddItem(new Sandals(0x901));
		AddItem(new FullApron(0x1BB));
		AddItem(new ShortPants(0x3B2));
		AddItem(new SmithHammer());
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