using Server.Accounting;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.NewMagincia;

public class MaginciaLottoSystem : Item
{
	private static readonly TimeSpan DefaultLottoDuration = TimeSpan.FromDays(30);
	public const int WritExpirePeriod = 30;
	public const bool AutoResetLotto = false;

	public static MaginciaLottoSystem Instance { get; set; }

	public static List<MaginciaHousingPlot> Plots { get; } = new();

	private static Dictionary<Map, List<Rectangle2D>> FreeHousingZones { get; set; }

	private static Dictionary<Mobile, List<NewMaginciaMessage>> MessageQueue { get; } = new();

	private Timer _timer;
	private bool _enabled;

	public static void Initialize()
	{
		EventSink.OnLogin += OnLogin;

		Instance?.PruneMessages();
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ResetAuctions
	{
		get => false;
		set
		{
			if (!value)
				return;
			foreach (var plot in Plots.Where(plot => plot.IsAvailable))
			{
				plot.LottoEnds = DateTime.UtcNow + LottoDuration;
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Enabled
	{
		get => _enabled;
		set
		{
			if (_enabled == value)
				return;
			if (value)
			{
				StartTimer();
			}
			else
			{
				EndTimer();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public static int GoldSink { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan LottoDuration { get; private set; }

	public MaginciaLottoSystem() : base(3240)
	{
		Movable = false;
		_enabled = true;
		LottoDuration = DefaultLottoDuration;

		FreeHousingZones = new Dictionary<Map, List<Rectangle2D>>
		{
			[Map.Trammel] = new(),
			[Map.Felucca] = new()
		};

		if (_enabled)
			StartTimer();

		LoadPlots();
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.AccessLevel > AccessLevel.Player)
		{
			from.CloseGump(typeof(LottoTrackingGump));
			from.CloseGump(typeof(PlotTrackingGump));
			from.SendGump(new LottoTrackingGump());
		}
	}

	private void StartTimer()
	{
		_timer?.Stop();

		_timer = Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), ProcessTick);
		_timer.Priority = TimerPriority.OneMinute;
		_timer.Start();
	}

	private void EndTimer()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}
	}

	private void ProcessTick()
	{
		List<MaginciaHousingPlot> plots = new List<MaginciaHousingPlot>(Plots);

		foreach (MaginciaHousingPlot plot in plots)
		{
			if (plot.IsAvailable && plot.LottoEnds != DateTime.MinValue && DateTime.UtcNow > plot.LottoEnds)
				plot.EndLotto();

			if (plot.Expires != DateTime.MinValue && plot.Expires < DateTime.UtcNow)
			{
				if (plot.Writ != null)
					plot.Writ.OnExpired();
				else
					UnregisterPlot(plot);
			}
		}

		ColUtility.Free(plots);

		if (Plots.Count == 0)
			EndTimer();
	}

	public override void Delete()
	{
	}

	private static void RegisterPlot(MaginciaHousingPlot plot)
	{
		Plots.Add(plot);
	}

	public static void UnregisterPlot(MaginciaHousingPlot plot)
	{
		if (plot == null)
			return;

		if (plot.Stone is {Deleted: false})
			plot.Stone.Delete();

		if (Plots.Contains(plot))
			Plots.Remove(plot);

		if (plot.Map != null && FreeHousingZones.ContainsKey(plot.Map) && !FreeHousingZones[plot.Map].Contains(plot.Bounds))
			FreeHousingZones[plot.Map].Add(plot.Bounds);
	}

	public static bool IsRegisteredPlot(MaginciaHousingPlot plot)
	{
		return Plots.Contains(plot);
	}

	public static bool IsFreeHousingZone(Point3D p, Map map)
	{
		return FreeHousingZones.ContainsKey(map) && FreeHousingZones[map].Any(rec => rec.Contains(p));
	}

	public static void CheckHousePlacement(Mobile from, Point3D center)
	{
		MaginciaLottoSystem system = Instance;

		if (system is {Enabled: true} && from.Backpack != null && IsInMagincia(center.X, center.Y, from.Map))
		{
			List<Item> items = new();

			Item[] packItems = from.Backpack.FindItemsByType(typeof(WritOfLease));
			Item[] bankItems = from.BankBox.FindItemsByType(typeof(WritOfLease));

			if (packItems is {Length: > 0})
				items.AddRange(packItems);

			if (bankItems is {Length: > 0})
				items.AddRange(bankItems);

			foreach (Item item in items)
			{
				if (item is not WritOfLease { Expired: false, Plot: { } } lease ||
				    !lease.Plot.Bounds.Contains(center) || from.Map != lease.Plot.Map) continue;
				lease.OnExpired();
				return;
			}
		}
	}

	private static bool IsInMagincia(int x, int y, Map map)
	{
		return x is > 3614 and < 3817 && y is > 2031 and < 2274 && (map == Map.Trammel || map == Map.Felucca);
	}

	private void LoadPlots()
	{
		for (int i = 0; i < MagHousingZones.Length; i++)
		{
			bool prime = i is > 0 and < 6 || i > 14;

			MaginciaHousingPlot tramplot = new(m_Identifiers[i], MagHousingZones[i], prime, Map.Trammel);
			MaginciaHousingPlot felplot = new(m_Identifiers[i], MagHousingZones[i], prime, Map.Felucca);

			RegisterPlot(tramplot);
			RegisterPlot(felplot);

			tramplot.AddPlotStone(m_StoneLocs[i]);
			tramplot.LottoEnds = DateTime.UtcNow + LottoDuration;

			felplot.AddPlotStone(m_StoneLocs[i]);
			felplot.LottoEnds = DateTime.UtcNow + LottoDuration;
		}
	}

	public static Rectangle2D[] MagHousingZones { get; } =
	{
		new(3686, 2125, 18, 18), // C1
		new(3686, 2086, 18, 18), // C2 / Prime
		new(3686, 2063, 18, 18), // C3 / Prime

		new(3657, 2036, 18, 18), // N1 / Prime 
		new(3648, 2058, 18, 18), // N2 / Prime
		new(3636, 2081, 18, 18), // N3 / Prime

		new(3712, 2123, 16, 16), // SE3
		new(3712, 2151, 18, 16), // SE2
		new(3712, 2172, 18, 16), // SE1
		new(3729, 2135, 16, 16), // SE4

		new(3655, 2213, 18, 18), // SW1        
		new(3656, 2191, 18, 16), // SW2
		new(3628, 2197, 20, 20), // SW3        
		new(3628, 2175, 18, 18), // SW4
		new(3657, 2165, 18, 18), // SW5   

		new(3745, 2122, 16, 18), // E1 / Prime       
		new(3765, 2122, 18, 18), // E2 / Prime
		new(3787, 2130, 18, 18), // E3 / Prime       
		new(3784, 2108, 18, 17), // E4 / Prime
		new(3765, 2086, 18, 18), // E5 / Prime      
		new(3749, 2065, 18, 18), // E6 / Prime
		new(3715, 2090, 18, 18), // E7 / Prime            
	};

	private static readonly Point3D[] m_StoneLocs = new Point3D[]
	{
		new(3683, 2134, 20),
		new(3704, 2092, 5),
		new(3704, 2069, 5),

		new(3677, 2045, 20),
		new(3667, 2065, 20),
		new(3644, 2099, 20),

		new(3711, 2131, 20),
		new(3711, 2160, 20),
		new(3711, 2180, 20),
		new(3735, 2133, 20),

		new(3676, 2220, 20),
		new(3675, 2198, 20),
		new(3647, 2205, 22),
		new(3647, 2184, 21),
		new(3665, 2183, 22),

		new(3753, 2119, 21),
		new(3772, 2119, 21),
		new(3785, 2127, 25),
		new(3790, 2106, 30),
		new(3761, 2090, 20),
		new(3746, 2064, 23),
		new(3711, 2087, 5)
	};

	private static readonly string[] m_Identifiers = {
		"C-1",
		"C-2",
		"C-3",

		"N-1",
		"N-2",
		"N-3",

		"SE-1",
		"SE-2",
		"SE-3",
		"SE-4",

		"SW-1",
		"SW-2",
		"SW-3",
		"SW-4",
		"SW-5",

		"E-1",
		"E-2",
		"E-3",
		"E-4",
		"E-5",
		"E-6",
		"E-7"
	};

	public static Point3D GetPlotStoneLoc(MaginciaHousingPlot plot)
	{
		if (plot == null)
			return Point3D.Zero;

		for (int i = 0; i < m_Identifiers.Length; i++)
		{
			if (m_Identifiers[i] == plot.Identifier)
				return m_StoneLocs[i];
		}

		int z = plot.Map.GetAverageZ(plot.Bounds.X - 1, plot.Bounds.Y - 1);
		return new Point3D(plot.Bounds.X - 1, plot.Bounds.Y - 1, z);
	}

	public static string FormatSextant(MaginciaHousingPlot plot)
	{
		int z = plot.Map.GetAverageZ(plot.Bounds.X, plot.Bounds.Y);
		Point3D p = new(plot.Bounds.X, plot.Bounds.Y, z);

		return FormatSextant(p, plot.Map);
	}

	private static string FormatSextant(Point3D p, Map map)
	{
		int xLong = 0, yLat = 0;
		int xMins = 0, yMins = 0;
		bool xEast = false, ySouth = false;

		if (Sextant.Format(p, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
		{
			return $"{yLat}° {yMins}'{(ySouth ? "S" : "N")}, {xLong}° {xMins}'{(xEast ? "E" : "W")}";
		}

		return p.ToString();
	}

	#region Messages
	public static void SendMessageTo(Mobile from, TextDefinition title, TextDefinition body, TimeSpan expires)
	{
		SendMessageTo(from, new NewMaginciaMessage(title, body, expires));
	}

	public static void SendMessageTo(Mobile from, NewMaginciaMessage message)
	{
		if (from == null || message == null)
			return;

		AddMessageToQueue(from, message);

		if (from is PlayerMobile {NetState: { }} mobile)
		{
			mobile.CloseGump(typeof(NewMaginciaMessageGump));
			mobile.CloseGump(typeof(NewMaginciaMessageListGump));
			mobile.CloseGump(typeof(NewMaginciaMessageDetailGump));

			var messages = GetMessages(mobile);

			if (messages != null)
			{
				BaseGump.SendGump(new NewMaginciaMessageGump(mobile, messages));
			}
		}
	}

	private static void AddMessageToQueue(Mobile from, NewMaginciaMessage message)
	{
		if (!MessageQueue.ContainsKey(from) || MessageQueue[from] == null)
		{
			MessageQueue[from] = new List<NewMaginciaMessage>();
		}

		MessageQueue[from].Add(message);
	}

	public static void RemoveMessageFromQueue(Mobile from, NewMaginciaMessage message)
	{
		if (from == null || message == null)
			return;

		if (MessageQueue.ContainsKey(from) && MessageQueue[from].Contains(message))
		{
			MessageQueue[from].Remove(message);

			if (MessageQueue[from].Count == 0)
			{
				MessageQueue.Remove(from);
			}

			return;
		}

		if (from.Account is Account account)
		{
			for (int i = 0; i < account.Length; i++)
			{
				var m = account[i];

				if (m == from)
				{
					continue;
				}

				if (MessageQueue.ContainsKey(m) && MessageQueue[m].Contains(message))
				{
					MessageQueue[m].Remove(message);

					if (MessageQueue[m].Count == 0)
					{
						MessageQueue.Remove(m);
					}

					break;
				}
			}
		}
	}

	private static void OnLogin(Mobile from)
	{
		CheckMessages(from);

		var messages = GetMessages(from);

		if (messages != null)
		{
			from.CloseGump(typeof(NewMaginciaMessageGump));
			BaseGump.SendGump(new NewMaginciaMessageGump((PlayerMobile)from, messages));
		}
		else if (from.Account != null)
		{
			var account = from.Account.Username;
		}

		GetWinnerGump(from);
	}

	private void PruneMessages()
	{
		List<Mobile> mobiles = new(MessageQueue.Keys);

		foreach (Mobile m in mobiles)
		{
			List<NewMaginciaMessage> messages = new(MessageQueue[m]);

			foreach (var message in messages.Where(message => MessageQueue.ContainsKey(m) && MessageQueue[m].Contains(message) && message.Expired))
			{
				MessageQueue[m].Remove(message);
			}

			ColUtility.Free(messages);
		}

		ColUtility.Free(mobiles);
	}

	public static List<NewMaginciaMessage> GetMessages(Mobile m)
	{
		List<NewMaginciaMessage> list = null;

		if (MessageQueue.ContainsKey(m))
		{
			var messages = MessageQueue[m];

			if (messages.Count == 0)
			{
				MessageQueue.Remove(m);
			}
			else
			{
				list = new List<NewMaginciaMessage>();

				list.AddRange(messages);
			}
		}

		if (m.Account != null)
		{
			foreach (var kvp in MessageQueue.Where(kvp => kvp.Key != m && kvp.Key.Account != null && kvp.Key.Account.Username == m.Account.Username && kvp.Value.Any(message => message.AccountBound)))
			{
				list ??= new List<NewMaginciaMessage>();

				list.AddRange(kvp.Value.Where(message => message.AccountBound));
			}
		}

		list = list?.OrderBy(message => message.Expires).ToList();

		return list;
	}

	private static void CheckMessages(Mobile from)
	{
		if (!MessageQueue.ContainsKey(from) || MessageQueue[from] == null || MessageQueue[from].Count == 0)
			return;

		List<NewMaginciaMessage> list = new(MessageQueue[from]);

		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Expired)
				MessageQueue[from].Remove(list[i]);
		}
	}
	#endregion

	public static void GetWinnerGump(Mobile from)
	{
		if (from.Account is not Account acct)
			return;

		for (int i = 0; i < acct.Length; i++)
		{
			Mobile m = acct[i];

			if (m == null)
				continue;

			foreach (var plot in Plots.Where(plot => plot.Expires != DateTime.MinValue && plot.Winner == m))
			{
				from.SendGump(new PlotWinnerGump(plot));
				return;
			}
		}
	}

	public MaginciaLottoSystem(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(GoldSink);
		writer.Write(_enabled);
		writer.Write(LottoDuration);

		writer.Write(Plots.Count);
		for (int i = 0; i < Plots.Count; i++)
			Plots[i].Serialize(writer);

		writer.Write(FreeHousingZones[Map.Trammel].Count);
		foreach (Rectangle2D rec in FreeHousingZones[Map.Trammel])
			writer.Write(rec);

		writer.Write(FreeHousingZones[Map.Felucca].Count);
		foreach (Rectangle2D rec in FreeHousingZones[Map.Felucca])
			writer.Write(rec);

		writer.Write(MessageQueue.Count);
		foreach (KeyValuePair<Mobile, List<NewMaginciaMessage>> kvp in MessageQueue)
		{
			writer.Write(kvp.Key);

			writer.Write(kvp.Value.Count);
			foreach (NewMaginciaMessage message in kvp.Value)
				message.Serialize(writer);
		}

		Timer.DelayCall(TimeSpan.FromSeconds(30), PruneMessages);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		FreeHousingZones = new Dictionary<Map, List<Rectangle2D>>
		{
			[Map.Trammel] = new(),
			[Map.Felucca] = new()
		};

		GoldSink = reader.ReadInt();
		_enabled = reader.ReadBool();
		LottoDuration = reader.ReadTimeSpan();

		int c = reader.ReadInt();
		for (int i = 0; i < c; i++)
			RegisterPlot(new MaginciaHousingPlot(reader));

		c = reader.ReadInt();
		for (int i = 0; i < c; i++)
			FreeHousingZones[Map.Trammel].Add(reader.ReadRect2D());

		c = reader.ReadInt();
		for (int i = 0; i < c; i++)
			FreeHousingZones[Map.Felucca].Add(reader.ReadRect2D());

		c = reader.ReadInt();
		for (int i = 0; i < c; i++)
		{
			Mobile m = reader.ReadMobile();
			List<NewMaginciaMessage> messages = new();

			int count = reader.ReadInt();
			for (int j = 0; j < count; j++)
				messages.Add(new NewMaginciaMessage(reader));

			if (m != null && messages.Count > 0)
				MessageQueue[m] = messages;
		}

		if (_enabled)
			StartTimer();

		Instance = this;

		Timer.DelayCall(ValidatePlots);
	}

	private void ValidatePlots()
	{
		for (int i = 0; i < m_Identifiers.Length; i++)
		{
			Rectangle2D rec = MagHousingZones[i];
			string id = m_Identifiers[i];

			MaginciaHousingPlot plotTram = Plots.FirstOrDefault(p => p.Identifier == id && p.Map == Map.Trammel);
			MaginciaHousingPlot plotFel = Plots.FirstOrDefault(p => p.Identifier == id && p.Map == Map.Felucca);

			if (plotTram == null && !FreeHousingZones[Map.Trammel].Contains(rec))
			{
				Console.WriteLine("Adding {0} to Magincia Free Housing Zone.[{1}]", rec, "Plot non-existent");
				FreeHousingZones[Map.Trammel].Add(rec);
			}
			else if (plotTram is {Stone: null} && (plotTram.Writ == null || plotTram.Writ.Expired))
			{
				Console.WriteLine("Adding {0} to Magincia Free Housing Zone.[{1}]", rec, "Plot existed, writ expired");
				UnregisterPlot(plotTram);
			}

			switch (plotFel)
			{
				case null when !FreeHousingZones[Map.Felucca].Contains(rec):
					Console.WriteLine("Adding {0} to Magincia Free Housing Zone.[{1}]", rec, "Plot non-existent");
					FreeHousingZones[Map.Felucca].Add(rec);
					break;
				case {Stone: null} when (plotFel.Writ == null || plotFel.Writ.Expired):
					Console.WriteLine("Adding {0} to Magincia Free Housing Zone.[{1}]", rec, "Plot existed, writ expired");
					UnregisterPlot(plotFel);
					break;
			}
		}
	}
}
