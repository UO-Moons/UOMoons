using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Champions
{
	public class ChampionSpawn : BaseItem
	{
		public static readonly int MaxStrayDistance = 250;

		private bool m_Active;
		private ChampionSpawnType m_Type;
		private List<Item> m_RedSkulls;
		private List<Item> m_WhiteSkulls;
		private ChampionPlatform m_Platform;
		private ChampionAltar m_Altar;
		private int m_Kills;

		//private int m_SpawnRange;
		private Rectangle2D m_SpawnArea;
		private ChampionSpawnRegion m_Region;

		private TimeSpan m_ExpireDelay;
		private Timer m_Timer, m_RestartTimer;

		private IdolOfTheChampion m_Idol;
		private Dictionary<Mobile, int> m_DamageEntries;

		public List<Mobile> Creatures { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string GroupName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public double SpawnMod { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SpawnRadius { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public double KillsMod { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AutoRestart { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string SpawnName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ConfinedRoaming { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasBeenAdvanced { get; set; }

		[Constructable]
		public ChampionSpawn()
			: base(0xBD2)
		{
			Movable = false;
			Visible = false;

			Creatures = new List<Mobile>();
			m_RedSkulls = new List<Item>();
			m_WhiteSkulls = new List<Item>();

			m_Platform = new ChampionPlatform(this);
			m_Altar = new ChampionAltar(this);
			m_Idol = new IdolOfTheChampion(this);

			m_ExpireDelay = TimeSpan.FromMinutes(10.0);
			RestartDelay = TimeSpan.FromMinutes(10.0);

			m_DamageEntries = new Dictionary<Mobile, int>();
			RandomizeType = false;

			SpawnRadius = 35;
			SpawnMod = 1;

			Timer.DelayCall(TimeSpan.Zero, SetInitialSpawnArea);
		}

		public void SetInitialSpawnArea()
		{
			//Previous default used to be 24;
			SpawnArea = new Rectangle2D(new Point2D(X - SpawnRadius, Y - SpawnRadius),
				new Point2D(X + SpawnRadius, Y + SpawnRadius));
		}

		public void UpdateRegion()
		{
			m_Region?.Unregister();

			if (Deleted || Map == Map.Internal)
				return;

			m_Region = new ChampionSpawnRegion(this);
			m_Region.Register();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RandomizeType { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Kills
		{
			get => m_Kills;
			set
			{
				m_Kills = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Rectangle2D SpawnArea
		{
			get => m_SpawnArea;
			set
			{
				m_SpawnArea = value;
				InvalidateProperties();
				UpdateRegion();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan RestartDelay { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime RestartTime { get; private set; }

		//[CommandProperty(AccessLevel.GameMaster)]
		//public TimeSpan ExpireDelay
		//{
		//	get => m_ExpireDelay;
		//	set => m_ExpireDelay = value;
		//}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime ExpireTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionSpawnType Type
		{
			get => m_Type;
			set
			{
				m_Type = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get => m_Active;
			set
			{
				if (value)
					Start();
				else
					Stop();

				//PrimevalLichPuzzle.Update(this);

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Champion { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Level
		{
			get => m_RedSkulls.Count;
			set
			{
				for (var i = m_RedSkulls.Count - 1; i >= value; --i)
				{
					m_RedSkulls[i].Delete();
					m_RedSkulls.RemoveAt(i);
				}

				for (var i = m_RedSkulls.Count; i < value; ++i)
				{
					Item skull = new(0x1854)
					{
						Hue = 0x26,
						Movable = false,
						Light = LightType.Circle150
					};

					skull.MoveToWorld(GetRedSkullLocation(i), Map);

					m_RedSkulls.Add(skull);
				}

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StartLevel { get; private set; }

		private void RemoveSkulls()
		{
			if (m_WhiteSkulls != null)
			{
				for (var i = 0; i < m_WhiteSkulls.Count; ++i)
					m_WhiteSkulls[i].Delete();

				m_WhiteSkulls.Clear();
			}

			if (m_RedSkulls != null)
			{
				for (var i = 0; i < m_RedSkulls.Count; i++)
					m_RedSkulls[i].Delete();

				m_RedSkulls.Clear();
			}
		}

		public int MaxKills
		{
			get
			{
				var l = Level;
				return ChampionSystem.MaxKillsForLevel(l);
			}
		}

		public bool IsChampionSpawn(Mobile m)
		{
			return Creatures.Contains(m);
		}

		public void SetWhiteSkullCount(int val)
		{
			for (var i = m_WhiteSkulls.Count - 1; i >= val; --i)
			{
				m_WhiteSkulls[i].Delete();
				m_WhiteSkulls.RemoveAt(i);
			}

			for (var i = m_WhiteSkulls.Count; i < val; ++i)
			{
				Item skull = new(0x1854)
				{
					Movable = false,
					Light = LightType.Circle150
				};

				skull.MoveToWorld(GetWhiteSkullLocation(i), Map);

				m_WhiteSkulls.Add(skull);

				Effects.PlaySound(skull.Location, skull.Map, 0x29);
				Effects.SendLocationEffect(new Point3D(skull.X + 1, skull.Y + 1, skull.Z), skull.Map, 0x3728, 10);
			}
		}

		public void Start(bool serverLoad = false)
		{
			if (m_Active || Deleted)
				return;

			m_Active = true;
			HasBeenAdvanced = false;

			m_Timer?.Stop();

			m_Timer = new SliceTimer(this);
			m_Timer.Start();

			m_RestartTimer?.Stop();

			m_RestartTimer = null;

			if (m_Altar != null)
				m_Altar.Hue = 0;

			//PrimevalLichPuzzle.Update(this);

			if (serverLoad)
				return;

			var chance = Utility.RandomDouble();

			switch (chance)
			{
				case < 0.1:
					Level = 4;
					break;
				case < 0.25:
					Level = 3;
					break;
				case < 0.5:
					Level = 2;
					break;
				default:
				{
					if (Utility.RandomBool())
						Level = 1;
					break;
				}
			}

			StartLevel = Level;

			if (Level > 0 && m_Altar != null)
			{
				Effects.PlaySound(m_Altar.Location, m_Altar.Map, 0x29);
				Effects.SendLocationEffect(new Point3D(m_Altar.X + 1, m_Altar.Y + 1, m_Altar.Z), m_Altar.Map, 0x3728, 10);
			}
		}

		public void Stop()
		{
			if (!m_Active || Deleted)
				return;

			m_Active = false;
			HasBeenAdvanced = false;

			// We must despawn all the creatures.
			if (Creatures != null)
			{
				for (var i = 0; i < Creatures.Count; ++i)
					Creatures[i].Delete();

				Creatures.Clear();
			}

			m_Timer?.Stop();

			m_Timer = null;

			m_RestartTimer?.Stop();

			m_RestartTimer = null;

			if (m_Altar != null)
				m_Altar.Hue = 0x455;

			//PrimevalLichPuzzle.Update(this);

			RemoveSkulls();
			m_Kills = 0;
		}

		public void BeginRestart(TimeSpan ts)
		{
			m_RestartTimer?.Stop();

			RestartTime = DateTime.UtcNow + ts;

			m_RestartTimer = new RestartTimer(this, ts);
			m_RestartTimer.Start();
		}

		public void EndRestart()
		{
			if (RandomizeType)
			{
				Type = Utility.Random(5) switch
				{
					0 => ChampionSpawnType.Abyss,
					1 => ChampionSpawnType.Arachnid,
					2 => ChampionSpawnType.ColdBlood,
					3 => ChampionSpawnType.VerminHorde,
					4 => ChampionSpawnType.UnholyTerror,
					_ => Type
				};
			}

			HasBeenAdvanced = false;

			Start();
		}

		#region Scroll of Transcendence
		public static ScrollOfTranscendence CreateRandomSoT(bool felucca)
		{
			var level = Utility.RandomMinMax(1, 5);

			if (felucca)
				level += 5;

			return ScrollOfTranscendence.CreateRandom(level, level);
		}

		#endregion

		public static void GiveScrollTo(Mobile killer, SpecialScroll scroll)
		{
			if (scroll == null || killer == null)   //sanity
				return;

			killer.SendLocalizedMessage(scroll is ScrollOfTranscendence ? 1094936 : 1049524);// You have received a Scroll of Transcendence!// You have received a scroll of power!

			if (killer.Alive)
				killer.AddToBackpack(scroll);
			else
			{
				if (killer.Corpse is {Deleted: false})
					killer.Corpse.DropItem(scroll);
				else
					killer.AddToBackpack(scroll);
			}

			// Justice reward
			var pm = (PlayerMobile)killer;
			for (var j = 0; j < pm.JusticeProtectors.Count; ++j)
			{
				var prot = pm.JusticeProtectors[j] ?? throw new ArgumentNullException(nameof(killer));

				if (prot.Map != killer.Map || prot.Murderer || prot.Criminal || !JusticeVirtue.CheckMapRegion(killer, prot))
					continue;

				var chance = VirtueHelper.GetLevel(prot, VirtueName.Justice) switch
				{
					VirtueLevel.Seeker => 60,
					VirtueLevel.Follower => 80,
					VirtueLevel.Knight => 100,
					_ => 0
				};

				if (chance > Utility.Random(100))
				{
					try
					{
						prot.SendLocalizedMessage(1049368); // You have been rewarded for your dedication to Justice!

						if (Activator.CreateInstance(scroll.GetType()) is SpecialScroll scrollDupe)
						{
							scrollDupe.Skill = scroll.Skill;
							scrollDupe.Value = scroll.Value;
							prot.AddToBackpack(scrollDupe);
						}
					}
					catch (Exception)
					{
						// ignored
					}
				}
			}
		}

		private DateTime _NextGhostCheck;

		public void OnSlice()
		{
			if (!m_Active || Deleted)
				return;

			int currentRank = Rank;

			if (Champion != null)
			{
				if (Champion.Deleted)
				{
					RegisterDamageTo(Champion);

					if (Champion is BaseChampion champion)
						AwardArtifact(champion.GetArtifact());

					m_DamageEntries.Clear();

					if (m_Altar != null)
					{
						m_Altar.Hue = 0x455;

						if (!Core.ML || Map == Map.Felucca)
						{
							_ = new StarRoomGate(true, m_Altar.Location, m_Altar.Map);
						}
					}

					Champion = null;
					Stop();

					if (AutoRestart)
						BeginRestart(RestartDelay);
				}
				else if (Champion.Alive && Champion.GetDistanceToSqrt(this) > MaxStrayDistance)
				{
					Champion.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
				}
			}
			else
			{
				var kills = m_Kills;

				for (var i = 0; i < Creatures.Count; ++i)
				{
					var m = Creatures[i];

					if (!m.Deleted)
						continue;

					if (m.Corpse is {Deleted: false})
					{
						((Corpse)m.Corpse).BeginDecay(TimeSpan.FromMinutes(1));
					}
					Creatures.RemoveAt(i);
					--i;

					var rankOfMob = GetRankFor(m);
					if (rankOfMob == currentRank)
						++m_Kills;

					var killer = m.FindMostRecentDamager(false);

					RegisterDamageTo(m);

					if (killer is BaseCreature creature)
						killer = creature.GetMaster();

					if (killer is not PlayerMobile mobile)
						continue;

					#region Scroll of Transcendence
					if (Core.ML)
					{
						if (Map == Map.Felucca)
						{
							if (Utility.RandomDouble() < ChampionSystem.ScrollChance)
							{
								if (Utility.RandomDouble() < ChampionSystem.TranscendenceChance)
								{
									var soTf = CreateRandomSoT(true) ?? throw new ArgumentNullException($@"CreateRandomSoT(true)");
									GiveScrollTo(mobile, soTf);
								}
								else
								{
									var ps = PowerScroll.CreateRandomNoCraft(5, 5);
									GiveScrollTo(mobile, ps);
								}
							}
						}

						if (Map == Map.Ilshenar || Map == Map.Tokuno || Map == Map.Malas)
						{
							if (Utility.RandomDouble() < 0.0015)
							{
								killer.SendLocalizedMessage(1094936); // You have received a Scroll of Transcendence!
								var soTt = CreateRandomSoT(false);
								killer.AddToBackpack(soTt);
							}
						}
					}
					#endregion

					var mobSubLevel = rankOfMob + 1;
					if (mobSubLevel < 0)
						continue;

					var gainedPath = false;

					var pointsToGain = mobSubLevel * 40;

					if (VirtueHelper.Award(killer, VirtueName.Valor, pointsToGain, ref gainedPath))
					{
						killer.SendLocalizedMessage(gainedPath ? 1054032 : 1054030);
						//No delay on Valor gains
					}

					var info = mobile.ChampionTitles;

					info.Award(m_Type, mobSubLevel);

					//Server.Engines.CityLoyalty.CityLoyaltySystem.OnSpawnCreatureKilled(m as BaseCreature, mobSubLevel);
				}

				// Only really needed once.
				if (m_Kills > kills)
					InvalidateProperties();

				var n = m_Kills / (double)MaxKills;
				var p = (int)(n * 100);

				switch (p)
				{
					case >= 90:
						AdvanceLevel();
						break;
					case > 0:
						SetWhiteSkullCount(p / 20);
						break;
				}

				if (DateTime.UtcNow >= ExpireTime)
					Expire();

				Respawn();
			}

			if (m_Timer is not {Running: true} || _NextGhostCheck >= DateTime.UtcNow)
				return;

			foreach (var ghost in m_Region.GetEnumeratedMobiles().OfType<PlayerMobile>().Where(pm => !pm.Alive && (pm.Corpse == null || pm.Corpse.Deleted)))
			{
				Map map = ghost.Map;
				Point3D loc = Helpers.GetNearestShrine(ghost, ref map);

				if (loc != Point3D.Zero)
				{
					ghost.MoveToWorld(loc, map);
				}
				else
				{
					ghost.MoveToWorld(new Point3D(989, 520, -50), Map.Malas);
				}
			}

			_NextGhostCheck = DateTime.UtcNow + TimeSpan.FromMinutes(Utility.RandomMinMax(5, 8));
		}

		public void AdvanceLevel()
		{
			ExpireTime = DateTime.UtcNow + m_ExpireDelay;

			if (Level < 16)
			{
				m_Kills = 0;
				++Level;
				InvalidateProperties();
				SetWhiteSkullCount(0);

				if (m_Altar != null)
				{
					Effects.PlaySound(m_Altar.Location, m_Altar.Map, 0x29);
					Effects.SendLocationEffect(new Point3D(m_Altar.X + 1, m_Altar.Y + 1, m_Altar.Z), m_Altar.Map, 0x3728, 10);
				}
			}
			else
			{
				SpawnChampion();
			}
		}

		public void SpawnChampion()
		{
			m_Kills = 0;
			Level = 0;
			StartLevel = 0;
			InvalidateProperties();
			SetWhiteSkullCount(0);

			try
			{
				Champion = Activator.CreateInstance(ChampionSpawnInfo.GetInfo(m_Type).Champion) as Mobile;
			}
			catch (Exception)
			{
				// ignored
			}

			if (Champion == null)
				return;

			Point3D p = new(X, Y, Z - 15);

			Champion.MoveToWorld(p, Map);
			((BaseCreature)Champion).Home = p;

			if (Champion is BaseChampion champion)
			{
				champion.OnChampPopped(this);
			}
		}

		private void Respawn()
		{
			if (!m_Active || Deleted || Champion != null)
				return;

			var currentLevel = Level;
			var currentRank = Rank;
			var maxSpawn = (int)(MaxKills * 0.5d * SpawnMod);
			if (currentLevel >= 16)
				maxSpawn = Math.Min(maxSpawn, MaxKills - m_Kills);
			if (maxSpawn < 3)
				maxSpawn = 3;

			var spawnRadius = (int)(SpawnRadius * ChampionSystem.SpawnRadiusModForLevel(Level));
			Rectangle2D spawnBounds = new(new Point2D(X - spawnRadius, Y - spawnRadius),
				new Point2D(X + spawnRadius, Y + spawnRadius));

			var mobCount = Creatures.Count(m => GetRankFor(m) == currentRank);

			while (mobCount <= maxSpawn)
			{
				var m = Spawn();

				if (m == null)
					return;

				var loc = GetSpawnLocation(spawnBounds, spawnRadius);

				// Allow creatures to turn into Paragons at Ilshenar champions.
				m.OnBeforeSpawn(loc, Map);

				Creatures.Add(m);
				m.MoveToWorld(loc, Map);
				++mobCount;

				if (m is not BaseCreature bc)
					continue;

				bc.Tamable = false;
				bc.IsChampionSpawn = true;

				if (!ConfinedRoaming)
				{
					bc.Home = Location;
					bc.RangeHome = spawnRadius;
				}
				else
				{
					bc.Home = bc.Location;

					Point2D xWall1 = new(spawnBounds.X, bc.Y);
					Point2D xWall2 = new(spawnBounds.X + spawnBounds.Width, bc.Y);
					Point2D yWall1 = new(bc.X, spawnBounds.Y);
					Point2D yWall2 = new(bc.X, spawnBounds.Y + spawnBounds.Height);

					var minXDist = Math.Min(bc.GetDistanceToSqrt(xWall1), bc.GetDistanceToSqrt(xWall2));
					var minYDist = Math.Min(bc.GetDistanceToSqrt(yWall1), bc.GetDistanceToSqrt(yWall2));

					bc.RangeHome = (int)Math.Min(minXDist, minYDist);
				}
			}
		}

		/*
		public Point3D GetSpawnLocation()
		{
			return GetSpawnLocation(m_SpawnArea, 24);
		}*/

		public Point3D GetSpawnLocation(Rectangle2D rect, int range)
		{
			var map = Map;

			if (map == null)
				return Location;
			_ = Location.X;
			_ = Location.Y;

			// Try 20 times to find a spawnable location.
			for (var i = 0; i < 20; i++)
			{
				var dx = Utility.Random(range * 2);
				var dy = Utility.Random(range * 2);
				var x = rect.X + dx;
				var y = rect.Y + dy;

				// Make spawn area circular
				//if ((cx - x) * (cx - x) + (cy - y) * (cy - y) > range * range)
				//	continue;

				var z = Map.GetAverageZ(x, y);

				if (Map.CanSpawnMobile(new Point2D(x, y), z))
					return new Point3D(x, y, z);

				/* try @ platform Z if map z fails */
				else if (Map.CanSpawnMobile(new Point2D(x, y), m_Platform.Location.Z))
					return new Point3D(x, y, m_Platform.Location.Z);
			}

			return Location;
		}

		public int Rank => ChampionSystem.RankForLevel(Level);

		public int GetRankFor(Mobile m)
		{
			var types = ChampionSpawnInfo.GetInfo(m_Type).SpawnTypes;
			var t = m.GetType();

			for (var i = 0; i < types.GetLength(0); i++)
			{
				var individualTypes = types[i];

				if (individualTypes.Any(t1 => t == t1))
				{
					return i;
				}
			}

			return -1;
		}

		public Mobile Spawn()
		{
			var types = ChampionSpawnInfo.GetInfo(m_Type).SpawnTypes;

			var v = Rank;

			if (v >= 0 && v < types.Length)
				return Spawn(types[v]);

			return null;
		}

		public static Mobile Spawn(params Type[] types)
		{
			try
			{
				return Activator.CreateInstance(types[Utility.Random(types.Length)]) as Mobile;
			}
			catch
			{
				return null;
			}
		}

		public void Expire()
		{
			m_Kills = 0;

			if (m_WhiteSkulls.Count == 0)
			{
				// They didn't even get 20%, go back a level
				if (Level > StartLevel)
					--Level;

				InvalidateProperties();
			}
			else
			{
				SetWhiteSkullCount(0);
			}

			ExpireTime = DateTime.UtcNow + m_ExpireDelay;
		}

		public Point3D GetRedSkullLocation(int index)
		{
			int x, y;

			switch (index)
			{
				case < 5:
					x = index - 2;
					y = -2;
					break;
				case < 9:
					x = 2;
					y = index - 6;
					break;
				case < 13:
					x = 10 - index;
					y = 2;
					break;
				default:
					x = -2;
					y = 14 - index;
					break;
			}

			return new Point3D(X + x, Y + y, Z - 15);
		}

		public Point3D GetWhiteSkullLocation(int index)
		{
			int x, y;

			switch (index)
			{
				default:
					x = -1;
					y = -1;
					break;
				case 1:
					x = 1;
					y = -1;
					break;
				case 2:
					x = 1;
					y = 1;
					break;
				case 3:
					x = -1;
					y = 1;
					break;
			}

			return new Point3D(X + x, Y + y, Z - 15);
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add("champion spawn");
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Active)
			{
				list.Add(1060742); // active
				list.Add(1060658, "Type\t{0}", m_Type); // ~1_val~: ~2_val~
				list.Add(1060659, "Level\t{0}", Level); // ~1_val~: ~2_val~
				list.Add(1060660, "Kills\t{0} of {1} ({2:F1}%)", m_Kills, MaxKills, 100.0 * ((double)m_Kills / MaxKills)); // ~1_val~: ~2_val~
																														   //list.Add( 1060661, "Spawn Range\t{0}", m_SpawnRange ); // ~1_val~: ~2_val~
			}
			else
			{
				list.Add(1060743); // inactive
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (m_Active)
				LabelTo(from, "{0} (Active; Level: {1}; Kills: {2}/{3})", m_Type, Level, m_Kills, MaxKills);
			else
				LabelTo(from, "{0} (Inactive)", m_Type);
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendGump(new PropertiesGump(from, this));
		}

		public override void OnLocationChange(Point3D oldLoc)
		{
			if (Deleted)
				return;

			if (m_Platform != null)
				m_Platform.Location = new Point3D(X, Y, Z - 20);

			if (m_Altar != null)
				m_Altar.Location = new Point3D(X, Y, Z - 15);

			if (m_Idol != null)
				m_Idol.Location = new Point3D(X, Y, Z - 15);

			if (m_RedSkulls != null)
			{
				for (int i = 0; i < m_RedSkulls.Count; ++i)
					m_RedSkulls[i].Location = GetRedSkullLocation(i);
			}

			if (m_WhiteSkulls != null)
			{
				for (int i = 0; i < m_WhiteSkulls.Count; ++i)
					m_WhiteSkulls[i].Location = GetWhiteSkullLocation(i);
			}

			m_SpawnArea.X += Location.X - oldLoc.X;
			m_SpawnArea.Y += Location.Y - oldLoc.Y;

			UpdateRegion();
		}

		public override void OnMapChange()
		{
			if (Deleted)
				return;

			if (m_Platform != null)
				m_Platform.Map = Map;

			if (m_Altar != null)
				m_Altar.Map = Map;

			if (m_Idol != null)
				m_Idol.Map = Map;

			if (m_RedSkulls != null)
			{
				for (var i = 0; i < m_RedSkulls.Count; ++i)
					m_RedSkulls[i].Map = Map;
			}

			if (m_WhiteSkulls != null)
			{
				for (var i = 0; i < m_WhiteSkulls.Count; ++i)
					m_WhiteSkulls[i].Map = Map;
			}

			UpdateRegion();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			m_Platform?.Delete();

			m_Altar?.Delete();

			m_Idol?.Delete();

			RemoveSkulls();

			if (Creatures != null)
			{
				for (var i = 0; i < Creatures.Count; ++i)
				{
					var mob = Creatures[i];

					if (!mob.Player)
						mob.Delete();
				}

				Creatures.Clear();
			}

			if (Champion is {Player: false})
				Champion.Delete();

			Stop();

			UpdateRegion();
		}

		public ChampionSpawn(Serial serial)
			: base(serial)
		{
		}

		public virtual void RegisterDamageTo(Mobile m)
		{
			if (m == null)
				return;

			foreach (var de in m.DamageEntries)
			{
				if (de.HasExpired)
					continue;

				var damager = de.Damager;

				var master = damager.GetDamageMaster(m);

				if (master != null)
					damager = master;

				RegisterDamage(damager, de.DamageGiven);
			}
		}

		public void RegisterDamage(Mobile from, int amount)
		{
			if (from is not {Player: true})
				return;

			if (m_DamageEntries.ContainsKey(from))
				m_DamageEntries[from] += amount;
			else
				m_DamageEntries.Add(from, amount);
		}

		public void AwardArtifact(Item artifact)
		{
			if (artifact == null)
				return;

			int totalDamage = 0;

			Dictionary<Mobile, int> validEntries = new();

			foreach (KeyValuePair<Mobile, int> kvp in m_DamageEntries)
			{
				if (IsEligible(kvp.Key, artifact))
				{
					validEntries.Add(kvp.Key, kvp.Value);
					totalDamage += kvp.Value;
				}
			}

			int randomDamage = Utility.RandomMinMax(1, totalDamage);

			totalDamage = 0;

			foreach (KeyValuePair<Mobile, int> kvp in validEntries)
			{
				totalDamage += kvp.Value;

				if (totalDamage >= randomDamage)
				{
					GiveArtifact(kvp.Key, artifact);
					return;
				}
			}

			artifact.Delete();
		}

		public static void GiveArtifact(Mobile to, Item artifact)
		{
			if (to == null || artifact == null)
				return;

			to.PlaySound(0x5B4);

			var pack = to.Backpack;

			if (pack == null || !pack.TryDropItem(to, artifact, false))
				artifact.Delete();
			else
				to.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
		}

		public bool IsEligible(Mobile m, Item artifact)
		{
			return m == null
				? throw new ArgumentNullException(nameof(m))
				: artifact == null
					? throw new ArgumentNullException(nameof(artifact))
					: m.Player && m.Alive && m.Region != null && m.Region == m_Region && m.Backpack != null &&
					  m.Backpack.CheckHold(m, artifact, false);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(8); // version

			writer.Write(StartLevel);

			writer.Write(KillsMod);
			writer.Write(GroupName);

			writer.Write(SpawnName);
			writer.Write(AutoRestart);
			writer.Write(SpawnMod);
			writer.Write(SpawnRadius);

			writer.Write(m_DamageEntries.Count);
			foreach (var kvp in m_DamageEntries)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}

			writer.Write(ConfinedRoaming);
			writer.WriteItem(m_Idol);
			writer.Write(HasBeenAdvanced);
			writer.Write(m_SpawnArea);

			writer.Write(RandomizeType);

			// writer.Write( m_SpawnRange );
			writer.Write(m_Kills);

			writer.Write(m_Active);
			writer.Write((int)m_Type);
			writer.Write(Creatures, true);
			writer.Write(m_RedSkulls, true);
			writer.Write(m_WhiteSkulls, true);
			writer.WriteItem(m_Platform);
			writer.WriteItem(m_Altar);
			writer.Write(m_ExpireDelay);
			writer.WriteDeltaTime(ExpireTime);
			writer.Write(Champion);
			writer.Write(RestartDelay);

			writer.Write(m_RestartTimer != null);

			if (m_RestartTimer != null)
				writer.WriteDeltaTime(RestartTime);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			m_DamageEntries = new Dictionary<Mobile, int>();

			int version = reader.ReadInt();

			switch (version)
			{
				case 8:
					StartLevel = reader.ReadInt();
					goto case 7;
				case 7:
					KillsMod = reader.ReadDouble();
					GroupName = reader.ReadString();
					goto case 6;
				case 6:
					SpawnName = reader.ReadString();
					AutoRestart = reader.ReadBool();
					SpawnMod = reader.ReadDouble();
					SpawnRadius = reader.ReadInt();
					goto case 5;
				case 5:
					{
						var entries = reader.ReadInt();
						for (var i = 0; i < entries; ++i)
						{
							var m = reader.ReadMobile();
							var damage = reader.ReadInt();

							if (m == null)
								continue;

							m_DamageEntries.Add(m, damage);
						}

						goto case 4;
					}
				case 4:
					{
						ConfinedRoaming = reader.ReadBool();
						m_Idol = reader.ReadItem<IdolOfTheChampion>();
						HasBeenAdvanced = reader.ReadBool();

						goto case 3;
					}
				case 3:
					{
						m_SpawnArea = reader.ReadRect2D();

						goto case 2;
					}
				case 2:
					{
						RandomizeType = reader.ReadBool();

						goto case 1;
					}
				case 1:
					{
						if (version < 3)
						{
							int oldRange = reader.ReadInt();

							m_SpawnArea = new Rectangle2D(new Point2D(X - oldRange, Y - oldRange), new Point2D(X + oldRange, Y + oldRange));
						}

						m_Kills = reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						if (version < 1)
							m_SpawnArea = new Rectangle2D(new Point2D(X - 24, Y - 24), new Point2D(X + 24, Y + 24));    //Default was 24

						bool active = reader.ReadBool();
						m_Type = (ChampionSpawnType)reader.ReadInt();
						Creatures = reader.ReadStrongMobileList();
						m_RedSkulls = reader.ReadStrongItemList();
						m_WhiteSkulls = reader.ReadStrongItemList();
						m_Platform = reader.ReadItem<ChampionPlatform>();
						m_Altar = reader.ReadItem<ChampionAltar>();
						m_ExpireDelay = reader.ReadTimeSpan();
						ExpireTime = reader.ReadDeltaTime();
						Champion = reader.ReadMobile();
						RestartDelay = reader.ReadTimeSpan();

						if (reader.ReadBool())
						{
							RestartTime = reader.ReadDeltaTime();
							BeginRestart(RestartTime - DateTime.UtcNow);
						}

						if (version < 4)
						{
							m_Idol = new IdolOfTheChampion(this);
							m_Idol.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
						}

						if (m_Platform == null || m_Altar == null || m_Idol == null)
							Delete();
						else if (active)
							Start(true);

						break;
					}
			}

			foreach (BaseCreature bc in Creatures.OfType<BaseCreature>())
			{
				bc.IsChampionSpawn = true;
			}

			Timer.DelayCall(TimeSpan.Zero, UpdateRegion);
		}

		public void SendGump(Mobile mob)
		{
			mob.SendGump(new ChampionSpawnInfoGump(this));
		}

		private class ChampionSpawnInfoGump : Gump
		{
			private class Damager
			{
				public readonly Mobile Mobile;
				public readonly int Damage;
				public Damager(Mobile mob, int dmg)
				{
					Mobile = mob;
					Damage = dmg;
				}

			}
			private const int GBoarder = 20;
			private const int GRowHeight = 25;
			private const int GFontHue = 0;
			private static readonly int[] GWidths = { 20, 160, 160, 20 };
			private static readonly int[] GTab;
			private static readonly int GWidth;

			static ChampionSpawnInfoGump()
			{
				GWidth = GWidths.Sum();
				var tab = 0;
				GTab = new int[GWidths.Length];
				for (var i = 0; i < GWidths.Length; ++i)
				{
					GTab[i] = tab;
					tab += GWidths[i];
				}
			}

			private readonly ChampionSpawn m_Spawn;

			public ChampionSpawnInfoGump(ChampionSpawn spawn)
				: base(40, 40)
			{
				m_Spawn = spawn;

				AddBackground(0, 0, GWidth, GBoarder * 2 + GRowHeight * (8 + spawn.m_DamageEntries.Count), 0x13BE);

				int top = GBoarder;
				AddLabel(GBoarder, top, GFontHue, "Champion Spawn Info Gump");
				top += GRowHeight;

				AddLabel(GTab[1], top, GFontHue, "Kills");
				AddLabel(GTab[2], top, GFontHue, spawn.Kills.ToString());
				top += GRowHeight;

				AddLabel(GTab[1], top, GFontHue, "Max Kills");
				AddLabel(GTab[2], top, GFontHue, spawn.MaxKills.ToString());
				top += GRowHeight;

				AddLabel(GTab[1], top, GFontHue, "Level");
				AddLabel(GTab[2], top, GFontHue, spawn.Level.ToString());
				top += GRowHeight;

				AddLabel(GTab[1], top, GFontHue, "Rank");
				AddLabel(GTab[2], top, GFontHue, spawn.Rank.ToString());
				top += GRowHeight;

				AddLabel(GTab[1], top, GFontHue, "Active");
				AddLabel(GTab[2], top, GFontHue, spawn.Active.ToString());
				top += GRowHeight;

				AddLabel(GTab[1], top, GFontHue, "Auto Restart");
				AddLabel(GTab[2], top, GFontHue, spawn.AutoRestart.ToString());
				top += GRowHeight;

				var damagers = spawn.m_DamageEntries.Keys.Select(mob => new Damager(mob, spawn.m_DamageEntries[mob])).ToList();
				damagers = damagers.OrderByDescending(x => x.Damage).ToList();

				foreach (var damager in damagers)
				{
					AddLabelCropped(GTab[1], top, 100, GRowHeight, GFontHue, damager.Mobile.RawName);
					AddLabelCropped(GTab[2], top, 80, GRowHeight, GFontHue, damager.Damage.ToString());
					top += GRowHeight;
				}

				AddButton(GWidth - (GBoarder + 30), top, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddLabel(GWidth - (GBoarder + 100), top, GFontHue, "Refresh");
			}

			public override void OnResponse(Network.NetState sender, RelayInfo info)
			{
				switch (info.ButtonID)
				{
					case 1:
						m_Spawn.SendGump(sender.Mobile);
						break;
				}
			}
		}
	}

	public class ChampionSpawnRegion : BaseRegion
	{
		public static void Initialize()
		{
			EventSink.OnLogout += OnLogout;
			EventSink.OnLogin += OnLogin;
		}

		public override bool YoungProtected => false;

		public ChampionSpawn ChampionSpawn { get; }

		public ChampionSpawnRegion(ChampionSpawn spawn)
			: base(null, spawn.Map, Find(spawn.Location, spawn.Map), spawn.SpawnArea)
		{
			ChampionSpawn = spawn;
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			return false;
		}

		public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			base.AlterLightLevel(m, ref global, ref personal);
			global = Math.Max(global, 1 + ChampionSpawn.Level); //This is a guesstimate.  TODO: Verify & get exact values // OSI testing: at 2 red skulls, light = 0x3 ; 1 red = 0x3.; 3 = 8; 9 = 0xD 8 = 0xD 12 = 0x12 10 = 0xD
		}

		public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			if (m is PlayerMobile && !m.Alive && (m.Corpse == null || m.Corpse.Deleted) && Map == Map.Felucca)
			{
				return false;
			}

			return base.OnMoveInto(m, d, newLocation, oldLocation);
		}

		public static void OnLogout(Mobile m)
		{
			if (m is PlayerMobile && m.Region.IsPartOf<ChampionSpawnRegion>() && m.AccessLevel == AccessLevel.Player && m.Map == Map.Felucca)
			{
				if (m.Alive && m.Backpack != null)
				{
					var list = new List<Item>(m.Backpack.Items.Where(i => i.LootType == LootType.Cursed));

					foreach (var item in list)
					{
						item.MoveToWorld(m.Location, m.Map);
					}

					ColUtility.Free(list);
				}

				Timer.DelayCall(TimeSpan.FromMilliseconds(250), () =>
				{
					Map map = m.LogoutMap;

					Point3D loc = Helpers.GetNearestShrine(m, ref map);

					if (loc != Point3D.Zero)
					{
						m.LogoutLocation = loc;
						m.LogoutMap = map;
					}
					else
					{
						m.LogoutLocation = new Point3D(989, 520, -50);
						m.LogoutMap = Map.Malas;
					}
				});
			}
		}

		public static void OnLogin(Mobile m)
		{
			if (m is PlayerMobile && !m.Alive && (m.Corpse == null || m.Corpse.Deleted) && m.Region.IsPartOf<ChampionSpawnRegion>() && m.Map == Map.Felucca)
			{
				Map map = m.Map;
				Point3D loc = Helpers.GetNearestShrine(m, ref map);

				if (loc != Point3D.Zero)
				{
					m.MoveToWorld(loc, map);
				}
				else
				{
					m.MoveToWorld(new Point3D(989, 520, -50), Map.Malas);
				}
			}
		}
	}

	public class IdolOfTheChampion : BaseItem
	{
		public ChampionSpawn Spawn { get; private set; }

		public override string DefaultName => "Idol of the Champion";

		public IdolOfTheChampion(ChampionSpawn spawn)
			: base(0x1F18)
		{
			Spawn = spawn;
			Movable = false;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Spawn?.Delete();
		}

		public IdolOfTheChampion(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(Spawn);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Spawn = reader.ReadItem() as ChampionSpawn;

						if (Spawn == null)
							Delete();

						break;
					}
			}
		}
	}
}
