using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Champions;

public class ChampionSpawn : BaseItem
{
	private const int MaxStrayDistance = 250;

	private bool _mActive;
	private ChampionSpawnType _mType;
	private List<Item> _mRedSkulls;
	private List<Item> _mWhiteSkulls;
	private ChampionPlatform _mPlatform;
	private ChampionAltar _mAltar;
	private int _mKills;

	//private int m_SpawnRange;
	private Rectangle2D _mSpawnArea;
	private ChampionSpawnRegion _mRegion;

	private TimeSpan _mExpireDelay;
	private Timer _mTimer, _mRestartTimer;

	private IdolOfTheChampion _mIdol;
	private Dictionary<Mobile, int> _mDamageEntries;

	private List<Mobile> Creatures { get; set; }

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
	private bool ConfinedRoaming { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool HasBeenAdvanced { get; set; }

	[Constructable]
	public ChampionSpawn()
		: base(0xBD2)
	{
		Movable = false;
		Visible = false;

		Creatures = new List<Mobile>();
		_mRedSkulls = new List<Item>();
		_mWhiteSkulls = new List<Item>();

		_mPlatform = new ChampionPlatform(this);
		_mAltar = new ChampionAltar(this);
		_mIdol = new IdolOfTheChampion(this);

		_mExpireDelay = TimeSpan.FromMinutes(10.0);
		RestartDelay = TimeSpan.FromMinutes(10.0);

		_mDamageEntries = new Dictionary<Mobile, int>();
		RandomizeType = false;

		SpawnRadius = 35;
		SpawnMod = 1;

		Timer.DelayCall(TimeSpan.Zero, SetInitialSpawnArea);
	}

	private void SetInitialSpawnArea()
	{
		//Previous default used to be 24;
		SpawnArea = new Rectangle2D(new Point2D(X - SpawnRadius, Y - SpawnRadius),
			new Point2D(X + SpawnRadius, Y + SpawnRadius));
	}

	private void UpdateRegion()
	{
		_mRegion?.Unregister();

		if (Deleted || Map == Map.Internal)
			return;

		_mRegion = new ChampionSpawnRegion(this);
		_mRegion.Register();
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool RandomizeType { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int Kills
	{
		get => _mKills;
		set
		{
			_mKills = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Rectangle2D SpawnArea
	{
		get => _mSpawnArea;
		private set
		{
			_mSpawnArea = value;
			InvalidateProperties();
			UpdateRegion();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private TimeSpan RestartDelay { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private DateTime RestartTime { get; set; }

	//[CommandProperty(AccessLevel.GameMaster)]
	//public TimeSpan ExpireDelay
	//{
	//	get => m_ExpireDelay;
	//	set => m_ExpireDelay = value;
	//}

	[CommandProperty(AccessLevel.GameMaster)]
	private DateTime ExpireTime { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public ChampionSpawnType Type
	{
		get => _mType;
		set
		{
			_mType = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Active
	{
		get => _mActive;
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
	public Mobile Champion { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Level
	{
		get => _mRedSkulls.Count;
		private set
		{
			for (var i = _mRedSkulls.Count - 1; i >= value; --i)
			{
				_mRedSkulls[i].Delete();
				_mRedSkulls.RemoveAt(i);
			}

			for (var i = _mRedSkulls.Count; i < value; ++i)
			{
				Item skull = new(0x1854)
				{
					Hue = 0x26,
					Movable = false,
					Light = LightType.Circle150
				};

				skull.MoveToWorld(GetRedSkullLocation(i), Map);

				_mRedSkulls.Add(skull);
			}

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private int StartLevel { get; set; }

	private void RemoveSkulls()
	{
		if (_mWhiteSkulls != null)
		{
			for (var i = 0; i < _mWhiteSkulls.Count; ++i)
				_mWhiteSkulls[i].Delete();

			_mWhiteSkulls.Clear();
		}

		if (_mRedSkulls == null) return;
		{
			for (var i = 0; i < _mRedSkulls.Count; i++)
				_mRedSkulls[i].Delete();

			_mRedSkulls.Clear();
		}
	}

	private int MaxKills
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

	private void SetWhiteSkullCount(int val)
	{
		for (var i = _mWhiteSkulls.Count - 1; i >= val; --i)
		{
			_mWhiteSkulls[i].Delete();
			_mWhiteSkulls.RemoveAt(i);
		}

		for (var i = _mWhiteSkulls.Count; i < val; ++i)
		{
			Item skull = new(0x1854)
			{
				Movable = false,
				Light = LightType.Circle150
			};

			skull.MoveToWorld(GetWhiteSkullLocation(i), Map);

			_mWhiteSkulls.Add(skull);

			Effects.PlaySound(skull.Location, skull.Map, 0x29);
			Effects.SendLocationEffect(new Point3D(skull.X + 1, skull.Y + 1, skull.Z), skull.Map, 0x3728, 10);
		}
	}

	private void Start(bool serverLoad = false)
	{
		if (_mActive || Deleted)
			return;

		_mActive = true;
		HasBeenAdvanced = false;

		_mTimer?.Stop();

		_mTimer = new SliceTimer(this);
		_mTimer.Start();

		_mRestartTimer?.Stop();

		_mRestartTimer = null;

		if (_mAltar != null)
			_mAltar.Hue = 0;

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

		if (Level > 0 && _mAltar != null)
		{
			Effects.PlaySound(_mAltar.Location, _mAltar.Map, 0x29);
			Effects.SendLocationEffect(new Point3D(_mAltar.X + 1, _mAltar.Y + 1, _mAltar.Z), _mAltar.Map, 0x3728, 10);
		}
	}

	private void Stop()
	{
		if (!_mActive || Deleted)
			return;

		_mActive = false;
		HasBeenAdvanced = false;

		// We must despawn all the creatures.
		if (Creatures != null)
		{
			for (var i = 0; i < Creatures.Count; ++i)
				Creatures[i].Delete();

			Creatures.Clear();
		}

		_mTimer?.Stop();

		_mTimer = null;

		_mRestartTimer?.Stop();

		_mRestartTimer = null;

		if (_mAltar != null)
			_mAltar.Hue = 0x455;

		//PrimevalLichPuzzle.Update(this);

		RemoveSkulls();
		_mKills = 0;
	}

	private void BeginRestart(TimeSpan ts)
	{
		_mRestartTimer?.Stop();

		RestartTime = DateTime.UtcNow + ts;

		_mRestartTimer = new RestartTimer(this, ts);
		_mRestartTimer.Start();
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

	private static ScrollOfTranscendence CreateRandomSoT(bool felucca)
	{
		var level = Utility.RandomMinMax(1, 5);

		if (felucca)
			level += 5;

		return ScrollOfTranscendence.CreateRandom(level, level);
	}

	#endregion

	private static void GiveScrollTo(Mobile killer, SpecialScroll scroll)
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

			if (chance <= Utility.Random(100)) continue;
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

	private DateTime _nextGhostCheck;

	public void OnSlice()
	{
		if (!_mActive || Deleted)
			return;

		int currentRank = Rank;

		if (Champion != null)
		{
			if (Champion.Deleted)
			{
				RegisterDamageTo(Champion);

				if (Champion is BaseChampion champion)
					AwardArtifact(champion.GetArtifact());

				_mDamageEntries.Clear();

				if (_mAltar != null)
				{
					_mAltar.Hue = 0x455;

					if (!Core.ML || Map == Map.Felucca)
					{
						_ = new StarRoomGate(true, _mAltar.Location, _mAltar.Map);
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
			var kills = _mKills;

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
					++_mKills;

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

				info.Award(_mType, mobSubLevel);

				//Server.Engines.CityLoyalty.CityLoyaltySystem.OnSpawnCreatureKilled(m as BaseCreature, mobSubLevel);
			}

			// Only really needed once.
			if (_mKills > kills)
				InvalidateProperties();

			var n = _mKills / (double)MaxKills;
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

		if (_mTimer is not {Running: true} || _nextGhostCheck >= DateTime.UtcNow)
			return;

		foreach (var ghost in _mRegion.GetEnumeratedMobiles().OfType<PlayerMobile>().Where(pm => !pm.Alive && (pm.Corpse == null || pm.Corpse.Deleted)))
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

		_nextGhostCheck = DateTime.UtcNow + TimeSpan.FromMinutes(Utility.RandomMinMax(5, 8));
	}

	public void AdvanceLevel()
	{
		ExpireTime = DateTime.UtcNow + _mExpireDelay;

		if (Level < 16)
		{
			_mKills = 0;
			++Level;
			InvalidateProperties();
			SetWhiteSkullCount(0);

			if (_mAltar == null) return;
			Effects.PlaySound(_mAltar.Location, _mAltar.Map, 0x29);
			Effects.SendLocationEffect(new Point3D(_mAltar.X + 1, _mAltar.Y + 1, _mAltar.Z), _mAltar.Map, 0x3728, 10);
		}
		else
		{
			SpawnChampion();
		}
	}

	private void SpawnChampion()
	{
		_mKills = 0;
		Level = 0;
		StartLevel = 0;
		InvalidateProperties();
		SetWhiteSkullCount(0);

		try
		{
			Champion = Activator.CreateInstance(ChampionSpawnInfo.GetInfo(_mType).Champion) as Mobile;
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
		if (!_mActive || Deleted || Champion != null)
			return;

		var currentLevel = Level;
		var currentRank = Rank;
		var maxSpawn = (int)(MaxKills * 0.5d * SpawnMod);
		if (currentLevel >= 16)
			maxSpawn = Math.Min(maxSpawn, MaxKills - _mKills);
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

	private Point3D GetSpawnLocation(Rectangle2D rect, int range)
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
			if (Map.CanSpawnMobile(new Point2D(x, y), _mPlatform.Location.Z))
				return new Point3D(x, y, _mPlatform.Location.Z);
		}

		return Location;
	}

	public int Rank => ChampionSystem.RankForLevel(Level);

	private int GetRankFor(Mobile m)
	{
		var types = ChampionSpawnInfo.GetInfo(_mType).SpawnTypes;
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

	private Mobile Spawn()
	{
		var types = ChampionSpawnInfo.GetInfo(_mType).SpawnTypes;

		var v = Rank;

		if (v >= 0 && v < types.Length)
			return Spawn(types[v]);

		return null;
	}

	private static Mobile Spawn(params Type[] types)
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

	private void Expire()
	{
		_mKills = 0;

		if (_mWhiteSkulls.Count == 0)
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

		ExpireTime = DateTime.UtcNow + _mExpireDelay;
	}

	private Point3D GetRedSkullLocation(int index)
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

	private Point3D GetWhiteSkullLocation(int index)
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

		if (_mActive)
		{
			list.Add(1060742); // active
			list.Add(1060658, "Type\t{0}", _mType); // ~1_val~: ~2_val~
			list.Add(1060659, "Level\t{0}", Level); // ~1_val~: ~2_val~
			list.Add(1060660, "Kills\t{0} of {1} ({2:F1}%)", _mKills, MaxKills, 100.0 * ((double)_mKills / MaxKills)); // ~1_val~: ~2_val~
			//list.Add( 1060661, "Spawn Range\t{0}", m_SpawnRange ); // ~1_val~: ~2_val~
		}
		else
		{
			list.Add(1060743); // inactive
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		if (_mActive)
			LabelTo(from, "{0} (Active; Level: {1}; Kills: {2}/{3})", _mType, Level, _mKills, MaxKills);
		else
			LabelTo(from, "{0} (Inactive)", _mType);
	}

	public override void OnDoubleClick(Mobile from)
	{
		from.SendGump(new PropertiesGump(from, this));
	}

	public override void OnLocationChange(Point3D oldLoc)
	{
		if (Deleted)
			return;

		if (_mPlatform != null)
			_mPlatform.Location = new Point3D(X, Y, Z - 20);

		if (_mAltar != null)
			_mAltar.Location = new Point3D(X, Y, Z - 15);

		if (_mIdol != null)
			_mIdol.Location = new Point3D(X, Y, Z - 15);

		if (_mRedSkulls != null)
		{
			for (var i = 0; i < _mRedSkulls.Count; ++i)
				_mRedSkulls[i].Location = GetRedSkullLocation(i);
		}

		if (_mWhiteSkulls != null)
		{
			for (var i = 0; i < _mWhiteSkulls.Count; ++i)
				_mWhiteSkulls[i].Location = GetWhiteSkullLocation(i);
		}

		_mSpawnArea.X += Location.X - oldLoc.X;
		_mSpawnArea.Y += Location.Y - oldLoc.Y;

		UpdateRegion();
	}

	public override void OnMapChange()
	{
		if (Deleted)
			return;

		if (_mPlatform != null)
			_mPlatform.Map = Map;

		if (_mAltar != null)
			_mAltar.Map = Map;

		if (_mIdol != null)
			_mIdol.Map = Map;

		if (_mRedSkulls != null)
		{
			for (var i = 0; i < _mRedSkulls.Count; ++i)
				_mRedSkulls[i].Map = Map;
		}

		if (_mWhiteSkulls != null)
		{
			for (var i = 0; i < _mWhiteSkulls.Count; ++i)
				_mWhiteSkulls[i].Map = Map;
		}

		UpdateRegion();
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		_mPlatform?.Delete();

		_mAltar?.Delete();

		_mIdol?.Delete();

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

	private void RegisterDamage(Mobile from, int amount)
	{
		if (from is not {Player: true})
			return;

		if (_mDamageEntries.ContainsKey(from))
			_mDamageEntries[from] += amount;
		else
			_mDamageEntries.Add(from, amount);
	}

	private void AwardArtifact(Item artifact)
	{
		if (artifact == null)
			return;

		int totalDamage = 0;

		Dictionary<Mobile, int> validEntries = new();

		foreach (var kvp in _mDamageEntries.Where(kvp => IsEligible(kvp.Key, artifact)))
		{
			validEntries.Add(kvp.Key, kvp.Value);
			totalDamage += kvp.Value;
		}

		int randomDamage = Utility.RandomMinMax(1, totalDamage);

		totalDamage = 0;

		foreach (KeyValuePair<Mobile, int> kvp in validEntries)
		{
			totalDamage += kvp.Value;

			if (totalDamage < randomDamage) continue;
			GiveArtifact(kvp.Key, artifact);
			return;
		}

		artifact.Delete();
	}

	private static void GiveArtifact(Mobile to, Item artifact)
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

	private bool IsEligible(Mobile m, Item artifact)
	{
		return m == null
			? throw new ArgumentNullException(nameof(m))
			: artifact == null
				? throw new ArgumentNullException(nameof(artifact))
				: m.Player && m.Alive && m.Region != null && m.Region == _mRegion && m.Backpack != null &&
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

		writer.Write(_mDamageEntries.Count);
		foreach (var kvp in _mDamageEntries)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value);
		}

		writer.Write(ConfinedRoaming);
		writer.Write(_mIdol);
		writer.Write(HasBeenAdvanced);
		writer.Write(_mSpawnArea);

		writer.Write(RandomizeType);

		// writer.Write( m_SpawnRange );
		writer.Write(_mKills);

		writer.Write(_mActive);
		writer.Write((int)_mType);
		writer.Write(Creatures, true);
		writer.Write(_mRedSkulls, true);
		writer.Write(_mWhiteSkulls, true);
		writer.Write(_mPlatform);
		writer.Write(_mAltar);
		writer.Write(_mExpireDelay);
		writer.WriteDeltaTime(ExpireTime);
		writer.Write(Champion);
		writer.Write(RestartDelay);

		writer.Write(_mRestartTimer != null);

		if (_mRestartTimer != null)
			writer.WriteDeltaTime(RestartTime);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		_mDamageEntries = new Dictionary<Mobile, int>();

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

					_mDamageEntries.Add(m, damage);
				}

				goto case 4;
			}
			case 4:
			{
				ConfinedRoaming = reader.ReadBool();
				_mIdol = reader.ReadItem<IdolOfTheChampion>();
				HasBeenAdvanced = reader.ReadBool();

				goto case 3;
			}
			case 3:
			{
				_mSpawnArea = reader.ReadRect2D();

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

					_mSpawnArea = new Rectangle2D(new Point2D(X - oldRange, Y - oldRange), new Point2D(X + oldRange, Y + oldRange));
				}

				_mKills = reader.ReadInt();

				goto case 0;
			}
			case 0:
			{
				if (version < 1)
					_mSpawnArea = new Rectangle2D(new Point2D(X - 24, Y - 24), new Point2D(X + 24, Y + 24));    //Default was 24

				bool active = reader.ReadBool();
				_mType = (ChampionSpawnType)reader.ReadInt();
				Creatures = reader.ReadStrongMobileList();
				_mRedSkulls = reader.ReadStrongItemList();
				_mWhiteSkulls = reader.ReadStrongItemList();
				_mPlatform = reader.ReadItem<ChampionPlatform>();
				_mAltar = reader.ReadItem<ChampionAltar>();
				_mExpireDelay = reader.ReadTimeSpan();
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
					_mIdol = new IdolOfTheChampion(this);
					_mIdol.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
				}

				if (_mPlatform == null || _mAltar == null || _mIdol == null)
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

		private readonly ChampionSpawn _mSpawn;

		public ChampionSpawnInfoGump(ChampionSpawn spawn)
			: base(40, 40)
		{
			_mSpawn = spawn;

			AddBackground(0, 0, GWidth, GBoarder * 2 + GRowHeight * (8 + spawn._mDamageEntries.Count), 0x13BE);

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

			var damagers = spawn._mDamageEntries.Keys.Select(mob => new Damager(mob, spawn._mDamageEntries[mob])).ToList();
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
					_mSpawn.SendGump(sender.Mobile);
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

	private static void OnLogout(Mobile m)
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

	private static void OnLogin(Mobile m)
	{
		if (m is not PlayerMobile || m.Alive || (m.Corpse != null && !m.Corpse.Deleted) ||
		    !m.Region.IsPartOf<ChampionSpawnRegion>() || m.Map != Map.Felucca) return;
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
