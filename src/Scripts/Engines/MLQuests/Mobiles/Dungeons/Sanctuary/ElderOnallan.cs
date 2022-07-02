using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Onallan : MondainQuester
{
	[Constructable]
	public Onallan()
		: base("Elder Onallan", "the Wise")
	{
	}

	public Onallan(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(MaraudersQuest),
		typeof(ProofOfTheDeedQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Race = Race.Elf;
		Female = false;
		CantWalk = true;

		Hue = Race.RandomSkinHue();
		HairItemId = 0x2FD0;
		HairHue = 0x322;
	}

	public override void InitOutfit()
	{
		AddItem(new Shoes(0x70A));
		AddItem(new Cloak(0x651));
		AddItem(new WildStaff());
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