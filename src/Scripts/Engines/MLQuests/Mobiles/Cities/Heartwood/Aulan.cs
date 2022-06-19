using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Aulan : MondainQuester
{
	[Constructable]
	public Aulan()
		: base("Aulan", "the Expeditionist")
	{
		SetSkill(SkillName.Meditation, 60.0, 83.0);
		SetSkill(SkillName.Focus, 60.0, 83.0);
	}

	public Aulan(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(CreepyCrawliesQuest),
		typeof(VoraciousPlantsQuest),
		typeof(GibberJabberQuest),
		typeof(AnimatedMonstrosityQuest),
		typeof(BirdsOfAFeatherQuest),
		typeof(FrightmaresQuest),
		typeof(MoltenReptilesQuest),
		typeof(BloodyNuisanceQuest),
		typeof(BloodSuckersQuest)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		Race = Race.Elf;

		Hue = 0x847E;
		HairItemID = 0x2FC1;
		HairHue = 0x852;
	}

	public override void InitOutfit()
	{
		AddItem(new ElvenBoots(0x725));
		AddItem(new ElvenPants(0x3B3));
		AddItem(new Cloak(0x16A));
		AddItem(new Circlet());

		Item item;

		item = new HideGloves
		{
			Hue = 0x224
		};
		AddItem(item);

		item = new HideChest
		{
			Hue = 0x224
		};
		AddItem(item);

		item = new HidePauldrons
		{
			Hue = 0x224
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