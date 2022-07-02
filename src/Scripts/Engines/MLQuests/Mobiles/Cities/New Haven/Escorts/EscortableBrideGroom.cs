using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class EscortableBrideGroom : TownEscortable
{
	[Constructable]
	public EscortableBrideGroom()
	{
		if (Female)
			Title = "the bride";
		else
			Title = "the groom";
	}

	public EscortableBrideGroom(Serial serial)
		: base(serial)
	{
	}

	public override bool ClickTitle => false;
	public override void InitOutfit()
	{
		if (Female)
			AddItem(new FancyDress());
		else
			AddItem(new FancyShirt());

		int lowHue = GetRandomHue();

		AddItem(new LongPants(lowHue));

		if (Female)
			AddItem(new Shoes(lowHue));
		else
			AddItem(new Boots(lowHue));

		if (Utility.RandomBool())
			HairItemId = 0x203B;
		else
			HairItemId = 0x203C;

		HairHue = Race.RandomHairHue();

		PackItem(Loot.PackGold(200, 250));
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();
	}
}