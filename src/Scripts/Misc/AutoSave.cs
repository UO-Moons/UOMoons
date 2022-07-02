using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.IO;

namespace Server.Misc
{
	public class AutoSave : Timer
	{
		public static bool SavesEnabled { get; set; } = Settings.Configuration.Get<bool>("AutoSave", "Enabled");
		private static readonly TimeSpan m_Delay = TimeSpan.FromMinutes(Settings.Configuration.Get<int>("AutoSave", "Frequency"));
		private static readonly TimeSpan m_Warning = TimeSpan.FromSeconds(Settings.Configuration.Get<int>("AutoSave", "WarningTime"));

		public static void Initialize()
		{
			new AutoSave().Start();
			CommandSystem.Register("SetSaves", AccessLevel.Administrator, new CommandEventHandler(SetSaves_OnCommand));
		}

		[Usage("SetSaves <true | false>")]
		[Description("Enables or disables automatic shard saving.")]
		public static void SetSaves_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{
				SavesEnabled = e.GetBoolean(0);
				e.Mobile.SendMessage("Saves have been {0}.", SavesEnabled ? "enabled" : "disabled");
			}
			else
			{
				e.Mobile.SendMessage("Format: SetSaves <true | false>");
			}
		}

		public AutoSave() : base(m_Delay - m_Warning, m_Delay)
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			if (!SavesEnabled || AutoRestart.Restarting)
				return;

