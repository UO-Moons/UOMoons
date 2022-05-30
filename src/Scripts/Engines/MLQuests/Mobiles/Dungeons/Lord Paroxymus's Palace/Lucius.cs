using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Lucius : MondainQuester
	{
		[Constructable]
		public Lucius()
			: base("Lucius", "the Adventurer")
		{
		}

		public Lucius(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(WatchYourStepQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Human;

			Hue = 0x83F3;
			HairItemID = 0x2047;
			HairHue = 0x393;
			FacialHairItemID = 0x203F;
			FacialHairHue = 0x393;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Boots(0x717));
			AddItem(new LongPants(0x1BB));
			AddItem(new Cloak(0x71));
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
