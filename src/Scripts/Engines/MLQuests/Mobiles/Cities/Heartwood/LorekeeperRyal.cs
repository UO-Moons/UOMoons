using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Ryal : MondainQuester
{
	[Constructable]
	public Ryal()
		: base("Lorekeeper Ryal", "the Keeper of Tradition")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Ryal(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(DaemonicPrismQuest),
		typeof(HowManyHeadsQuest),
		typeof(GlassyFoeQuest),
		typeof(HailstormQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x82FE;
		HairItemId = 0x2FC2;
		HairHue = 0x324;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x1BB));
		AddItem(new Cloak(0x219));
		AddItem(new LeafTonlet());
		AddItem(new GnarledStaff());
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