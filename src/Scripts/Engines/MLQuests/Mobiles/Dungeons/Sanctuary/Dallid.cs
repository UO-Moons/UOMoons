using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Dallid : MondainQuester
{
	[Constructable]
	public Dallid()
		: base("Dallid", "the Cook")
	{
	}

	public Dallid(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(TappingTheKegQuest),
		typeof(BreezesSongQuest),
		typeof(WaitingToBeFilledQuest),
		typeof(MougGuurMustDieQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Elf;

		Hue = 0x8376;
		HairItemID = 0x2FCD;
		HairHue = 0x100;
	}

	public override void InitOutfit()
	{
		AddItem(new Boots(0x901));
		AddItem(new ShortPants(0x733));
		AddItem(new Shirt(0x70E));
		AddItem(new FullApron(0x1BE));
		AddItem(new Cleaver());
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