using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Server;

public class Firewall
{
	#region Firewall Entries
	public interface IFirewallEntry
	{
		bool IsBlocked(IPAddress address);
	}

	private class IpFirewallEntry : IFirewallEntry
	{
		private readonly IPAddress _mAddress;
		public IpFirewallEntry(IPAddress address)
		{
			_mAddress = address;
		}

		bool IFirewallEntry.IsBlocked(IPAddress address)
		{
			return _mAddress.Equals(address);
		}

		public override string ToString()
		{
			return _mAddress.ToString();
		}

		public override bool Equals(object obj)
		{
			return obj switch
			{
				IPAddress => obj.Equals(_mAddress),
				string @string when IPAddress.TryParse(@string, out var otherAddress) => otherAddress.Equals(
					_mAddress),
				IpFirewallEntry entry => _mAddress.Equals(entry._mAddress),
				_ => false
			};
		}

		public override int GetHashCode()
		{
			return _mAddress.GetHashCode();
		}
	}

	private class CidrFirewallEntry : IFirewallEntry
	{
		private readonly IPAddress _mCidrPrefix;
		private readonly int _mCidrLength;

		public CidrFirewallEntry(IPAddress cidrPrefix, int cidrLength)
		{
			_mCidrPrefix = cidrPrefix;
			_mCidrLength = cidrLength;
		}

		bool IFirewallEntry.IsBlocked(IPAddress address)
		{
			return Utility.IPMatchCIDR(_mCidrPrefix, address, _mCidrLength);
		}

		public override string ToString()
		{
			return $"{_mCidrPrefix}/{_mCidrLength}";
		}

		public override bool Equals(object obj)
		{
			switch (obj)
			{
				case string s:
				{
					string[] str = s.Split('/');

					if (str.Length == 2)
					{

						if (IPAddress.TryParse(str[0], out IPAddress cidrPrefix))
						{

							if (int.TryParse(str[1], out var cidrLength))
								return _mCidrPrefix.Equals(cidrPrefix) && _mCidrLength.Equals(cidrLength);
						}
					}

					break;
				}
				case CidrFirewallEntry firewallEntry:
				{
					return _mCidrPrefix.Equals(firewallEntry._mCidrPrefix) && _mCidrLength.Equals(firewallEntry._mCidrLength);
				}
			}

			return false;
		}

		public override int GetHashCode()
		{
			return _mCidrPrefix.GetHashCode() ^ _mCidrLength.GetHashCode();
		}
	}

	private class WildcardIpFirewallEntry : IFirewallEntry
	{
		private readonly string _mEntry;
		private bool _mValid = true;

		public WildcardIpFirewallEntry(string entry)
		{
			_mEntry = entry;
		}

		bool IFirewallEntry.IsBlocked(IPAddress address)
		{
			return _mValid && Utility.IPMatch(_mEntry, address, ref _mValid);
		}

		public override string ToString()
		{
			return _mEntry;
		}

		public override bool Equals(object obj)
		{
			return obj switch
			{
				string => obj.Equals(_mEntry),
				WildcardIpFirewallEntry entry => _mEntry.Equals(entry._mEntry),
				_ => false
			};
		}

		public override int GetHashCode()
		{
			return _mEntry.GetHashCode();
		}
	}
	#endregion


	static Firewall()
	{
		List = new List<IFirewallEntry>();

		const string path = "firewall.cfg";

		if (!File.Exists(path)) return;
		using StreamReader ip = new(path);

		while (ip.ReadLine() is { } line)
		{
			line = line.Trim();

			if (line.Length == 0)
				continue;

			List.Add(ToFirewallEntry(line));
		}
	}

	public static List<IFirewallEntry> List { get; }

	private static IFirewallEntry ToFirewallEntry(object entry)
	{
		return entry switch
		{
			IFirewallEntry entry1 => entry1,
			IPAddress address => new IpFirewallEntry(address),
			string @string => ToFirewallEntry(@string),
			_ => null
		};
	}

	public static IFirewallEntry ToFirewallEntry(string entry)
	{

		if (IPAddress.TryParse(entry, out IPAddress addr))
			return new IpFirewallEntry(addr);

		//Try CIDR parse
		if (entry == null) return new WildcardIpFirewallEntry(null);
		string[] str = entry.Split('/');

		if (str.Length != 2) return new WildcardIpFirewallEntry(entry);
		if (!IPAddress.TryParse(str[0], out IPAddress cidrPrefix)) return new WildcardIpFirewallEntry(entry);
		if (int.TryParse(str[1], out var cidrLength))
			return new CidrFirewallEntry(cidrPrefix, cidrLength);

		return new WildcardIpFirewallEntry(entry);
	}

	public static void RemoveAt(int index)
	{
		List.RemoveAt(index);
		Save();
	}

	public static void Remove(object obj)
	{
		IFirewallEntry entry = ToFirewallEntry(obj);

		if (entry == null) return;
		List.Remove(entry);
		Save();
	}

	public static void Add(object obj)
	{
		switch (obj)
		{
			case IPAddress address:
				Add(address);
				break;
			case string @string:
				Add(@string);
				break;
			case IFirewallEntry entry:
				Add(entry);
				break;
		}
	}

	private static void Add(IFirewallEntry entry)
	{
		if (!List.Contains(entry))
			List.Add(entry);

		Save();
	}

	private static void Add(string pattern)
	{
		IFirewallEntry entry = ToFirewallEntry(pattern);

		if (!List.Contains(entry))
			List.Add(entry);

		Save();
	}

	public static void Add(IPAddress ip)
	{
		IFirewallEntry entry = new IpFirewallEntry(ip);

		if (!List.Contains(entry))
			List.Add(entry);

		Save();
	}

	private static void Save()
	{
		const string path = "firewall.cfg";

		using StreamWriter op = new(path);
		for (var i = 0; i < List.Count; ++i)
			op.WriteLine(List[i]);
	}

	public static bool IsBlocked(IPAddress ip)
	{
		return List.Any(t => t.IsBlocked(ip));
	}
}
