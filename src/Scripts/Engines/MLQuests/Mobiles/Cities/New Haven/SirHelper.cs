using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class SirHelper : MondainQuester
{
	[Constructable]
	public SirHelper()
		: base("Sir Helper", "the Profession Guide")
	{
	}

	public SirHelper(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new Type[]
	{
		typeof(NewHavenProfessionGuide)
	};
	public override void InitBody()
	{
		InitStats(100, 100, 25);

		Female = false;
		CantWalk = true;
		Race = Race.Human;

		Hue = 0x840B;
		HairItemId = 0x203D;
		HairHue = 0x458;
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new Robe(0x3C9));
		AddItem(new Cloak(0x3C9));
		AddItem(new Shoes(0x740));
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}