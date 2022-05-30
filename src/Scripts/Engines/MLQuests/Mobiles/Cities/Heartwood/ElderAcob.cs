using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class ElderAcob : MondainQuester
	{
		[Constructable]
		public ElderAcob()
			: base("Elder Acob", "the Wise")
		{
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.Focus, 60.0, 83.0);
		}

		public ElderAcob(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(OverpopulationQuest),
					typeof(WildBoarCullQuest),
					typeof(NewLeadershipQuest),
					typeof(ExAssassinsQuest),
					typeof(ExtinguishingTheFlameQuest),
					typeof(DeathToTheNinjaQuest),
					typeof(CrimeAndPunishmentQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Elf;

			Hue = 0x8389;
			HairItemID = 0x2FCF;
			HairHue = 0x389;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new ElvenBoots(0x73D));
			AddItem(new HidePants());
			AddItem(new ElvenShirt(0x71));
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
