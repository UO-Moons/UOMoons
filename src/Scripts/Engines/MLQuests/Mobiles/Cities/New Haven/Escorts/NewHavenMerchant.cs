using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class NewHavenMerchant : NewHavenEscortable
{
	[Constructable]
	public NewHavenMerchant()
	{
		Title = "the merchant";
		SetSkill(SkillName.ItemID, 55.0, 78.0);
		SetSkill(SkillName.ArmsLore, 55, 78);
	}

	public NewHavenMerchant(Serial serial)
		: base(serial)
	{
	}

	public override bool CanTeach => true;
	public override bool ClickTitle => false;
	public override void InitOutfit()
	{
		int lowHue = GetRandomHue();

		AddItem(new ThighBoots());

		if (Female)
			AddItem(new FancyDress(lowHue));
		else
			AddItem(new FancyShirt(lowHue));

		AddItem(new LongPants(lowHue));

		if (!Female)
			AddItem(new BodySash(lowHue));

		//PackItem(Loot.PackGold(200, 250));
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