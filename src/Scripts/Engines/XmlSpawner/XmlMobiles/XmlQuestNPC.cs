using Server.Items;

namespace Server.Mobiles
{
	public class XmlQuestNPC : TalkingBaseCreature
	{

		[Constructable]
		public XmlQuestNPC() : this(-1)
		{
		}

		[Constructable]
		public XmlQuestNPC(int gender) : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.8, 3.0)
		{
			SetStr(10, 30);
			SetDex(10, 30);
			SetInt(10, 30);

			Fame = 50;
			Karma = 50;

			CanHearGhosts = true;

			SpeechHue = Utility.RandomDyedHue();
			Title = string.Empty;
			Hue = Utility.RandomSkinHue();

			switch (gender)
			{
				case -1: Female = Utility.RandomBool(); break;
				case 0: Female = false; break;
				case 1: Female = true; break;
			}

			if (Female)
			{
				Body = 0x191;
				Name = NameList.RandomName("female");
				Item hair = new(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2045, 0x204A, 0x2046, 0x2049))
				{
					Hue = Utility.RandomHairHue(),
					Layer = Layer.Hair,
					Movable = false
				};
				AddItem(hair);
				Item hat = null;
				switch (Utility.Random(5))//4 hats, one empty, for no hat
				{
					case 0: hat = new FloppyHat(Utility.RandomNeutralHue()); break;
					case 1: hat = new FeatheredHat(Utility.RandomNeutralHue()); break;
					case 2: hat = new Bonnet(); break;
					case 3: hat = new Cap(Utility.RandomNeutralHue()); break;
				}
				AddItem(hat);
				Item pants = null;
				switch (Utility.Random(3))
				{
					case 0: pants = new ShortPants(GetRandomHue()); break;
					case 1: pants = new LongPants(GetRandomHue()); break;
					case 2: pants = new Skirt(GetRandomHue()); break;
				}
				AddItem(pants);
				Item shirt = null;
				switch (Utility.Random(7))
				{
					case 0: shirt = new Doublet(GetRandomHue()); break;
					case 1: shirt = new Surcoat(GetRandomHue()); break;
					case 2: shirt = new Tunic(GetRandomHue()); break;
					case 3: shirt = new FancyDress(GetRandomHue()); break;
					case 4: shirt = new PlainDress(GetRandomHue()); break;
					case 5: shirt = new FancyShirt(GetRandomHue()); break;
					case 6: shirt = new Shirt(GetRandomHue()); break;
				}
				AddItem(shirt);
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName("male");
				Item hair = new(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2044, 0x2045, 0x2047, 0x2048))
				{
					Hue = Utility.RandomHairHue(),
					Layer = Layer.Hair,
					Movable = false
				};
				AddItem(hair);
				Item beard = new(Utility.RandomList(0x0000, 0x203E, 0x203F, 0x2040, 0x2041, 0x2067, 0x2068, 0x2069))
				{
					Hue = hair.Hue,
					Layer = Layer.FacialHair,
					Movable = false
				};
				AddItem(beard);
				Item hat = null;
				switch (Utility.Random(7)) //6 hats, one empty, for no hat
				{
					case 0: hat = new SkullCap(GetRandomHue()); break;
					case 1: hat = new Bandana(GetRandomHue()); break;
					case 2: hat = new WideBrimHat(); break;
					case 3: hat = new TallStrawHat(Utility.RandomNeutralHue()); break;
					case 4: hat = new StrawHat(Utility.RandomNeutralHue()); break;
					case 5: hat = new TricorneHat(Utility.RandomNeutralHue()); break;
					default:
						break;
				}
				AddItem(hat);
				Item pants = null;
				switch (Utility.Random(2))
				{
					case 0: pants = new ShortPants(GetRandomHue()); break;
					case 1: pants = new LongPants(GetRandomHue()); break;
				}
				AddItem(pants);
				Item shirt = null;
				switch (Utility.Random(5))
				{
					case 0: shirt = new Doublet(GetRandomHue()); break;
					case 1: shirt = new Surcoat(GetRandomHue()); break;
					case 2: shirt = new Tunic(GetRandomHue()); break;
					case 3: shirt = new FancyShirt(GetRandomHue()); break;
					case 4: shirt = new Shirt(GetRandomHue()); break;
				}
				AddItem(shirt);
			}

			Item feet = null;
			switch (Utility.Random(3))
			{
				case 0: feet = new Boots(Utility.RandomNeutralHue()); break;
				case 1: feet = new Shoes(Utility.RandomNeutralHue()); break;
				case 2: feet = new Sandals(Utility.RandomNeutralHue()); break;
			}
			AddItem(feet);
			Container pack = new Backpack();

			pack.DropItem(new Gold(0, 50));

			pack.Movable = false;

			AddItem(pack);
		}

		public XmlQuestNPC(Serial serial) : base(serial)
		{
		}



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
