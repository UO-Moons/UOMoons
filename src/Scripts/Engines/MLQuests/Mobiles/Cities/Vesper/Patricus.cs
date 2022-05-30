using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Patricus : MondainQuester
	{
		[Constructable]
		public Patricus()
			: base("Patricus", "the Trader")
		{
		}

		public Patricus(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(HeaveHoQuest),
					typeof(OddsAndEndsQuest)
				};
		public override void InitBody()
		{
			Female = false;
			Race = Race.Human;

			base.InitBody();
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Shoes(0x74B));
			AddItem(new LongPants(0x1C));
			AddItem(new FancyShirt(0x71B));
			AddItem(new Cloak(0x1BB));
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
