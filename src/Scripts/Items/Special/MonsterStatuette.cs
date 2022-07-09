using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
	public enum MonsterStatuetteType
	{
		Crocodile,
		Daemon,
		Dragon,
		EarthElemental,
		Ettin,
		Gargoyle,
		Gorilla,
		Lich,
		Lizardman,
		Ogre,
		Orc,
		Ratman,
		Skeleton,
		Troll,
		Cow,
		Zombie,
		Llama,
		Ophidian,
		Reaper,
		Mongbat,
		Gazer,
		FireElemental,
		Wolf,
		PhillipsWoodenSteed,
		Seahorse,
		Harrower,
		Efreet,
		Slime,
		PlagueBeast,
		RedDeath,
		Spider,
		OphidianArchMage,
		OphidianWarrior,
		OphidianKnight,
		OphidianMage,
		DreadHorn,
		Minotaur,
		BlackCat,
		HalloweenGhoul,
		SherryTheMouse,
		SlasherOfVeils,
		StygianDragon,
		Medusa,
		PrimevalLich,
		AbyssalInfernal,
		ArchDemon,
		FireAnt,
		Navrey,
		DragonTurtle,
		TigerCub,
		SakkhranBirdOfPrey,
		Exodus,
		TerathanMatriarch,
		FleshRenderer,
		CrystalElemental,
		DarkFather,
		PlatinumDragon,
		Rex,
		Zipactriotal,
		MyrmidexQueen,
		Virtuebane,
		GreyGoblin,
		GreenGoblin,
		Pyros,
		Lithos,
		Hydros,
		Stratos,
		Santa,
		Krampus,
		KhalAnkur,
		KrampusMinion,
		Horse,
		Pig,
		Goat,
		IceFiend
	}

	public class MonsterStatuetteInfo
	{
		public int LabelNumber { get; }
		public int ItemId { get; }
		public int[] Sounds { get; }

		public MonsterStatuetteInfo(int labelNumber, int itemId, int baseSoundId)
		{
			LabelNumber = labelNumber;
			ItemId = itemId;
			Sounds = new[] { baseSoundId, baseSoundId + 1, baseSoundId + 2, baseSoundId + 3, baseSoundId + 4 };
		}

		public MonsterStatuetteInfo(int labelNumber, int itemId, int[] sounds)
		{
			LabelNumber = labelNumber;
			ItemId = itemId;
			Sounds = sounds;
		}

		private static readonly MonsterStatuetteInfo[] m_Table = {
            /* Crocodile */			new(1041249, 0x20DA, 660),
            /* Daemon */			new(1041250, 0x20D3, 357),
            /* Dragon */			new(1041251, 0x20D6, 362),
            /* EarthElemental */	new(1041252, 0x20D7, 268),
            /* Ettin */			    new(1041253, 0x20D8, 367),
            /* Gargoyle */			new(1041254, 0x20D9, 372),
            /* Gorilla */			new(1041255, 0x20F5, 158),
            /* Lich */			    new(1041256, 0x20F8, 1001),
            /* Lizardman */			new(1041257, 0x20DE, 417),
            /* Ogre */			    new(1041258, 0x20DF, 427),
            /* Orc */			    new(1041259, 0x20E0, 1114),
            /* Ratman */			new(1041260, 0x20E3, 437),
            /* Skeleton */			new(1041261, 0x20E7, 1165),
            /* Troll */		    	new(1041262, 0x20E9, 461),
            /* Cow */			    new(1041263, 0x2103, 120),
            /* Zombie */			new(1041264, 0x20EC, 471),
            /* Llama */			    new(1041265, 0x20F6, 1011),
            /* Ophidian */			new(1049742, 0x2133, 634),
            /* Reaper */			new(1049743, 0x20FA, 442),
            /* Mongbat */			new(1049744, 0x20F9, 422),
            /* Gazer */			    new(1049768, 0x20F4, 377),
            /* FireElemental */		new(1049769, 0x20F3, 838),
            /* Wolf */			    new(1049770, 0x2122, 229),
            /* Phillip's Steed */	new(1063488, 0x3FFE, 168),
            /* Seahorse */			new(1070819, 0x25BA, 138),
            /* Harrower */			new(1080520, 0x25BB, new[] { 0x289, 0x28A, 0x28B }),
            /* Efreet */			new(1080521, 0x2590, 0x300),
            /* Slime */			    new(1015246, 0x20E8, 456),
            /* PlagueBeast */		new(1029747, 0x2613, 0x1BF),
            /* RedDeath */			new(1094932, 0x2617, System.Array.Empty<int>()),
            /* Spider */			new(1029668, 0x25C4, 1170),
            /* OphidianArchMage */	new(1029641, 0x25A9, 639),
            /* OphidianWarrior */	new(1029645, 0x25AD, 634),
            /* OphidianKnight */	new(1029642, 0x25aa, 634),
            /* OphidianMage */		new(1029643, 0x25ab, 639),
            /* DreadHorn */			new(1031651, 0x2D83, 0xA8),
            /* Minotaur */			new(1031657, 0x2D89, 0x596),
            /* Black Cat */		    new(1096928, 0x4688, 0x69),
            /* HalloweenGhoul */	new(1076782, 0x2109, 0x482),
            /* SherryTheMouse */	new(1080171, 0x20D0, 0x0CE),
            /* Slasher of Veils */  new(1113624, 0x42A0, 0x632),
            /* Stygian Dragon   */  new(1113625, 0x42A6, 0x63E),
            /* Medusa */            new(1113626, 0x4298, 0x612),
            /* Primeval Lich */     new(1113627, 0x429A, 0x61E),
            /* Abyssal Infernal */  new(1113628, 0x4287, 1492), 
            /* ArchDemon */         new(1112411, 0x20D3, 357), 
            /* FireAnt */           new(1113801, 0x42A7, 1006),
            /* Navrey Night-Eyes */ new(1153593, 0x4C07, new[] { 0x61B, 0x61C, 0x61D, 0x61E }),
            /* Dragon Turtle */     new(1156367, 0x9848, 362),
            /* Tiger Cub     */     new(1156517, 0x9CA7, 0x69),
            /* SakkhranBirdOfPrey */new(1156699, 0x276A, 0x4FE),
            /* Exodus */            new(1153594, 0x4C08, new[] { 0x301, 0x302, 0x303, 0x304 }),
            /* Terathan Matriarch */new(1113800, 0x212C, 599),
            /* Flesh Renderer */    new(1155746, 0x262F, new[] { 0x34C, 0x354 }),
            /* Crystal Elemental */ new(1155747, 0x2620, 278),
            /* Dark Father */       new(1155748, 0x2632, 0x165),
            /* Platinum Dragon */   new(1155745, 0x2635, new[] { 0x2C1, 0x2C3 }),
            /* TRex */              new(1157078, 0x9DED, 278),
            /* Zipactriotl */       new(1157079, 0x9DE4, 609),
            /* Myrmidex Queen */    new(1157080, 0x9DB6, 959),
            /* Virtuebane */        new(1153592, 0x4C06, 357),
            /* Grey Goblin */       new(1125135, 0xA095, 0x45A),
            /* Green Goblin */      new(1125133, 0xA097, 0x45A),
            /* Pyros */             new(1157993, 0x9F4D, new[] { 0x112, 0x113, 0x114, 0x115, 0x116 }),
            /* Lithos */            new(1157994, 0x9FA1, new[] { 0x10D, 0x10E, 0x10F, 0x110, 0x111 }),
            /* Hydros */            new(1157992, 0x9F49, new[] { 0x117, 0x118, 0x1119, 0x11A, 0x11B }),
            /* Stratos */           new(1157991, 0x9F4C, new[] { 0x108, 0x109, 0x10A, 0x10B, 0x10C }),
            /* Santa */             new(1097968, 0x4A9A, new[] { 1641 }),
            /* Krampus */           new(1158875, 0xA270, new[] { 0x586, 0x587, 0x588, 0x589, 0x58A }),
            /* Khal Ankur */        new(1158877, 0xA1C6, new[] { 0x301, 0x302, 0x303, 0x304, 0x305 }),
            /* Krampus Minion */    new(1158876, 0xA271, new[] { 0X1C8, 0X1C9, 0X1CA, 0X1CB, 0X1CC }),
            /* Horse */             new(1018263, 0xA511, 0x0A9),
            /* Pig */               new(1159417, 0x2101, 0x0C5),
            /* Goat */              new(1159418, 0x2580, 0x09A),
            /* Ice Fiend */         new(1159419, 0x2587, 0x166),
			};

		public static MonsterStatuetteInfo GetInfo(MonsterStatuetteType type)
		{
			var v = (int)type;

			if (v < 0 || v >= m_Table.Length)
				v = 0;

			return m_Table[v];
		}
	}

	public class MonsterStatuette : BaseItem, IRewardItem
	{
		private MonsterStatuetteType _type;
		private bool _turnedOn;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool TurnedOn
		{
			get => _turnedOn;
			set { _turnedOn = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public MonsterStatuetteType Type
		{
			get => _type;
			set
			{
				_type = value;
				ItemId = MonsterStatuetteInfo.GetInfo(_type).ItemId;

				Hue = _type switch
				{
					MonsterStatuetteType.Slime => Utility.RandomSlimeHue(),
					MonsterStatuetteType.RedDeath => 0x21,
					MonsterStatuetteType.HalloweenGhoul => 0xF4,
					_ => 0
				};

				InvalidateProperties();
			}
		}

		public override int LabelNumber => MonsterStatuetteInfo.GetInfo(_type).LabelNumber;

		public override double DefaultWeight => 1.0;

		[Constructable]
		public MonsterStatuette() : this(MonsterStatuetteType.Crocodile)
		{
		}

		[Constructable]
		public MonsterStatuette(MonsterStatuetteType type) : base(MonsterStatuetteInfo.GetInfo(type).ItemId)
		{
			LootType = LootType.Blessed;

			_type = type;

			Hue = _type switch
			{
				MonsterStatuetteType.Slime => Utility.RandomSlimeHue(),
				MonsterStatuetteType.RedDeath => 0x21,
				MonsterStatuetteType.HalloweenGhoul => 0xF4,
				_ => Hue
			};
		}

		public override bool HandlesOnMovement => _turnedOn && IsLockedDown;

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (_turnedOn && IsLockedDown && (!m.Hidden || m.AccessLevel == AccessLevel.Player) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
			{
				var sounds = MonsterStatuetteInfo.GetInfo(_type).Sounds;

				if (sounds.Length > 0)
					Effects.PlaySound(Location, Map, sounds[Utility.Random(sounds.Length)]);
			}

			base.OnMovement(m, oldLocation);
		}

		public MonsterStatuette(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && IsRewardItem)
				list.Add(RewardSystem.GetRewardYearLabel(this, new object[] {_type})); // X Year Veteran Reward
			// turned on// turned off
			list.Add(_turnedOn
				? 502695
				: 502696);
		}

		public bool IsOwner(Mobile mob)
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			return house != null && house.IsOwner(mob);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsOwner(from))
			{
				OnOffGump onOffGump = new(this);
				from.SendGump(onOffGump);
			}
			else
			{
				from.SendLocalizedMessage(502691); // You must be the owner to use this.
			}
		}

		private class OnOffGump : Gump
		{
			private readonly MonsterStatuette _statuette;

			public OnOffGump(MonsterStatuette statuette) : base(150, 200)
			{
				_statuette = statuette;

				AddBackground(0, 0, 300, 150, 0xA28);

				AddHtmlLocalized(45, 20, 300, 35, statuette.TurnedOn ? 1011035 : 1011034, false, false); // [De]Activate this item

				AddButton(40, 53, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(80, 55, 65, 35, 1011036, false, false); // OKAY

				AddButton(150, 53, 0xFA5, 0xFA7, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(190, 55, 100, 35, 1011012, false, false); // CANCEL
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				Mobile from = sender.Mobile;

				if (info.ButtonID == 1)
				{
					bool newValue = !_statuette.TurnedOn;
					_statuette.TurnedOn = newValue;

					if (newValue && !_statuette.IsLockedDown)
						from.SendLocalizedMessage(502693); // Remember, this only works when locked down.
				}
				else
				{
					from.SendLocalizedMessage(502694); // Cancelled action.
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.WriteEncodedInt((int)_type);
			writer.Write(_turnedOn);
			writer.Write(IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						_type = (MonsterStatuetteType)reader.ReadEncodedInt();
						_turnedOn = reader.ReadBool();
						IsRewardItem = reader.ReadBool();
						break;
					}
			}
		}
	}
}
