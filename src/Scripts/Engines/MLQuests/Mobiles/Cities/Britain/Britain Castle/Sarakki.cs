using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Sarakki : MondainQuester
{
	[Constructable]
	public Sarakki()
		: base("Sarakki", "the Notary")
	{
	}

	public Sarakki(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(BlackOrderBadgesQuest),
		typeof(EvidenceQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = true;
		Race = Race.Human;

		Hue = 0x841E;
		HairItemId = 0x2049;
		HairHue = 0x1BB;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Shoes(0x740));
		AddItem(new FancyShirt(0x72C));
		AddItem(new Skirt(0x53C));
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