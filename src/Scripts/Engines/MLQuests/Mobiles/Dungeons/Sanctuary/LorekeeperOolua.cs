using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Oolua : MondainQuester
{
	[Constructable]
	public Oolua()
		: base("Lorekeeper Oolua", "the Keeper Of Tradition")
	{
	}

	public Oolua(Serial serial)
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
		typeof(PlagueLordQuest),
		typeof(OrcSlayingQuest),
		typeof(DreadhornQuest),
		typeof(PixieDustToDustQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		CantWalk = true;
		Race = Race.Elf;

		Hue = 0x853F;
		HairItemId = 0x2FCC;
		HairHue = 0x388;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x70E));
		AddItem(new WildStaff());
		AddItem(new GemmedCirclet());
		AddItem(new Cloak(0x1BB));
		AddItem(new Skirt(0x3));
		AddItem(new FancyShirt(0x70A));
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