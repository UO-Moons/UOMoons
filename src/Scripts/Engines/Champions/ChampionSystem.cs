using Server.Commands;
using Server.Gumps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Server.Engines.Champions
{
	public class ChampionSystem
	{
		private static bool m_Enabled = false;
		private static bool m_Initialized = false;
		private static readonly string m_Path = Path.Combine("Saves", "Champions", "ChampionSystem.bin");
		private static readonly string m_ConfigPath = Path.Combine("Data", "ChampionSpawns.xml");
		private static DateTime m_LastRotate;
		private static TimeSpan m_RotateDelay;
		private static InternalTimer m_Timer;
		private static readonly int[] m_Rank = new int[16];
		private static readonly int[] m_MaxKill = new int[4];
		private static readonly bool m_ForceGenerate = false;

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
			if (l >= m_Rank.Length)
				return 3;
			return m_Rank[l];
		}
		public static int MaxKillsForLevel(int l)
		{
			return m_MaxKill[RankForLevel(l)];
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
			m_Enabled = true;
			m_RotateDelay = TimeSpan.FromDays(1.0);
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
			int rank2 = 5;
			int rank3 = 10;
			int rank4 = 10;
			for (int i = 0; i < m_Rank.Length; ++i)
			{
				if (i < rank2)
					m_Rank[i] = 0;
				else if (i < rank3)
					m_Rank[i] = 1;
				else if (i < rank4)
					m_Rank[i] = 2;
				else
					m_Rank[i] = 3;
			}
			m_MaxKill[0] = 256;
			m_MaxKill[1] = 128;
			m_MaxKill[2] = 64;
			m_MaxKill[3] = 32;
			EventSink.OnWorldLoad += EventSink_WorldLoad;
			EventSink.OnWorldSave += EventSink_WorldSave;
		}
		private static void EventSink_WorldSave()
		{
			Persistence.Serialize(
				m_Path,
				writer =>
				{
					writer.Write(1); // Version
					writer.Write(m_Initialized);
					writer.Write(m_LastRotate);
					writer.WriteItemList(AllSpawns, true);
				});
		}

		private static void EventSink_WorldLoad()
		{
			Persistence.Deserialize(
				m_Path,
				reader =>
				{
					int version = reader.ReadInt();

					m_Initialized = reader.ReadBool();
					m_LastRotate = reader.ReadDateTime();
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

			CommandSystem.Register("ChampionInfo", AccessLevel.GameMaster, new CommandEventHandler(ChampionInfo_OnCommand));

			if (!m_Enabled || m_ForceGenerate)
			{
				m_Initialized = false;

				if (m_Enabled)
				{
					LoadSpawns();
				}
				else
				{
					RemoveSpawns();
				}
			}

			if (m_Enabled)
			{
				m_Timer = new InternalTimer();
			}
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
			if (m_Initialized)
				return;

			RemoveSpawns();

			Utility.PushColor(ConsoleColor.White);
			Console.WriteLine("Generating Champion Spawns");
			Utility.PopColor();

			ChampionSpawn spawn;

			XmlDocument doc = new();
			doc.Load(m_ConfigPath);
			foreach (XmlNode node in doc.GetElementsByTagName("championSystem")[0].ChildNodes)
			{
				if (node.Name.Equals("spawn"))
				{
					spawn = new ChampionSpawn
					{
						SpawnName = GetAttr(node, "name", "Unamed Spawner")
					};
					string value = GetAttr(node, "type", null);

					if (value == null)
						spawn.RandomizeType = true;
					else
						spawn.Type = (ChampionSpawnType)Enum.Parse(typeof(ChampionSpawnType), value);

					value = GetAttr(node, "spawnMod", "1.0");
					spawn.SpawnMod = XmlConvert.ToDouble(value);
					value = GetAttr(node, "killsMod", "1.0");
					spawn.KillsMod = XmlConvert.ToDouble(value);

					foreach (XmlNode child in node.ChildNodes)
					{
						if (child.Name.Equals("location"))
						{
							int x = XmlConvert.ToInt32(GetAttr(child, "x", "0"));
							int y = XmlConvert.ToInt32(GetAttr(child, "y", "0"));
							int z = XmlConvert.ToInt32(GetAttr(child, "z", "0"));
							int r = XmlConvert.ToInt32(GetAttr(child, "radius", "0"));
							string mapName = GetAttr(child, "map", "Felucca");
							Map map = Map.Parse(mapName);

							spawn.SpawnRadius = r;
							spawn.MoveToWorld(new Point3D(x, y, z), map);
						}
					}

					spawn.GroupName = GetAttr(node, "group", null);
					AllSpawns.Add(spawn);

					//if (spawn.Type == ChampionSpawnType.Infuse)
					//{
					//	PrimevalLichPuzzle.GenLichPuzzle(null);
					//}
				}
			}

			Rotate();

			m_Initialized = true;
		}

		public static void RemoveSpawns()
		{
			if (AllSpawns != null && AllSpawns.Count > 0)
			{
				foreach (ChampionSpawn s in AllSpawns.Where(sp => sp != null && !sp.Deleted))
				{
					s.Delete();
				}

				AllSpawns.Clear();
			}
		}

		private static string GetAttr(XmlNode node, string name, string def)
		{
			XmlAttribute attr = node.Attributes[name];
			if (attr != null)
				return attr.Value;
			return def;
		}

		[Usage("ChampionInfo")]
		[Description("Opens a UI that displays information about the champion system")]
		private static void ChampionInfo_OnCommand(CommandEventArgs e)
		{
			if (!m_Enabled)
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
			m_LastRotate = DateTime.UtcNow;

			foreach (ChampionSpawn spawn in AllSpawns.Where(spawn => spawn != null && !spawn.Deleted))
			{
				if (spawn.GroupName == null)
				{
					spawn.AutoRestart = true;
					if (!spawn.Active)
						spawn.Active = true;
					continue;
				}
				if (!groups.TryGetValue(spawn.GroupName, out List<ChampionSpawn> group))
				{
					group = new List<ChampionSpawn>();
					groups.Add(spawn.GroupName, group);
				}
				group.Add(spawn);
			}

			foreach (string key in groups.Keys)
			{
				List<ChampionSpawn> group = groups[key];
				foreach (ChampionSpawn spawn in group)
				{
					spawn.AutoRestart = false;
				}
				ChampionSpawn s = group[Utility.Random(group.Count)];
				s.AutoRestart = true;
				if (!s.Active)
					s.Active = true;
			}
		}

		private static void OnSlice()
		{
			if (DateTime.UtcNow > m_LastRotate + m_RotateDelay)
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
			private const int gBoarder = 20;
			private const int gRowHeight = 25;
			private const int gFontHue = 0;
			private static readonly int[] gWidths = { 20, 100, 100, 40, 40, 40, 80, 60, 50, 50, 50, 20 };
			private static readonly int[] gTab;
			private static readonly int gWidth;

			public List<ChampionSpawn> Spawners { get; set; }

			static ChampionSystemGump()
			{
				gWidth = gWidths.Sum();
				int tab = 0;
				gTab = new int[gWidths.Length];
				for (int i = 0; i < gWidths.Length; ++i)
				{
					gTab[i] = tab;
					tab += gWidths[i];
				}
			}

			public ChampionSystemGump()
				: base(40, 40)
			{
				Spawners = AllSpawns.Where(spawn => spawn != null && !spawn.Deleted).ToList();

				AddBackground(0, 0, gWidth, gBoarder * 2 + Spawners.Count * gRowHeight + gRowHeight * 2, 0x13BE);

				int top = gBoarder;
				AddLabel(gBoarder, top, gFontHue, "Champion Spawn System Gump");
				top += gRowHeight;

				AddLabel(gTab[1], top, gFontHue, "Spawn Name");
				AddLabel(gTab[2], top, gFontHue, "Spawn Group");
				AddLabel(gTab[3], top, gFontHue, "X");
				AddLabel(gTab[4], top, gFontHue, "Y");
				AddLabel(gTab[5], top, gFontHue, "Z");
				AddLabel(gTab[6], top, gFontHue, "Map");
				AddLabel(gTab[7], top, gFontHue, "Active");
				AddLabel(gTab[8], top, gFontHue, "Auto");
				AddLabel(gTab[9], top, gFontHue, "Go");
				AddLabel(gTab[10], top, gFontHue, "Info");
				top += gRowHeight;

				for (int i = 0; i < Spawners.Count; i++)
				{
					ChampionSpawn spawn = Spawners[i];
					AddLabel(gTab[1], top, gFontHue, spawn.SpawnName);
					AddLabel(gTab[2], top, gFontHue, spawn.GroupName ?? "None");
					AddLabel(gTab[3], top, gFontHue, spawn.X.ToString());
					AddLabel(gTab[4], top, gFontHue, spawn.Y.ToString());
					AddLabel(gTab[5], top, gFontHue, spawn.Z.ToString());
					AddLabel(gTab[6], top, gFontHue, spawn.Map == null ? "null" : spawn.Map.ToString());
					AddLabel(gTab[7], top, gFontHue, spawn.Active ? "Y" : "N");
					AddLabel(gTab[8], top, gFontHue, spawn.AutoRestart ? "Y" : "N");
					AddButton(gTab[9], top, 0xFA5, 0xFA7, 1 + i, GumpButtonType.Reply, 0);
					AddButton(gTab[10], top, 0xFA5, 0xFA7, 1001 + i, GumpButtonType.Reply, 0);
					top += gRowHeight;
				}
			}

			public override void OnResponse(Network.NetState sender, RelayInfo info)
			{
				ChampionSpawn spawn;
				int idx;

				if (info.ButtonID > 0 && info.ButtonID <= 1000)
				{
					idx = info.ButtonID - 1;
					if (idx < 0 || idx >= Spawners.Count)
						return;
					spawn = Spawners[idx];
					sender.Mobile.MoveToWorld(spawn.Location, spawn.Map);
					sender.Mobile.SendGump(this);
				}
				else if (info.ButtonID > 1000)
				{
					idx = info.ButtonID - 1001;
					if (idx < 0 || idx > Spawners.Count)
						return;
					spawn = Spawners[idx];
					spawn.SendGump(sender.Mobile);
				}
			}
		}
	}
}