			if (m_Warning == TimeSpan.Zero)
			{
				Save(true);
			}
			else
			{
				int s = (int)m_Warning.TotalSeconds;
				int m = s / 60;
				s %= 60;

				if (m > 0 && s > 0)
					World.Broadcast(0x35, true, "The world will save in {0} minute{1} and {2} second{3}.", m, m != 1 ? "s" : "", s, s != 1 ? "s" : "");
				else if (m > 0)
					World.Broadcast(0x35, true, "The world will save in {0} minute{1}.", m, m != 1 ? "s" : "");
				else
					World.Broadcast(0x35, true, "The world will save in {0} second{1}.", s, s != 1 ? "s" : "");

				DelayCall(m_Warning, new TimerCallback(Save));
			}
		}

		public static void Save()
		{
			Save(false);
		}
		public static TAutoSaveTimer tr_timer;
		public static void Save(bool permitBackgroundWrite)
		{
			if (AutoRestart.Restarting)
				return;

			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m != null && m is PlayerMobile mobile && mobile.AutoSaveGump == true)
				{
					PlayerMobile from = m as PlayerMobile;
					tr_timer = new TAutoSaveTimer(from, 10);
					tr_timer.Start();
					m.SendGump(new SaveGump());
				}
			}

			World.WaitForWriteCompletion();

			try { Backup(); }
			catch (Exception e) { Console.WriteLine("WARNING: Automatic backup FAILED: {0}", e); }

			World.Save(true, permitBackgroundWrite);
		}

		private static readonly string[] m_Backups = new string[]
			{
				"Third Backup",
				"Second Backup",
				"Most Recent"
			};

		private static void Backup()
		{
			if (m_Backups.Length == 0)
				return;

			string root = Path.Combine(Core.BaseDirectory, "Backups/Automatic");

			if (!Directory.Exists(root))
				Directory.CreateDirectory(root);

			string[] existing = Directory.GetDirectories(root);

			for (int i = 0; i < m_Backups.Length; ++i)
			{
				DirectoryInfo dir = Match(existing, m_Backups[i]);

				if (dir == null)
					continue;

				if (i > 0)
				{
					string timeStamp = FindTimeStamp(dir.Name);

					if (timeStamp != null)
					{
						try { dir.MoveTo(FormatDirectory(root, m_Backups[i - 1], timeStamp)); }
						catch { }
					}
				}
				else
				{
					try { dir.Delete(true); }
					catch { }
				}
			}

			string saves = Path.Combine(Core.BaseDirectory, "Saves");

			if (Directory.Exists(saves))
				Directory.Move(saves, FormatDirectory(root, m_Backups[m_Backups.Length - 1], GetTimeStamp()));
		}

		private static DirectoryInfo Match(string[] paths, string match)
		{
			for (int i = 0; i < paths.Length; ++i)
			{
				DirectoryInfo info = new(paths[i]);

				if (info.Name.StartsWith(match))
					return info;
			}

			return null;
		}

		private static string FormatDirectory(string root, string name, string timeStamp)
		{
			return Path.Combine(root, string.Format("{0} ({1})", name, timeStamp));
		}

		private static string FindTimeStamp(string input)
		{
			int start = input.IndexOf('(');

			if (start >= 0)
			{
				int end = input.IndexOf(')', ++start);

				if (end >= start)
					return input.Substring(start, end - start);
			}

			return null;
		}

		private static string GetTimeStamp()
		{
			DateTime now = DateTime.UtcNow;

			return string.Format("{0}-{1}-{2} {3}-{4:D2}-{5:D2}",
					now.Day,
					now.Month,
					now.Year,
					now.Hour,
					now.Minute,
					now.Second
				);
		}

		public class TAutoSaveTimer : Timer
		{
			private Mobile m_mobile;
			private PlayerMobile m_pmobile;
			private int cnt = 0;
			private int m_count = 0;
			private int m_countmax;

			public TAutoSaveTimer(Mobile mobile, int count) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1), count)
			{
				Priority = TimerPriority.TenMs;
				m_mobile = mobile;
				m_pmobile = (PlayerMobile)mobile;
				m_countmax = count;
			}
			protected override void OnTick()
			{
				if (!m_mobile.Alive)
				{
					Stop();
				}
				cnt += 1;
				m_count += 10;

				if (cnt == 2)
				{
					m_pmobile.CloseGump(typeof(SaveGump));
					m_pmobile.SendGump(new SaveGump());
				}
				if (cnt == 4)
				{
					m_pmobile.CloseGump(typeof(SaveGump));
					m_pmobile.SendGump(new SaveGump());
				}
				if (cnt == 6)
				{
					m_pmobile.CloseGump(typeof(SaveGump));
					m_pmobile.SendGump(new SaveGump());
				}
				if (cnt == 8)
				{
					m_pmobile.CloseGump(typeof(SaveGump));
					m_pmobile.SendGump(new SaveGump());
				}
				if (cnt == 10)
				{
					m_pmobile.CloseGump(typeof(SaveGump));
				}

				if (m_count == m_countmax)
				{
					return;
				}
			}
		}

		public class SaveGump : Gump
		{
			public SaveGump()
				: base(50, 50)
			{
				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;
				AddPage(0);
				AddBackground(5, 5, 415, 100, 9270);
				AddLabel(165, 30, 2062, string.Format("UOMoons"));
				AddLabel(105, 55, 1165, @"The world is saving...   Please be patient.");
				AddImage(25, 25, 5608);
				AddItem(360, 50, 6168);
			}
		}

		[Usage("AutoSaveGump || ASG")]
		[Description("Manual command to call the Auto Save gump and see what settings you have on.")]
		public static void AutSaveGump_OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;

			if (from.HasGump(typeof(GumpOptions)))
			{
				from.CloseGump(typeof(GumpOptions));
			}
			from.SendGump(new GumpOptions(from));

		}

		public class GumpOptions : Gump
		{
			public GumpOptions(Mobile from) : base(0, 0)
			{
				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;
				AddPage(0);
				AddBackground(0, 29, 192, 168, 9200);
				AddImage(10, 41, 52);
				AddLabel(19, 123, 3, @"Auto Save Gump (Auto/Man)");
				AddImage(82, 157, 113);
				AddButton(15, 163, 2111, 2112, 1, GumpButtonType.Reply, 0);
				AddImage(75, 56, 2529);
				AddButton(123, 162, 2114, 248, 2, GumpButtonType.Reply, 0);
				AddLabel(59, 44, 36, @"Gump Options");

			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				Mobile from = sender.Mobile;

				PlayerMobile From = from as PlayerMobile;

				//From.CloseGump(typeof(PernOptions));

				if (info.ButtonID == 1)
				{
					From.AutoSaveGump = true;
					return;
				}
				if (info.ButtonID == 2)
				{
					From.AutoSaveGump = false;
					return;
				}
			}
		}
	}
}
