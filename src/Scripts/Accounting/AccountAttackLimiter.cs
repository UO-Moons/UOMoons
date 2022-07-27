using Server.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Server.Accounting;

public class AccountAttackLimiter
{
	private static readonly bool Enabled = true;

	public static void Initialize()
	{
		if (!Enabled)
			return;

		PacketHandlers.RegisterThrottler(0x80, Throttle_Callback);
		PacketHandlers.RegisterThrottler(0x91, Throttle_Callback);
		PacketHandlers.RegisterThrottler(0xCF, Throttle_Callback);
	}

	private static bool Throttle_Callback(NetState ns)
	{
		InvalidAccountAccessLog accessLog = FindAccessLog(ns);

		if (accessLog == null)
			return true;

		return DateTime.UtcNow >= accessLog.LastAccessTime + ComputeThrottle(accessLog.Counts);
	}

	private static readonly List<InvalidAccountAccessLog> MList = new();

	private static InvalidAccountAccessLog FindAccessLog(NetState ns)
	{
		if (ns == null)
			return null;

		IPAddress ipAddress = ns.Address;

		for (var i = 0; i < MList.Count; ++i)
		{
			InvalidAccountAccessLog accessLog = MList[i];

			if (accessLog.HasExpired)
				MList.RemoveAt(i--);
			else if (accessLog.Address.Equals(ipAddress))
				return accessLog;
		}

		return null;
	}

	public static void RegisterInvalidAccess(NetState ns)
	{
		if (ns == null || !Enabled)
			return;

		InvalidAccountAccessLog accessLog = FindAccessLog(ns);

		if (accessLog == null)
			MList.Add(accessLog = new InvalidAccountAccessLog(ns.Address));

		accessLog.Counts += 1;
		accessLog.RefreshAccessTime();

		if (accessLog.Counts < 3) return;
		try
		{
			using StreamWriter op = new("throttle.log", true);
			op.WriteLine(
				"{0}\t{1}\t{2}",
				DateTime.UtcNow,
				ns,
				accessLog.Counts
			);
		}
		catch
		{
			// ignored
		}
	}

	private static TimeSpan ComputeThrottle(int counts)
	{
		return counts switch
		{
			>= 15 => TimeSpan.FromMinutes(5.0),
			>= 10 => TimeSpan.FromMinutes(1.0),
			>= 5 => TimeSpan.FromSeconds(20.0),
			>= 3 => TimeSpan.FromSeconds(10.0),
			>= 1 => TimeSpan.FromSeconds(2.0),
			_ => TimeSpan.Zero
		};
	}
}

public class InvalidAccountAccessLog
{
	public IPAddress Address { get; }
	public DateTime LastAccessTime { get; private set; }
	public int Counts { get; set; }

	public bool HasExpired => DateTime.UtcNow >= LastAccessTime + TimeSpan.FromHours(1.0);

	public void RefreshAccessTime()
	{
		LastAccessTime = DateTime.UtcNow;
	}

	public InvalidAccountAccessLog(IPAddress address)
	{
		Address = address;
		RefreshAccessTime();
	}
}
