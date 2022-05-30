using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Cailla : MondainQuester
	{
		[Constructable]
		public Cailla()
			: base("Cailla", "the Guard")
		{
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.Focus, 60.0, 83.0);
		}

		public Cailla(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(KingOfBearsQuest),
					typeof(SpecimensQuest),
					typeof(DeadManWalkingQuest),
					typeof(SpiritsQuest),
					typeof(RollTheBonesQuest),
					typeof(ItsGhastlyJobQuest),
					typeof(TroglodytesQuest),
					typeof(UnholyKnightsQuest),
					typeof(FriendlyNeighborhoodSpiderkillerQuest),
					typeof(FeatherInYerCapQuest),
					typeof(TaleOfTailQuest),
					typeof(TrogAndHisDogQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Elf;

			Hue = 0x876B;
			HairItemID = 0x2FCE;
			HairHue = 0x2C8;
		}

		public override void InitOutfit()
		{
			AddItem(new ElvenBoots());
			AddItem(new MagicalShortbow());
			AddItem(new HidePants());
			AddItem(new HidePauldrons());
			AddItem(new HideGloves());
			AddItem(new HideFemaleChest());
			AddItem(new WoodlandBelt());

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
