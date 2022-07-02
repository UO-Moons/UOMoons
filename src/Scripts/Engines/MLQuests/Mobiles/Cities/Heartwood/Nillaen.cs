using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Nillaen : MondainQuester
{
	[Constructable]
	public Nillaen()
		: base("Lorekeeper Nillaen", "the Keeper of Tradition")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Nillaen(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(ParoxysmusSuccubiQuest),
		typeof(ParoxysmusMolochQuest),
		typeof(ParoxysmusDaemonsQuest),
		typeof(ParoxysmusArcaneDaemonsQuest),
		typeof(CausticComboQuest),
		typeof(PlagueLordQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Elf;

		Hue = 0x8367;
		HairItemId = 0x2FCF;
		HairHue = 0x26B;
	}

	public override void InitOutfit()
	{
		AddItem(new Shoes(0x1BB));
		AddItem(new LongPants(0x1FB));
		AddItem(new ElvenShirt());
		AddItem(new GemmedCirclet());
		AddItem(new BodySash(0x25));
		AddItem(new BlackStaff());
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