using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class NewHavenPeasant : NewHavenEscortable
{
	[Constructable]
	public NewHavenPeasant()
	{
		Title = "the peasant";
	}

	public NewHavenPeasant(Serial serial)
		: base(serial)
	{
	}

	public override bool ClickTitle => false;
	public override void InitOutfit()
	{
		if (Female)
			AddItem(new PlainDress());
		else
			AddItem(new Shirt(GetRandomHue()));

		int lowHue = GetRandomHue();

		AddItem(new ShortPants(lowHue));

		if (Female)
			AddItem(new Boots(lowHue));
		else
			AddItem(new Shoes(lowHue));

		Utility.AssignRandomHair(this);

		// PackItem(Loot.PackGold(200, 250));
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