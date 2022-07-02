using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class DerekMerchant : MondainQuester
{
	public override Type[] Quests => new Type[] { typeof(HonorOfDeBoorsQuest) };

	[Constructable]
	public DerekMerchant()
		: base("Derek", "the Merchant")
	{
	}

	public DerekMerchant(Serial serial)
		: base(serial)
	{
	}

	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Human;

		Hue = 0x8406;
		HairItemId = 0x2048;
		HairHue = 0x473;
		FacialHairItemId = 0x204B;
		FacialHairHue = 0x473;
	}

	public override void InitOutfit()
	{
		AddItem(new Shoes());
		AddItem(new LongPants(0x901));
		AddItem(new FancyShirt(0x5F4));
		AddItem(new Backpack());
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