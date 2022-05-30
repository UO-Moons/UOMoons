using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Yorus : MondainQuester
	{
		[Constructable]
		public Yorus()
			: base("Yorus", "the Tinker")
		{
		}

		public Yorus(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(BullfightingSortOfQuest),
					typeof(ForcedMigrationQuest),
					typeof(FineFeastQuest),
					typeof(OverpopulationQuest),
					typeof(HeroInTheMakingQuest),
					typeof(ThinningTheHerdQuest),
					typeof(TheyllEatAnythingQuest),
					typeof(NoGoodFishStealingQuest),
					typeof(WildBoarCullQuest),
					typeof(TheyreBreedingLikeRabbitsQuest),
					typeof(GuileIrkAndSpiteQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Human;

			Hue = 0x841D;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Shoes(0x755));
			AddItem(new LongPants(0x1BB));
			AddItem(new FancyShirt(0x71E));
			AddItem(new Cloak(0x59));
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
