using Server.Commands;
using Server.Gumps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Server.Engines.Champions;

public class ChampionSystem
{
	private static bool _enabled;
	private static bool _initialized;
	private static readonly string Path = System.IO.Path.Combine("Saves", "Champions", "ChampionSystem.bin");
	private static readonly string ConfigPath = System.IO.Path.Combine("Data", "ChampionSpawns.xml");
	private static DateTime _lastRotate;
	private static TimeSpan _rotateDelay;
	private static readonly int[] Rank = new int[16];
	private static readonly int[] MaxKill = new int[4];
	private const bool ForceGenerate = false;

	public static int GoldShowerPiles { get; private set; }
	public static int GoldShowerMinAmount { get; private set; }
	public static int GoldShowerMaxAmount { get; private set; }
	public static int HarrowerGoldShowerPiles { get; private set; }
	public static int HarrowerGoldShowerMinAmount { get; private set; }
	public static int HarrowerGoldShowerMaxAmount { get; private set; }
	public static int PowerScrollAmount { get; private set; }
	public static int StatScrollAmount { get; private set; }

	public static List<ChampionSpawn> AllSpawns { get; } = new();

	public static int RankForLevel(int l)
	{
		if (l < 0)
			return 0;
		return l >= Rank.Length ? 3 : Rank[l];
	}
	public static int MaxKillsForLevel(int l)
	{
		return MaxKill[RankForLevel(l)];
	}
	public static double SpawnRadiusModForLevel(int l)
	{
		return RankForLevel(l) switch
		{
			0 => 1.0d,
			1 => 0.75d,
			2 => 0.5d,
			_ => 0.25d,
		};
	}
	public static double TranscendenceChance { get; private set; }
	public static double ScrollChance { get; private set; }

	public static void Configure()
	{
		_enabled = true;
		_rotateDelay = TimeSpan.FromDays(1.0);
		GoldShowerPiles = 50;
		GoldShowerMinAmount = 4000;
		GoldShowerMaxAmount = 5500;
		HarrowerGoldShowerPiles = 75;
		HarrowerGoldShowerMinAmount = 5000;
		HarrowerGoldShowerMaxAmount = 10000;
		PowerScrollAmount = 6;
		StatScrollAmount = 16;
		ScrollChance = 0.1d / 100.0d;
		TranscendenceChance = 50.0d / 100.0d;
		const int rank2 = 5;
		const int rank3 = 10;
		//const int rank4 = 10;
		for (var i = 0; i < Rank.Length; ++i)
		{
			Rank[i] = i switch
			{
				< rank2 => 0,
				< rank3 => 1,
				_ => 3
			};
		}
		MaxKill[0] = 256;
		MaxKill[1] = 128;
		MaxKill[2] = 64;
		MaxKill[3] = 32;
		EventSink.OnWorldLoad += EventSink_WorldLoad;
		EventSink.OnWorldSave += EventSink_WorldSave;
	}
	private static void EventSink_WorldSave()
	{
		Persistence.Serialize(
			Path,
			writer =>
			{
				writer.Write(1); // Version
				writer.Write(_initialized);
				writer.Write(_lastRotate);
				writer.WriteItemList(AllSpawns, true);
			});
	}

	private static void EventSink_WorldLoad()
	{
		Persistence.Deserialize(
			Path,
			reader =>
			{
				var version = reader.ReadInt();

				_initialized = reader.ReadBool();
				_lastRotate = reader.ReadDateTime();
				AllSpawns.AddRange(reader.ReadItemList().Cast<ChampionSpawn>());

				if (version == 0)
				{
					//m_ForceGenerate = true;
				}
			});
	}

	public static void Initialize()
	{
		CommandSystem.Register("GenChampSpawns", AccessLevel.GameMaster, GenSpawns_OnCommand);
		CommandSystem.Register("DelChampSpawns", AccessLevel.GameMaster, DelSpawns_OnCommand);

		CommandSystem.Register("ChampionInfo", AccessLevel.GameMaster, ChampionInfo_OnCommand);

		if (!_enabled || ForceGenerate)
		{
			_initialized = false;

			if (_enabled)
			{
				LoadSpawns();
			}
			else
			{
				RemoveSpawns();
			}
		}

		if (!_enabled)
			return;
		var internalTimer = new InternalTimer();
		if (internalTimer == null) throw new ArgumentNullException(nameof(internalTimer));
	}

