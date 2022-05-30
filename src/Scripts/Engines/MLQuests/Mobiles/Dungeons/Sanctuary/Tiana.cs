using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Tiana : MondainQuester
	{
		[Constructable]
		public Tiana()
			: base("Tiana", "the Guard")
		{
		}

		public Tiana(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(MaraudersQuest),
					typeof(ChillInTheAirQuest),
					typeof(IndustriousAsAnAntLionQuest),
					typeof(ThePerilsOfFarmingQuest),
					typeof(UnholyConstructQuest),
					typeof(DishBestServedColdQuest),
					typeof(CommonBrigandsQuest),
					typeof(ArchEnemiesQuest),
					typeof(TrollingForTrollsQuest),
					typeof(DeadManWalkingQuest),
					typeof(ForkedTonguesQuest),
					typeof(VerminQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = true;
			CantWalk = true;
			Race = Race.Elf;

			Hue = 0x824E;
			HairItemID = 0x2FCC;
			HairHue = 0x385;
		}

		public override void InitOutfit()
		{
			AddItem(new ElvenBoots());
			AddItem(new HidePants());
			AddItem(new HideFemaleChest());
			AddItem(new HidePauldrons());
			AddItem(new WoodlandBelt(0x657));

			Item item;

			item = new RavenHelm
			{
				Hue = 0x1BB
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
