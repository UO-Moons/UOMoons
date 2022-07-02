using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Jelrice : MondainQuester
{
	[Constructable]
	public Jelrice()
		: base("Jelrice", "the Trader")
	{
	}

	public Jelrice(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(SomethingToWailAboutQuest),
		typeof(RunawaysQuest),
		typeof(ViciousPredatorQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Human;

		Hue = 0x8410;
		HairItemId = 0x2047;
		HairHue = 0x471;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x1BB));
		AddItem(new Skirt(0xD));
		AddItem(new FancyShirt(0x65F));
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