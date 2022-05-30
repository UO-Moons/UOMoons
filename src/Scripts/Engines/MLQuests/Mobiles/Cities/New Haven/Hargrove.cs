using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Hargrove : MondainQuester
	{
		[Constructable]
		public Hargrove()
			: base("Hargrove", "the Lumberjack")
		{
		}

		public Hargrove(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(ChopChopOnTheDoubleQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Human;

			Hue = 0x83FF;
			HairItemID = 0x203C;
			HairHue = 0x0;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new BattleAxe());
			AddItem(new Boots(0x901));
			AddItem(new StuddedLegs());
			AddItem(new Shirt(0x288));
			AddItem(new Bandana(0x20));

			Item item;

			item = new PlateGloves
			{
				Hue = 0x21E
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
}
