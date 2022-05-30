using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Lyle : MondainQuester
	{
		[Constructable]
		public Lyle()
			: base("Lyle", "the Mage")
		{
		}

		public Lyle(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(ThePenIsMightierQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Human;

			Hue = 0x83F7;
			HairItemID = 0x204A;
			HairHue = 0x459;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new ThighBoots());
			AddItem(new Robe(0x2FD));
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
