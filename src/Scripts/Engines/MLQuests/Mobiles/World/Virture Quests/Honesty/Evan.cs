using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Evan : MondainQuester
{
	[Constructable]
	public Evan()
		: base("Evan", "the Beggar")
	{
	}

	public Evan(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[] { typeof(HonestBeggarQuest) };
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;

		Hue = 0x841B;
		HairItemId = 0x204A;
		HairHue = 0x451;
		FacialHairItemId = 0x203F;
		FacialHairHue = 0x451;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x737));
		AddItem(new ShortPants(0x74C));
		AddItem(new FancyShirt(0x535));
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