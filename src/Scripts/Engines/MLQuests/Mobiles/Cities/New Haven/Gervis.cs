using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Gervis : MondainQuester
{
	[Constructable]
	public Gervis()
		: base("Gervis", "the Blacksmith Trainer")
	{
		SetSkill(SkillName.Blacksmith, 65.0, 88.0);
	}

	public Gervis(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(BatteredBucklersQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x83F5;
		HairItemID = 0x203B;
		HairHue = 0x5EC;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new SmithHammer());
		AddItem(new Boots(0x3B2));
		AddItem(new ShortPants(0x1BB));
		AddItem(new Shirt(0x71F));
		AddItem(new FullApron(0x3B2));

		Item item;

		item = new LeatherGloves
		{
			Hue = 0x3B2
		};
		AddItem(item);
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