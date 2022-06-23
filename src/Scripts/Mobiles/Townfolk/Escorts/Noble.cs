using Server.Items;

namespace Server.Mobiles
{
	public class Noble : BaseEscortable
	{
		[Constructable]
		public Noble()
		{
			Title = "the noble";
			Karma = Utility.RandomMinMax(13, -45);
			SetSkill(SkillName.Parry, 80.0, 100.0);
			SetSkill(SkillName.Swords, 80.0, 100.0);
			SetSkill(SkillName.Tactics, 80.0, 100.0);
		}

		public override bool CanTeach => true;
		public override bool ClickTitle => false;  // Do not display 'the noble' when single-clicking

		private static int GetRandomHue()
		{
			switch (Utility.Random(6))
			{
				default:
				case 0: return 0;
				case 1: return Utility.RandomBlueHue();
				case 2: return Utility.RandomGreenHue();
				case 3: return Utility.RandomRedHue();
				case 4: return Utility.RandomYellowHue();
				case 5: return Utility.RandomNeutralHue();
			}
		}

		public override void InitOutfit()
		{
			if (Female)
				SetWearable(new FancyDress());
			else
				SetWearable(new FancyShirt(GetRandomHue()));

			int lowHue = GetRandomHue();

			SetWearable(new ShortPants(lowHue));

			if (Female)
				SetWearable(new ThighBoots(lowHue));
			else
				SetWearable(new Boots(lowHue));

			if (!Female)
				SetWearable(new BodySash(lowHue));

			SetWearable(new Cloak(GetRandomHue()));

			if (!Female)
				SetWearable(new Longsword());

			Utility.AssignRandomHair(this);

			PackGold(200, 250);
		}

		public Noble(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