	public static void GenSpawns_OnCommand(CommandEventArgs e)
	{
		LoadSpawns();
		e.Mobile.SendMessage("Champ Spawns Generated!");
	}

	public static void DelSpawns_OnCommand(CommandEventArgs e)
	{
		RemoveSpawns();
		e.Mobile.SendMessage("Champ Spawns Removed!");
	}

	public static void LoadSpawns()
	{
		if (_initialized)
			return;

		RemoveSpawns();

		Utility.PushColor(ConsoleColor.White);
		Console.WriteLine("Generating Champion Spawns");
		Utility.PopColor();

		XmlDocument doc = new();
		doc.Load(ConfigPath);
		var xmlNodeList = doc.GetElementsByTagName("championSystem")[0]?.ChildNodes;
		if (xmlNodeList != null)
			foreach (XmlNode node in xmlNodeList)
			{
				if (!node.Name.Equals("spawn"))
					continue;

				var spawn = new ChampionSpawn
				{
					SpawnName = GetAttr(node, "name", "Unamed Spawner")
				};
				string value = GetAttr(node, "type", null);

				if (value == null)
					spawn.RandomizeType = true;
				else
					spawn.Type = (ChampionSpawnType) Enum.Parse(typeof(ChampionSpawnType), value);

				value = GetAttr(node, "spawnMod", "1.0");
				spawn.SpawnMod = XmlConvert.ToDouble(value);
				value = GetAttr(node, "killsMod", "1.0");
				spawn.KillsMod = XmlConvert.ToDouble(value);

				foreach (XmlNode child in node.ChildNodes)
				{
					if (!child.Name.Equals("location"))
						continue;

					var x = XmlConvert.ToInt32(GetAttr(child, "x", "0"));
					var y = XmlConvert.ToInt32(GetAttr(child, "y", "0"));
					var z = XmlConvert.ToInt32(GetAttr(child, "z", "0"));
					var r = XmlConvert.ToInt32(GetAttr(child, "radius", "0"));
					var mapName = GetAttr(child, "map", "Felucca");
					var map = Map.Parse(mapName);

					spawn.SpawnRadius = r;
					spawn.MoveToWorld(new Point3D(x, y, z), map);
				}

				spawn.GroupName = GetAttr(node, "group", null);
				AllSpawns.Add(spawn);

				//if (spawn.Type == ChampionSpawnType.Infuse)
				//{
				//	PrimevalLichPuzzle.GenLichPuzzle(null);
				//}
			}

		Rotate();

		_initialized = true;
	}

	public static void RemoveSpawns()
	{
		if (AllSpawns == null || AllSpawns.Count <= 0)
			return;

		foreach (var s in AllSpawns.Where(sp => sp is {Deleted: false}))
		{
			s.Delete();
		}

		AllSpawns.Clear();
	}

	private static string GetAttr(XmlNode node, string name, string def)
	{
		var attr = node.Attributes?[name];
		return attr != null ? attr.Value : def;
	}

	[Usage("ChampionInfo")]
	[Description("Opens a UI that displays information about the champion system")]
	private static void ChampionInfo_OnCommand(CommandEventArgs e)
	{
		if (!_enabled)
		{
			e.Mobile.SendMessage("The champion system is not enabled.");
			return;
		}
		if (AllSpawns.Count <= 0)
		{
			e.Mobile.SendMessage("The champion system is enabled but no altars exist");
			return;
		}
		e.Mobile.SendGump(new ChampionSystemGump());
	}

	private static void Rotate()
	{
		Dictionary<string, List<ChampionSpawn>> groups = new();
		_lastRotate = DateTime.UtcNow;

		foreach (var spawn in AllSpawns.Where(spawn => spawn is {Deleted: false}))
		{
			if (spawn.GroupName == null)
			{
				spawn.AutoRestart = true;
				if (!spawn.Active)
					spawn.Active = true;
				continue;
			}
			if (!groups.TryGetValue(spawn.GroupName, out var group))
			{
				group = new List<ChampionSpawn>();
				groups.Add(spawn.GroupName, group);
			}
			group.Add(spawn);
		}

		foreach (var key in groups.Keys)
		{
			var group = groups[key];
			foreach (var spawn in group)
			{
				spawn.AutoRestart = false;
			}
			var s = group[Utility.Random(group.Count)];
			s.AutoRestart = true;
			if (!s.Active)
				s.Active = true;
		}
	}

