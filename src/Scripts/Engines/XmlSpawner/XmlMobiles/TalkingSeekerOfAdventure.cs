using Server.Items;

namespace Server.Mobiles
{
	public class TalkingSeekerOfAdventure : TalkingBaseEscortable
	{
		private static readonly string[] m_Dungeons = new string[]
			{
				"Covetous", "Deceit", "Despise",
				"Destard", "Hythloth", "Shame",
				"Wrong"
			};

		public override string[] GetPossibleDestinations()
		{
			return m_Dungeons;
		}

		[Constructable]
		public TalkingSeekerOfAdventure()
		{
			Title = "the seeker of adventure";
		}

		public override bool ClickTitle => false;  // Do not display 'the seeker of adventure' when single-clicking

		private static int GetRandomHue()
		{
			return Utility.Random(6) switch
			{
				1 => Utility.RandomBlueHue(),
				2 => Utility.RandomGreenHue(),
				3 => Utility.RandomRedHue(),
				4 => Utility.RandomYellowHue(),
				5 => Utility.RandomNeutralHue(),
				_ => 0,
			};
		}

		public override void InitOutfit()
		{
			if (Female)
				AddItem(new FancyDress(GetRandomHue()));
			else
				AddItem(new FancyShirt(GetRandomHue()));

			int lowHue = GetRandomHue();

			AddItem(new ShortPants(lowHue));

			if (Female)
				AddItem(new ThighBoots(lowHue));
			else
				AddItem(new Boots(lowHue));

			if (!Female)
				AddItem(new BodySash(lowHue));

			AddItem(new Cloak(GetRandomHue()));

			AddItem(new Longsword());

			PackGold(100, 150);
		}

		public TalkingSeekerOfAdventure(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
