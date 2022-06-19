using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class Churchill : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(CrushingBonesAndTakingNamesQuest)
	};

	[Constructable]
	public Churchill()
		: base("Churchill", "the Mace Fighting Instructor")
	{
		SetSkill(SkillName.Anatomy, 120.0, 120.0);
		SetSkill(SkillName.Parry, 120.0, 120.0);
		SetSkill(SkillName.Healing, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Macing, 120.0, 120.0);
		SetSkill(SkillName.Focus, 120.0, 120.0);
	}

	public Churchill(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078141); // Don't listen to Jockles. Real warriors wield mace weapons!
	}

	public override void OnOfferFailed()
	{
		Say(1077772); // I cannot teach you, for you know all I can teach!
	}

	public override void InitBody()
	{
		Female = false;
		CantWalk = true;
		Race = Race.Human;
		Direction = Direction.South;

		base.InitBody();
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new OrderShield());
		AddItem(new WarMace());

		Item item;

		item = new PlateLegs();
		item.Hue = 0x966;
		AddItem(item);

		item = new PlateGloves();
		item.Hue = 0x966;
		AddItem(item);

		item = new PlateGorget();
		item.Hue = 0x966;
		AddItem(item);

		item = new PlateChest();
		item.Hue = 0x966;
		AddItem(item);

		item = new PlateArms();
		item.Hue = 0x966;
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

		int version = reader.ReadInt();
	}
}