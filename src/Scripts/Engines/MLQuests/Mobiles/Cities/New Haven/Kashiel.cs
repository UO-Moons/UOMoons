using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Kashiel : MondainQuester
	{
		[Constructable]
		public Kashiel()
			: base("Kashiel", "the Archer")
		{
		}

		public Kashiel(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(ShotAnArrowIntoTheAirQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Human;

			Hue = 0x83FE;
			HairItemID = 0x2045;
			HairHue = 0x1;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Boots(0x1BB));

			Item item;

			item = new LeatherLegs
			{
				Hue = 0x901
			};
			AddItem(item);

			item = new LeatherGloves
			{
				Hue = 0x1BB
			};
			AddItem(item);

			item = new LeatherChest
			{
				Hue = 0x1BB
			};
			AddItem(item);

			item = new LeatherArms
			{
				Hue = 0x901
			};
			AddItem(item);

			item = new CompositeBow
			{
				Hue = 0x606
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
