using Server.ContextMenus;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Engines.Champions;
using Server.Misc;
using Server.Multis;
using Server.Regions;
using Server.Spells;

namespace Server.Items;

public class TreasureMap : MapItem
{
	private int m_Level;
	private bool m_Completed;
	private Mobile m_CompletedBy;
	private Mobile m_Decoder;
	private Map m_Map;
	private Point2D m_Location;

	private static TimeSpan ResetTime => TimeSpan.FromDays(30.0);

	[CommandProperty(AccessLevel.GameMaster)]
	public int Level { get => m_Level; set { m_Level = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool Completed { get => m_Completed; set { m_Completed = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	private Mobile CompletedBy { get => m_CompletedBy; set { m_CompletedBy = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	private Mobile Decoder { get => m_Decoder; set { m_Decoder = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public Map ChestMap { get => m_Map; set { m_Map = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	private Point2D ChestLocation { get => m_Location; set => m_Location = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	private DateTime NextReset { get; set; }

	public const double LootChance = 0.01; // 1% chance to appear as loot

	#region Forgotten Treasures
	private TreasurePackage m_Package;

	[CommandProperty(AccessLevel.GameMaster)]
	public TreasureLevel TreasureLevel
	{
		get => (TreasureLevel)m_Level;
		set
		{
			if ((int)value != Level)
			{
				Level = (int)value;
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public TreasurePackage Package
	{
		get => m_Package;
		private set { m_Package = value; InvalidateProperties(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public TreasureFacet TreasureFacet => TreasureMapInfo.GetFacet(ChestLocation, Facet);

	private void AssignRandomPackage()
	{
		Package = (TreasurePackage)Utility.Random(5);
	}

	private void AssignChestQuality(Mobile digger, TreasureMapChest chest)
	{
		double skill = digger.Skills[SkillName.Cartography].Value;
		var dif = TreasureLevel switch
		{
			TreasureLevel.Supply => 200,
			TreasureLevel.Cache => 300,
			TreasureLevel.Hoard => 400,
			TreasureLevel.Trove => 500,
			_ => 100,
		};
		if (Utility.Random(dif) <= skill)
		{
			chest.ChestQuality = ChestQuality.Gold;
		}
		else if (Utility.Random(dif) <= skill * 2)
		{
			chest.ChestQuality = ChestQuality.Standard;
		}
		else
		{
			chest.ChestQuality = ChestQuality.Rusty;
		}
	}
	#endregion

	#region Spawn Types
	private static readonly Type[][] m_SpawnTypes = new[]
	{
			new[]{ typeof( HeadlessOne ), typeof( Skeleton ) },
			new[]{ typeof( Mongbat ), typeof( Ratman ), typeof( HeadlessOne ), typeof( Skeleton ), typeof( Zombie ) },
			new[]{ typeof( OrcishMage ), typeof( Gargoyle ), typeof( Gazer ), typeof( HellHound ), typeof( EarthElemental ) },
			new[]{ typeof( Lich ), typeof( OgreLord ), typeof( DreadSpider ), typeof( AirElemental ), typeof( FireElemental ) },
			new[]{ typeof( DreadSpider ), typeof( LichLord ), typeof( Daemon ), typeof( ElderGazer ), typeof( OgreLord ) },
			new[]{ typeof( LichLord ), typeof( Daemon ), typeof( ElderGazer ), typeof( PoisonElemental ), typeof( BloodElemental ) },
			new[]{ typeof( AncientWyrm ), typeof( Balron ), typeof( BloodElemental ), typeof( PoisonElemental ), typeof( Titan ) },
			new[]{ typeof( BloodElemental), /*typeof(ColdDrake), typeof(FrostDragon), typeof(FrostDrake),*/ typeof(GreaterDragon), typeof(PoisonElemental)}
	};

	private static readonly Type[][] m_TokunoSpawnTypes = {
			new[]{ typeof( HeadlessOne ), typeof( Skeleton ) },
			new[]{ typeof( HeadlessOne ), typeof( Mongbat ), typeof( Ratman ), typeof( Skeleton), typeof( Zombie ),  },
			new[]{ typeof( EarthElemental ), typeof( Gazer ), typeof( Gargoyle ), typeof( HellHound ), typeof( OrcishMage ), },
			new[]{ typeof( AirElemental ), typeof( DreadSpider ), typeof( FireElemental ), typeof( Lich ), typeof( OgreLord ), },
			new[]{ typeof( ElderGazer ), typeof( Daemon ), typeof( DreadSpider ), typeof( LichLord ), typeof( OgreLord ), },
			new[]{ typeof( FanDancer ), typeof( RevenantLion ), typeof( Ronin ), typeof( RuneBeetle ) },
			new[]{ typeof( Hiryu ), typeof( LadyOfTheSnow ), typeof( Oni ), typeof( RuneBeetle ), typeof( YomotsuWarrior ), typeof( YomotsuPriest ) },
			new[]{ typeof( Yamandon ), typeof( LadyOfTheSnow ), typeof( RuneBeetle ), typeof( YomotsuPriest ) }
	};

	private static readonly Type[][] m_MalasSpawnTypes = {
			new[]{ typeof( HeadlessOne ), typeof( Skeleton ) },
			new[]{ typeof( Mongbat ), typeof( Ratman ), typeof( HeadlessOne ), typeof( Skeleton ), typeof( Zombie ) },
			new[]{ typeof( OrcishMage ), typeof( Gargoyle ), typeof( Gazer ), typeof( HellHound ), typeof( EarthElemental ) },
			new[]{ typeof( Lich ), typeof( OgreLord ), typeof( DreadSpider ), typeof( AirElemental ), typeof( FireElemental ) },
			new[]{ typeof( DreadSpider ), typeof( LichLord ), typeof( Daemon ), typeof( ElderGazer ), typeof( OgreLord ) },
			new[]{ typeof( LichLord ), typeof( Ravager ), typeof( WandererOfTheVoid ), typeof( Minotaur ) },
			new[]{ typeof( Devourer ), typeof( MinotaurScout ), typeof( MinotaurCaptain ), typeof( RottingCorpse ), typeof( WandererOfTheVoid ) },
			new[]{ typeof( Devourer ), typeof( MinotaurGeneral ), typeof( MinotaurCaptain ), typeof( RottingCorpse ), typeof( WandererOfTheVoid ) }

	};

	private static readonly Type[][] m_IlshenarSpawnTypes = {
			new[]{ typeof( HeadlessOne ), typeof( Skeleton ) },
			new[]{ typeof( Mongbat ), typeof( Ratman ), typeof( HeadlessOne ), typeof( Skeleton ), typeof( Zombie ) },
			new[]{ typeof( OrcishMage ), typeof( Gargoyle ), typeof( Gazer ), typeof( HellHound ), typeof( EarthElemental ) },
			new[]{ typeof( Lich ), typeof( OgreLord ), typeof( DreadSpider ), typeof( AirElemental ), typeof( FireElemental ) },
			new[]{ typeof( DreadSpider ), typeof( LichLord ), typeof( Daemon ), typeof( ElderGazer ), typeof( OgreLord ) },
			new[]{ typeof( DarkGuardian ), typeof( ExodusOverseer ), typeof( GargoyleDestroyer ), typeof( GargoyleEnforcer ), typeof( PoisonElemental ) },
			new[]{ typeof( Changeling ), typeof( ExodusMinion ), typeof( GargoyleEnforcer ), typeof( GargoyleDestroyer ), typeof( Titan ) },
			new[]{ typeof( RenegadeChangeling ), typeof( ExodusMinion ), typeof( GargoyleEnforcer ), typeof( GargoyleDestroyer ), typeof( Titan ) }
	};

	private static readonly Type[][] m_TerMurSpawnTypes = {
			new[]{ typeof( HeadlessOne ), typeof( Skeleton ) },
			new[]{ typeof( ClockworkScorpion ), typeof( CorrosiveSlime ), typeof( GreaterMongbat ) },
			new[]{ typeof( AcidSlug ), typeof( FireElemental ), typeof( WaterElemental ) },
			new[]{ typeof( LeatherWolf ), typeof( StoneSlith ), typeof( ToxicSlith ) },
			new[]{ typeof( BloodWorm ), typeof( Kepetch ), typeof( StoneSlith ), typeof( ToxicSlith ) },
			new[]{ typeof( FireAnt ), typeof( LavaElemental ), typeof( MaddeningHorror ) },
			new[]{ typeof( EnragedEarthElemental ), typeof( FireDaemon ), typeof( GreaterPoisonElemental ), typeof( LavaElemental ), typeof( DragonWolf ) },
			new[]{ typeof( EnragedColossus ), typeof( EnragedEarthElemental ), typeof( FireDaemon ), typeof( GreaterPoisonElemental ), typeof( LavaElemental ) }
	};

	private static readonly Type[][] m_EodonSpawnTypes = {
			new[] { typeof(MyrmidexLarvae), /*typeof(SilverbackGorilla),*/ typeof(Panther)/*, typeof(WildTiger)*/ },
			new[] { typeof(AcidElemental), typeof(SandVortex)/* typeof(Lion), typeof(SabreToothedTiger)*/ },
			new[] { typeof(AcidElemental), typeof(SandVortex)/*, typeof(Lion), typeof(SabreToothedTiger)*/ },
			new[] { /*typeof(Infernus),*/ typeof(FireElemental)/*, typeof(Dimetrosaur), typeof(Saurosaurus)*/ },
			new[] {/* typeof(Infernus),*/ typeof(FireElemental)/*, typeof(Dimetrosaur), typeof(Saurosaurus)*/ },
			/*new Type[] { typeof(KotlAutomaton), typeof(MyrmidexDrone), typeof(Allosaurus), typeof(Triceratops) },*/
			new[] { /*typeof(Anchisaur), typeof(Allosaurus),*/ typeof(SandVortex) }
	};
	#endregion

	#region Spawn Locations
	private static readonly Rectangle2D[] m_FelTramWrap = {
			new(0, 0, 5119, 4095)
	};

	private static readonly Rectangle2D[] m_TokunoWrap = {
			new(155, 207, 30, 40),
			new(280, 230, 157, 45),
			new(445, 215, 30, 35 ),
			new(447, 53, 58, 40),
			new(612, 240, 20, 17),
			new(167, 275, 53, 60),
			new(734, 407, 14, 22),
			new(753, 489, 8, 30),
			new(624, 619, 20, 24),
			new(624, 725, 8, 8),
			new(574, 734, 20, 16),
			new(431, 752, 25, 27),
			new(348, 968, 52, 135),
			new(282, 1188, 90, 100),
			new(348, 1335, 50, 50),

			new(228, 284, 500, 316),
			new(95, 600, 345, 243),
			new(155, 842, 146, 1358),
			new(495, 812, 435, 350),
			new(501, 1156, 100, 150),
			new(876, 1156, 90, 150),

			new(970, 1159, 14, 25),
			new(990, 1151, 5, 15),
			new(1004, 1120, 16, 30),
			new(1008, 1032, 12, 15),
			new(1163, 383, 20, 20),

			new(839, 30, 168, 120),
			new(707, 150, 307, 250),
			new(845, 397, 179, 75),
			new(1068, 382, 60, 80),
			new(787, 687, 60, 72),
			new(848, 473, 557, 655),
	};

	private static readonly Rectangle2D[] m_MalasWrap = {
			new(611, 67, 1862, 705),
			new(1540, 852, 286, 182),
			new(602, 784, 546, 746),
			new(1160, 1035, 1299, 871)
	};

	private static readonly Rectangle2D[] m_IlshenarWrap = {
			new(221, 314, 657, 286),
			new(530, 600, 212, 205),
			new(261, 805, 495, 655),
			new(908, 925, 90, 170),
			new(1031, 904, 730, 450),
			new(1028, 630, 318, 161),
			new(1205, 368, 265, 237),
			new(1551, 516, 200, 130),
	};

	private static readonly Rectangle2D[] m_TerMurWrap = {
			new(535, 2895, 85, 117),
			new(525, 3085, 115, 70),
			new(755, 2860, 400, 270),
			new(1025, 3280, 190, 100 ),
			new(305, 3445, 175, 255),
			new(480, 3540, 90, 110),
			new(605, 3880, 200, 170),
			new(750, 3830, 80, 80),
	};

	private static readonly Rectangle2D[] m_EodonWrap = {
			new(259, 1400, 354, 510),
			new(259, 1400, 354, 510),
			new(259, 1400, 354, 510),
			new(688, 1440, 46, 88),
			new(613, 1466, 65, 139),
			new(678, 1568, 43, 40),
			new(613, 1720, 91, 72),
			new(618, 1792, 44, 273),
			new(662, 1969, 84, 166),
			new(754, 1963, 100, 65),
			new(174, 1540, 85, 420),
	};
	#endregion

	private static Map GetRandomMap()
	{
		return Utility.Random(8) switch
		{
			1 => Map.Felucca,
			2 or 3 => Map.Ilshenar,
			4 or 5 => Map.Malas,
			6 or 7 => Map.Tokuno,
			_ => Map.Trammel,
		};
	}

	private static Point2D GetRandomLocation(Map map, bool eodon = false)
	{
		Rectangle2D[] recs;

		if (map == Map.Trammel || map == Map.Felucca)
			recs = m_FelTramWrap;
		else if (map == Map.Tokuno)
			recs = m_TokunoWrap;
		else if (map == Map.Malas)
			recs = m_MalasWrap;
		else if (map == Map.Ilshenar)
			recs = m_IlshenarWrap;
		else if (eodon)
			recs = m_EodonWrap;
		else
			recs = m_TerMurWrap;

		while (true)
		{
			Rectangle2D rec = recs[Utility.Random(recs.Length)];

			var x = Utility.Random(rec.X, rec.Width);
			var y = Utility.Random(rec.Y, rec.Height);

			if (ValidateLocation(x, y, map))
				return new Point2D(x, y);
		}
	}

	private static bool ValidateLocation(int x, int y, Map map)
	{
		LandTile lt = map.Tiles.GetLandTile(x, y);
		LandData ld = TileData.LandTable[lt.Id];

		//Checks for impassable flag..cant walk, cant have a chest
		if (lt.Ignored || (ld.Flags & TileFlag.Impassable) > 0)
		{
			return false;
		}

		//Checks for roads
		for (int i = 0; i < HousePlacement.m_RoadIDs.Length; i += 2)
		{
			if (lt.Id >= HousePlacement.m_RoadIDs[i] && lt.Id <= HousePlacement.m_RoadIDs[i + 1])
			{
				return false;
			}
		}

		Region reg = Region.Find(new Point3D(x, y, lt.Z), map);

		//no-go in towns, houses, dungeons and champspawns
		if (reg != null)
		{
			if (reg.IsPartOf<TownRegion>() || reg.IsPartOf<DungeonRegion>() ||
				reg.IsPartOf<ChampionSpawnRegion>() || reg.IsPartOf<HouseRegion>())
			{
				return false;
			}
		}

		string n = (ld.Name ?? string.Empty).ToLower();

		if (n != "dirt" && n != "grass" && n != "jungle" && n != "forest" && n != "snow")
		{
			return false;
		}

		//Rare occurances where a static tile needs to be checked
		foreach (StaticTile tile in map.Tiles.GetStaticTiles(x, y, true))
		{
			ItemData td = TileData.ItemTable[tile.Id & TileData.MaxItemValue];

			if ((td.Flags & TileFlag.Impassable) > 0)
			{
				return false;
			}

			n = (td.Name ?? string.Empty).ToLower();

			if (n != "dirt" && n != "grass" && n != "jungle" && n != "forest" && n != "snow")
			{
				return false;
			}
		}

		//check for house within 5 tiles
		for (int xx = x - 5; xx <= x + 5; xx++)
		{
			for (int yy = y - 5; yy <= y + 5; yy++)
			{
				if (BaseHouse.FindHouseAt(new Point3D(xx, yy, lt.Z), map, Region.MaxZ - lt.Z) != null)
				{
					return false;
				}
			}
		}

		return true;
	}

	private static void GetWidthAndHeight(Map map, out int width, out int height)
	{
		if (map == Map.Trammel || map == Map.Felucca)
		{
			width = 600;
			height = 600;
		}
		if (map == Map.TerMur)
		{
			width = 200;
			height = 200;
		}
		else
		{
			width = 300;
			height = 300;
		}
	}


	public static void AdjustMap(Map map, out int x2, out int y2, int x1, int y1, int width, int height)
	{
		AdjustMap(map, out x2, out y2, x1, y1, width, height, false);
	}

	private static void AdjustMap(Map map, out int x2, out int y2, int x1, int y1, int width, int height, bool eodon)
	{
		x2 = x1 + width;
		y2 = y1 + height;

		if (map == Map.Trammel || map == Map.Felucca)
		{
			if (x2 >= 5120)
				x2 = 5119;

			if (y2 >= 4096)
				y2 = 4095;
		}
		else if (map == Map.Ilshenar)
		{
			if (x2 >= 1890)
				x2 = 1889;

			if (x2 <= 120)
				x2 = 121;

			if (y2 >= 1465)
				y2 = 1464;

			if (y2 <= 105)
				y2 = 106;
		}
		else if (map == Map.Malas)
		{
			if (x2 >= 2522)
				x2 = 2521;

			if (x2 <= 515)
				x2 = 516;

			if (y2 >= 1990)
				y2 = 1989;

			if (y2 <= 0)
				y2 = 1;
		}
		else if (map == Map.Tokuno)
		{
			if (x2 >= 1428)
				x2 = 1427;

			if (x2 <= 0)
				x2 = 1;

			if (y2 >= 1420)
				y2 = 1419;

			if (y2 <= 0)
				y2 = 1;
		}
		else if (map == Map.TerMur)
		{
			if (eodon)
			{
				if (x2 <= 62)
					x2 = 63;

				if (x2 >= 960)
					x2 = 959;

				if (y2 <= 1343)
					y2 = 1344;

				if (y2 >= 2240)
					y2 = 2239;
			}
			else
			{
				if (x2 >= 1271)
					x2 = 1270;

				if (x2 <= 260)
					x2 = 261;

				if (y2 >= 4094)
					y2 = 4083;

				if (y2 <= 2760)
					y2 = 2761;
			}
		}
	}

	public virtual void OnMapComplete()
	{
	}

	public virtual void OnChestOpened()
	{
	}

	private static BaseCreature Spawn(int level, Point3D p, bool guardian, Map map)
	{
		Type[][] spawns;

		if (map == Map.Trammel || map == Map.Felucca)
			spawns = m_SpawnTypes;
		else if (map == Map.Tokuno)
			spawns = m_TokunoSpawnTypes;
		else if (map == Map.Ilshenar)
			spawns = m_IlshenarSpawnTypes;
		else if (map == Map.Malas)
			spawns = m_MalasSpawnTypes;
		else
		{
			spawns = SpellHelper.IsEodon(map, p) ? m_EodonSpawnTypes : m_TerMurSpawnTypes;
		}

		if (level < 0 || level >= spawns.Length)
			return null;

		BaseCreature bc;
		Type[] list = GetSpawnList(spawns, level);

		try
		{
			bc = (BaseCreature)Activator.CreateInstance(list[Utility.Random(list.Length)]);
		}
		catch
		{
			return null;
		}

		if (bc == null)
			return null;

		bc.Home = p;
		bc.RangeHome = 5;

		if (!guardian)
			return bc;

		bc.Title = "(Guardian)";

		if (!TreasureMapInfo.NewSystem && level == 0)
		{
			bc.Name = "a chest guardian";
			bc.Hue = 0x835;
		}

		if (BaseCreature.IsSoulboundEnemies && !bc.Tamable)
		{
			bc.IsSoulBound = true;
		}

		return bc;

	}

	public static BaseCreature Spawn(int level, Point3D p, Map map, Mobile target, bool guardian)
	{
		if (map == null)
			return null;

		BaseCreature c = Spawn(level, p, guardian, map);

		if (c != null)
		{
			bool spawned = false;

			for (int i = 0; !spawned && i < 10; ++i)
			{
				int x = p.X - 3 + Utility.Random(7);
				int y = p.Y - 3 + Utility.Random(7);

				if (map.CanSpawnMobile(x, y, p.Z))
				{
					c.MoveToWorld(new Point3D(x, y, p.Z), map);
					spawned = true;
				}
				else
				{
					int z = map.GetAverageZ(x, y);

					if (map.CanSpawnMobile(x, y, z))
					{
						c.MoveToWorld(new Point3D(x, y, z), map);
						spawned = true;
					}
				}
			}

			if (!spawned)
			{
				c.Delete();
				return null;
			}

			if (target != null)
				c.Combatant = target;

			return c;
		}

		return null;
	}

	private static Type[] GetSpawnList(Type[][] table, int level)
	{
		Type[] array;

		if (TreasureMapInfo.NewSystem)
		{
			switch (level)
			{
				default: array = table[level + 1]; break;
				case 2:
					List<Type> list1 = new();
					list1.AddRange(table[2]);
					list1.AddRange(table[3]);

					array = list1.ToArray();
					break;
				case 3:
					List<Type> list2 = new();
					list2.AddRange(table[4]);
					list2.AddRange(table[5]);

					array = list2.ToArray();
					break;
				case 4: array = table[6]; break;
				case 5: array = table[7]; break;
			}
		}
		else
		{
			array = table[level];
		}

		return array;
	}

	public TreasureMap()
	{
	}

	[Constructable]
	public TreasureMap(int level, Map map, bool eodon = false)
	{
		Level = level;
		bool newSystem = TreasureMapInfo.NewSystem;

		if (newSystem)
		{
			AssignRandomPackage();
		}

		if ((!newSystem && level == 7) || map == Map.Internal)
			map = GetRandomMap();

		Facet = map;

		ChestLocation = GetRandomLocation(map, eodon);

		Width = 300;
		Height = 300;

		GetWidthAndHeight(map, out var width, out var height);

		int x1 = ChestLocation.X - Utility.RandomMinMax(width / 4, width / 4 * 3);
		int y1 = ChestLocation.Y - Utility.RandomMinMax(height / 4, height / 4 * 3);

		if (x1 < 0)
			x1 = 0;

		if (y1 < 0)
			y1 = 0;

		AdjustMap(map, out var x2, out var y2, x1, y1, width, height, eodon);

		x1 = x2 - width;
		y1 = y2 - height;

		Bounds = new Rectangle2D(x1, y1, width, height);
		Protected = true;

		AddWorldPin(m_Location.X, m_Location.Y);

		NextReset = DateTime.UtcNow + ResetTime;
	}

	public TreasureMap(Serial serial) : base(serial)
	{
	}

	private static bool HasDiggingTool(Mobile m)
	{
		if (m.Backpack == null)
			return false;

		List<BaseHarvestTool> items = m.Backpack.FindItemsByType<BaseHarvestTool>();

		return items.Any(tool => tool.HarvestSystem == Engines.Harvest.Mining.System);
	}

	public void OnBeginDig(Mobile from)
	{
		if (m_Completed)
		{
			from.SendLocalizedMessage(503028); // The treasure for this map has already been found.
		}
		else if (m_Level == 0 && !CheckYoung(from))
		{
			from.SendLocalizedMessage(1046447); // Only a young player may use this treasure map.
		}
		/*
		else if ( from != m_Decoder )
		{
			from.SendLocalizedMessage( 503016 ); // Only the person who decoded this map may actually dig up the treasure.
		}
		*/
		else if (m_Decoder != from && !HasRequiredSkill(from))
		{
			from.SendLocalizedMessage(503031); // You did not decode this map and have no clue where to look for the treasure.
		}
		else if (!from.CanBeginAction(typeof(TreasureMap)))
		{
			from.SendLocalizedMessage(503020); // You are already digging treasure.
		}
		else if (from.Map != m_Map)
		{
			from.SendLocalizedMessage(1010479); // You seem to be in the right place, but may be on the wrong facet!
		}
		else
		{
			from.SendLocalizedMessage(503033); // Where do you wish to dig?
			from.Target = new DigTarget(this);
		}
	}

	private class DigTarget : Target
	{
		private readonly TreasureMap m_Map;

		public DigTarget(TreasureMap map) : base(6, true, TargetFlags.None)
		{
			m_Map = map;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Map.Deleted)
				return;

			Map map = m_Map.m_Map;

			if (m_Map.m_Completed)
			{
				from.SendLocalizedMessage(503028); // The treasure for this map has already been found.
			}
			/*
			else if ( from != m_Map.m_Decoder )
			{
				from.SendLocalizedMessage( 503016 ); // Only the person who decoded this map may actually dig up the treasure.
			}
			*/
			else if (m_Map.m_Decoder != from && !m_Map.HasRequiredSkill(from))
			{
				from.SendLocalizedMessage(503031); // You did not decode this map and have no clue where to look for the treasure.
			}
			else if (!from.CanBeginAction(typeof(TreasureMap)))
			{
				from.SendLocalizedMessage(503020); // You are already digging treasure.
			}
			else if (!HasDiggingTool(from))
			{
				from.SendMessage("You must have a digging tool to dig for treasure.");
			}
			else if (from.Map != map)
			{
				from.SendLocalizedMessage(1010479); // You seem to be in the right place, but may be on the wrong facet!
			}
			else
			{
				IPoint3D p = targeted as IPoint3D;

				Point3D targ3D;
				if (p is Item item)
					targ3D = item.GetWorldLocation();
				else
					targ3D = new Point3D(p);

				double skillValue = from.Skills[SkillName.Mining].Value;

				var maxRange = skillValue switch
				{
					>= 100.0 => 4,
					>= 81.0 => 3,
					>= 51.0 => 2,
					_ => 1
				};

				Point2D loc = m_Map.m_Location;
				int x = loc.X, y = loc.Y;

				Point3D chest3D0 = new(loc, 0);

				if (Utility.InRange(targ3D, chest3D0, maxRange))
				{
					if (from.Location.X == x && from.Location.Y == y)
					{
						from.SendLocalizedMessage(503030); // The chest can't be dug up because you are standing on top of it.
					}
					else if (map != null)
					{
						int z = map.GetAverageZ(x, y);

						if (!map.CanFit(x, y, z, 16, true, true))
						{
							from.SendLocalizedMessage(503021); // You have found the treasure chest but something is keeping it from being dug up.
						}
						else if (from.BeginAction(typeof(TreasureMap)))
						{
							new DigTimer(from, m_Map, new Point3D(x, y, z), map).Start();
						}
						else
						{
							from.SendLocalizedMessage(503020); // You are already digging treasure.
						}
					}
				}
				else if (m_Map.Level > 0)
				{
					// We're close, but not quite// You dig and dig but no treasure seems to be here.// You dig and dig but fail to find any treasure.
					from.SendLocalizedMessage(Utility.InRange(targ3D, chest3D0, 8) ? 503032 : 503035);
				}
				else
				{
					if (Utility.InRange(targ3D, chest3D0, 8)) // We're close, but not quite
					{
						from.SendAsciiMessage(0x44, "The treasure chest is very close!");
					}
					else
					{
						Direction dir = Utility.GetDirection(targ3D, chest3D0);
						string sDir = dir switch
						{
							Direction.North => "north",
							Direction.Right => "northeast",
							Direction.East => "east",
							Direction.Down => "southeast",
							Direction.South => "south",
							Direction.Left => "southwest",
							Direction.West => "west",
							_ => "northwest",
						};
						from.SendAsciiMessage(0x44, "Try looking for the treasure chest more to the {0}.", sDir);
					}
				}
			}
		}
	}

	private class DigTimer : Timer
	{
		private readonly Mobile m_From;
		private readonly TreasureMap m_TreasureMap;

		private Point3D m_Location;
		private readonly Map m_Map;

		private TreasureChestDirt m_Dirt1;
		private TreasureChestDirt m_Dirt2;
		private TreasureMapChest m_Chest;

		private int m_Count;

		private readonly long m_NextSkillTime;
		private readonly long m_NextSpellTime;
		private readonly long m_NextActionTime;
		private readonly long m_LastMoveTime;

		public DigTimer(Mobile from, TreasureMap treasureMap, Point3D location, Map map) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
		{
			m_From = from;
			m_TreasureMap = treasureMap;

			m_Location = location;
			m_Map = map;

			m_NextSkillTime = from.NextSkillTime;
			m_NextSpellTime = from.NextSpellTime;
			m_NextActionTime = from.NextActionTime;
			m_LastMoveTime = from.LastMoveTime;

			Priority = TimerPriority.TenMs;
		}

		private void Terminate()
		{
			Stop();
			m_From.EndAction(typeof(TreasureMap));

			m_Chest?.Delete();

			if (m_Dirt1 == null)
				return;

			m_Dirt1.Delete();
			m_Dirt2.Delete();
		}

		protected override void OnTick()
		{
			if (m_NextSkillTime != m_From.NextSkillTime || m_NextSpellTime != m_From.NextSpellTime ||
			    m_NextActionTime != m_From.NextActionTime)
			{
				Terminate();
				return;
			}

			if (m_LastMoveTime != m_From.LastMoveTime)
			{
				m_From.SendLocalizedMessage(
					503023); // You cannot move around while digging up treasure. You will need to start digging anew.
				Terminate();
				return;
			}

			int z = m_Chest != null ? m_Chest.Z + m_Chest.ItemData.Height : int.MinValue;
			int height = 16;

			if (z > m_Location.Z)
				height -= z - m_Location.Z;
			else
				z = m_Location.Z;

			if (!m_Map.CanFit(m_Location.X, m_Location.Y, z, height, true, true, false))
			{
				m_From.SendLocalizedMessage(
					503024); // You stop digging because something is directly on top of the treasure chest.
				Terminate();
				return;
			}

			m_Count++;

			m_From.RevealingAction();
			m_From.Direction = m_From.GetDirectionTo(m_Location);

			if (m_Count > 1 && m_Dirt1 == null)
			{
				m_Dirt1 = new TreasureChestDirt();
				m_Dirt1.MoveToWorld(m_Location, m_Map);

				m_Dirt2 = new TreasureChestDirt();
				m_Dirt2.MoveToWorld(new Point3D(m_Location.X, m_Location.Y - 1, m_Location.Z), m_Map);
			}

			if (m_Count == 5)
			{
				m_Dirt1.Turn1();
			}
			else if (m_Count == 10)
			{
				m_Dirt1.Turn2();
				m_Dirt2.Turn2();
			}
			else if (m_Count > 10)
			{
				if (m_Chest == null)
				{
					m_Chest = new TreasureMapChest(m_From, m_TreasureMap.Level, true);
					if (TreasureMapInfo.NewSystem)
					{
						m_TreasureMap.AssignChestQuality(m_From, m_Chest);
					}

					m_Chest.MoveToWorld(new Point3D(m_Location.X, m_Location.Y, m_Location.Z - 15), m_Map);
				}
				else
				{
					m_Chest.Z++;
				}

				Effects.PlaySound(m_Chest, m_Map, 0x33B);
			}

			if (m_Chest != null && m_Chest.Location.Z >= m_Location.Z)
			{
				Stop();
				m_From.EndAction(typeof(TreasureMap));

				m_Chest.Temporary = false;
				m_Chest.TreasureMap = m_TreasureMap;
				m_Chest.DigTime = DateTime.UtcNow;
				m_TreasureMap.Completed = true;
				m_TreasureMap.CompletedBy = m_From;


				if (TreasureMapInfo.NewSystem)
				{
					TreasureMapInfo.Fill(m_From, m_Chest, m_TreasureMap);
				}
				else
				{
					LootHelpers.Fill(m_From, m_Chest, m_Chest.Level, false);
				}

				m_TreasureMap.OnMapComplete();

				int spawns = m_TreasureMap.Level switch
				{
					0 => TreasureMapInfo.NewSystem ? 4 : 3,
					1 => TreasureMapInfo.NewSystem ? 4 : 0,
					_ => 4
				};

				for (int i = 0; i < spawns; ++i)
				{
					bool guardian = !TreasureMapInfo.NewSystem || Utility.RandomDouble() >= 0.3;

					BaseCreature bc = Spawn(m_TreasureMap.Level, m_Chest.Location, m_Chest.Map, null, guardian);

					if (bc == null || !guardian)
						continue;

					bc.Hue = 2725;
					m_Chest.Guardians.Add(bc);
				}
			}
			else
			{
				if (m_From.Body.IsHuman && !m_From.Mounted)
				{
					m_From.Animate(AnimationType.Attack, 3);
				}

				new SoundTimer(m_From, 0x125 + m_Count % 2).Start();
			}
		}

		private class SoundTimer : Timer
		{
			private readonly Mobile m_From;
			private readonly int m_SoundId;

			public SoundTimer(Mobile from, int soundId) : base(TimeSpan.FromSeconds(0.9))
			{
				m_From = from;
				m_SoundId = soundId;

				Priority = TimerPriority.TenMs;
			}

			protected override void OnTick()
			{
				m_From.PlaySound(m_SoundId);
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			return;
		}

		if (!m_Completed && m_Decoder == null)
			Decode(from);
		else
			DisplayTo(from);
	}

	private bool CheckYoung(Mobile from)
	{
		if (from.IsStaff())
			return true;

		if (from is PlayerMobile { Young: true })
			return true;

		if (from == Decoder)
		{
			Level = 1;
			from.SendLocalizedMessage(1046446); // This is now a level one treasure map.
			return true;
		}

		return false;
	}

	private double GetMinSkillLevel()
	{
		return m_Level switch
		{
			0 => Core.TOL ? 27 : 0.0,
			1 => Core.TOL ? 70 : -3.0,
			2 => Core.TOL ? 90 : 41.0,
			3 => Core.TOL ? 100.0 : 51.0,
			4 => Core.TOL ? 100.0 : 61.0,
			5 or 6 => Core.TOL ? 100.0 : 70.0,
			_ => 0.0,
		};
	}

	private bool HasRequiredSkill(Mobile from)
	{
		return from.Skills[SkillName.Cartography].Value >= GetMinSkillLevel();
	}

	private void Decode(Mobile from)
	{
		if (m_Completed || m_Decoder != null)
			return;

		if (m_Level == 0)
		{
			if (!CheckYoung(from))
			{
				from.SendLocalizedMessage(1046447); // Only a young player may use this treasure map.
				return;
			}
		}
		else
		{
			double minSkill = GetMinSkillLevel();

			if (from.Skills[SkillName.Cartography].Value < minSkill)
				from.SendLocalizedMessage(503013); // The map is too difficult to attempt to decode.

			double maxSkill = minSkill + 60.0;

			if (!from.CheckSkill(SkillName.Cartography, minSkill, maxSkill))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503018); // You fail to make anything of the map.
				return;
			}
		}

		from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503019); // You successfully decode a treasure map!
		Decoder = from;

		if (Core.AOS)
			LootType = LootType.Blessed;

		DisplayTo(from);
	}

	private void ResetLocation()
	{
		if (m_Completed)
			return;

		ClearPins();
		LootType = LootType.Regular;
		m_Decoder = null;
		GetRandomLocation(Facet, TreasureMapInfo.NewSystem && TreasureFacet == TreasureFacet.Eodon);
		InvalidateProperties();
		NextReset = DateTime.UtcNow + ResetTime;
	}

	public override void DisplayTo(Mobile from)
	{
		if (m_Completed)
		{
			SendLocalizedMessageTo(from, 503014); // This treasure hunt has already been completed.
		}
		else if (m_Level == 0 && !CheckYoung(from))
		{
			from.SendLocalizedMessage(1046447); // Only a young player may use this treasure map.
			return;
		}
		else if (m_Decoder != from && !HasRequiredSkill(from))
		{
			from.SendLocalizedMessage(503031); // You did not decode this map and have no clue where to look for the treasure.
			return;
		}
		else
		{
			SendLocalizedMessageTo(from, 503017); // The treasure is marked by the red pin. Grab a shovel and go dig it up!
		}

		from.PlaySound(0x249);
		base.DisplayTo(from);
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (!m_Completed)
		{
			if (m_Decoder == null)
			{
				list.Add(new DecodeMapEntry(this));
			}
			else
			{
				bool digTool = HasDiggingTool(from);

				list.Add(new OpenMapEntry(this));
				list.Add(new DigEntry(this, digTool));
			}
		}
	}

	private class DecodeMapEntry : ContextMenuEntry
	{
		private readonly TreasureMap m_Map;

		public DecodeMapEntry(TreasureMap map) : base(6147, 2)
		{
			m_Map = map;
		}

		public override void OnClick()
		{
			if (!m_Map.Deleted)
				m_Map.Decode(Owner.From);
		}
	}

	private class OpenMapEntry : ContextMenuEntry
	{
		private readonly TreasureMap m_Map;

		public OpenMapEntry(TreasureMap map) : base(6150, 2)
		{
			m_Map = map;
		}

		public override void OnClick()
		{
			if (!m_Map.Deleted)
				m_Map.DisplayTo(Owner.From);
		}
	}

	private class DigEntry : ContextMenuEntry
	{
		private readonly TreasureMap m_Map;

		public DigEntry(TreasureMap map, bool enabled) : base(6148, 2)
		{
			m_Map = map;

			if (!enabled)
				Flags |= CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			if (m_Map.Deleted)
				return;

			Mobile from = Owner.From;

			if (HasDiggingTool(from))
				m_Map.OnBeginDig(from);
			else
				from.SendMessage("You must have a digging tool to dig for treasure.");
		}
	}

	public override int LabelNumber
	{
		get
		{
			if (m_Decoder != null)
			{
				return m_Level switch
				{
					6 => 1063453,
					7 => 1116773,
					_ => 1041516 + m_Level
				};
			}

			return m_Level switch
			{
				6 => 1063452,
				7 => 1116790,
				_ => 1041510 + m_Level
			};
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(m_Map == Map.Felucca ? 1041502 : 1041503); // for somewhere in Felucca : for somewhere in Trammel

		if (m_Completed)
		{
			list.Add(1041507, m_CompletedBy == null ? "someone" : m_CompletedBy.Name); // completed by ~1_val~
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		if (m_Completed)
		{
			from.Send(new MessageLocalizedAffix(Serial, ItemId, MessageType.Label, 0x3B2, 3, 1048030, "", AffixType.Append,
				$" completed by {(m_CompletedBy == null ? "someone" : m_CompletedBy.Name)}", ""));
		}
		else if (m_Decoder != null)
		{
			if (m_Level == 6)
				LabelTo(from, 1063453);
			else
				LabelTo(from, 1041516 + m_Level);
		}
		else
		{
			LabelTo(from, 1041522,
				m_Level == 6
					? $"#{1063452}\t \t#{(m_Map == Map.Felucca ? 1041502 : 1041503)}"
					: $"#{1041510 + m_Level}\t \t#{(m_Map == Map.Felucca ? 1041502 : 1041503)}");
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)Package);
		writer.Write(NextReset);
		writer.Write(m_CompletedBy);
		writer.Write(m_Level);
		writer.Write(m_Completed);
		writer.Write(m_Decoder);
		writer.Write(ChestLocation);

		if (!Completed && NextReset != DateTime.MinValue && NextReset < DateTime.UtcNow)
			Timer.DelayCall(TimeSpan.FromSeconds(30), ResetLocation);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				{
					Package = (TreasurePackage)reader.ReadInt();
					NextReset = reader.ReadDateTime();
					m_CompletedBy = reader.ReadMobile();
					m_Level = reader.ReadInt();
					m_Completed = reader.ReadBool();
					m_Decoder = reader.ReadMobile();
					ChestLocation = reader.ReadPoint2D();
					break;
				}
		}

		if (Core.AOS && m_Decoder != null && LootType == LootType.Regular)
			LootType = LootType.Blessed;

		if (NextReset == DateTime.MinValue)
		{
			NextReset = DateTime.UtcNow + ResetTime;
		}
	}
}

public class TreasureChestDirt : BaseItem
{
	public TreasureChestDirt() : base(0x912)
	{
		Movable = false;
		Timer.DelayCall(TimeSpan.FromMinutes(2.0), Delete);
	}

	public TreasureChestDirt(Serial serial) : base(serial)
	{
	}

	public void Turn1()
	{
		ItemId = 0x913;
	}

	public void Turn2()
	{
		ItemId = 0x914;
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
		Delete();
	}
}
