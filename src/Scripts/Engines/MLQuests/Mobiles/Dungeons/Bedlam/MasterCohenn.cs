using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Cohenn : MondainQuester
	{
		[Constructable]
		public Cohenn()
			: base("Master Cohenn", "the Librarian")
		{
		}

		public Cohenn(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(MisplacedQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Human;

			Hue = 0x840C;
			HairItemID = 0x2045;
			HairHue = 0x453;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Sandals(0x74A));
			AddItem(new Robe(0x498));
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
