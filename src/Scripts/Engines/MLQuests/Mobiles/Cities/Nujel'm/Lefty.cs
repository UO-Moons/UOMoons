using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Lefty : MondainQuester
	{
		[Constructable]
		public Lefty()
			: base("Lefty", "the Ticket Seller")
		{
		}

		public Lefty(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(WondersOfTheNaturalWorldQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Human;

			Hue = 0x83F4;
			HairItemID = 0x203B;
			HairHue = 0x470;
		}

		public override void InitOutfit()
		{
			AddItem(new ThighBoots(0x901));
			AddItem(new LongPants(0x70D));
			AddItem(new Tunic(0x30));
			AddItem(new Cloak(0x30));
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
