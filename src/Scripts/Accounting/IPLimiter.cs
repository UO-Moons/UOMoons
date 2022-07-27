using Server.Network;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Server.Misc;

public class IpLimiter
{
	private static bool Enabled { get; } = Settings.Configuration.Get<bool>("Accounts", "IpLimiterEnabled");
	public static bool SocketBlock { get; } = Settings.Configuration.Get<bool>("Accounts", "SocketBlock"); // true to block at connection, false to block at login request
	private static int MaxAddresses { get; } = Settings.Configuration.Get<int>("Accounts", "AddressPerIP");

	private static readonly IPAddress[] Exemptions = System.Array.Empty<IPAddress>();

	public static bool IsExempt(IPAddress ip)
	{
		return Exemptions.Contains(ip);
	}

	public static bool Verify(IPAddress ourAddress)
	{
		if (!Enabled || IsExempt(ourAddress))
			return true;

		List<NetState> netStates = NetState.Instances;

		int count = 0;

		for (var i = 0; i < netStates.Count; ++i)
		{
			NetState compState = netStates[i];

			if (!ourAddress.Equals(compState.Address)) continue;
			++count;

			if (count >= MaxAddresses)
				return false;
		}

		return true;
	}
}