	private static void OnSlice()
	{
		if (DateTime.UtcNow > _lastRotate + _rotateDelay)
			Rotate();
	}

	private class InternalTimer : Timer
	{
		public InternalTimer()
			: base(TimeSpan.FromMinutes(1.0d))
		{
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			OnSlice();
		}
	}

	private class ChampionSystemGump : Gump
	{
		private const int GBoarder = 20;
		private const int GRowHeight = 25;
		private const int GFontHue = 0;
		private static readonly int[] GWidths = { 20, 100, 100, 40, 40, 40, 80, 60, 50, 50, 50, 20 };
		private static readonly int[] GTab;
		private static readonly int GWidth;

		private List<ChampionSpawn> Spawners { get; }

		static ChampionSystemGump()
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

		public ChampionSystemGump()
			: base(40, 40)
		{
			Spawners = AllSpawns.Where(spawn => spawn is {Deleted: false}).ToList();

			AddBackground(0, 0, GWidth, GBoarder * 2 + Spawners.Count * GRowHeight + GRowHeight * 2, 0x13BE);

			var top = GBoarder;
			AddLabel(GBoarder, top, GFontHue, "Champion Spawn System Gump");
			top += GRowHeight;

			AddLabel(GTab[1], top, GFontHue, "Spawn Name");
			AddLabel(GTab[2], top, GFontHue, "Spawn Group");
			AddLabel(GTab[3], top, GFontHue, "X");
			AddLabel(GTab[4], top, GFontHue, "Y");
			AddLabel(GTab[5], top, GFontHue, "Z");
			AddLabel(GTab[6], top, GFontHue, "Map");
			AddLabel(GTab[7], top, GFontHue, "Active");
			AddLabel(GTab[8], top, GFontHue, "Auto");
			AddLabel(GTab[9], top, GFontHue, "Go");
			AddLabel(GTab[10], top, GFontHue, "Info");
			top += GRowHeight;

			for (int i = 0; i < Spawners.Count; i++)
			{
				ChampionSpawn spawn = Spawners[i];
				AddLabel(GTab[1], top, GFontHue, spawn.SpawnName);
				AddLabel(GTab[2], top, GFontHue, spawn.GroupName ?? "None");
				AddLabel(GTab[3], top, GFontHue, spawn.X.ToString());
				AddLabel(GTab[4], top, GFontHue, spawn.Y.ToString());
				AddLabel(GTab[5], top, GFontHue, spawn.Z.ToString());
				AddLabel(GTab[6], top, GFontHue, spawn.Map == null ? "null" : spawn.Map.ToString());
				AddLabel(GTab[7], top, GFontHue, spawn.Active ? "Y" : "N");
				AddLabel(GTab[8], top, GFontHue, spawn.AutoRestart ? "Y" : "N");
				AddButton(GTab[9], top, 0xFA5, 0xFA7, 1 + i, GumpButtonType.Reply, 0);
				AddButton(GTab[10], top, 0xFA5, 0xFA7, 1001 + i, GumpButtonType.Reply, 0);
				top += GRowHeight;
			}
		}

		public override void OnResponse(Network.NetState sender, RelayInfo info)
		{
			ChampionSpawn spawn;
			int idx;

			switch (info.ButtonID)
			{
				case > 0 and <= 1000:
				{
					idx = info.ButtonID - 1;
					if (idx < 0 || idx >= Spawners.Count)
						return;
					spawn = Spawners[idx];
					sender.Mobile.MoveToWorld(spawn.Location, spawn.Map);
					sender.Mobile.SendGump(this);
					break;
				}
				case > 1000:
				{
					idx = info.ButtonID - 1001;
					if (idx < 0 || idx > Spawners.Count)
						return;
					spawn = Spawners[idx];
					spawn.SendGump(sender.Mobile);
					break;
				}
			}
		}
	}
}
