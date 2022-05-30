using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Beotham : MondainQuester
	{
		[Constructable]
		public Beotham()
			: base("Beotham", "the Bowcrafter")
		{
		}

		public Beotham(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(BrokenShaftQuest),
					typeof(BendingTheBowQuest),
					typeof(ArmsRaceQuest),
					typeof(ImprovedCrossbowsQuest),
					typeof(BuildingTheBetterCrossbowQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Elf;

			Hue = 0x876C;
			HairItemID = 0x2FC0;
			HairHue = 0x238;
		}

		public override void InitOutfit()
		{
			AddItem(new Sandals(0x901));
			AddItem(new LongPants(0x52C));
			AddItem(new FancyShirt(0x546));

			Item item;

			item = new LeafGloves
			{
				Hue = 0x901
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
