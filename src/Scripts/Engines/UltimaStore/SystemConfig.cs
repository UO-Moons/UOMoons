//using Server.Engines.Points;
using System;

namespace Server.Engines.UOStore;

public enum CurrencyType
{
	None,
	Sovereigns,
	Gold,
	//PointsSystem,
	Custom
}

public delegate int CustomCurrencyHandler(Mobile m, int consume);

public static class Configuration
{
	public static bool Enabled { get; set; }
	public static string Website { get; }

	/// <summary>
	///     A hook to allow handling of custom currencies.
	///     This implementation should be treated as such;
	///     If 'consume' is less than zero, return the currency total.
	///     Else deduct from the currency total, return the amount consumed.
	/// </summary>
	private static CustomCurrencyHandler ResolveCurrency { get; }

	public static CurrencyType CurrencyImpl { get; }
	public static string CurrencyName { get; }
	public static bool CurrencyDisplay { get; }

	//public static PointsType PointsImpl { get; set; }

	public static double CostMultiplier { get; }

	public static int CartCapacity { get; }

	static Configuration()
	{
		Enabled = Core.TOL;
		Website = "https://uo.com/ultima-store/";

		ResolveCurrency = null;

		CurrencyImpl = CurrencyType.Sovereigns;
		CurrencyName = "Sovereigns";
		CurrencyDisplay = true;

		//PointsImpl = Config.GetEnum("Store.PointsImpl", PointsType.None);

		CostMultiplier = 1.0;
		CartCapacity = 10;
	}

	public static int GetCustomCurrency(Mobile m)
	{
		if (ResolveCurrency != null)
		{
			return ResolveCurrency(m, -1);
		}

		m.SendMessage(1174, "Currency is not set up for this system. Contact a shard administrator.");

		Utility.WriteConsole(ConsoleColor.Red, "[Ultima Store]: No custom currency method has been implemented.");

		return 0;
	}

	public static int DeductCustomCurrency(Mobile m, int amount)
	{
		if (ResolveCurrency != null)
		{
			return ResolveCurrency(m, amount);
		}

		m.SendMessage(1174, "Currency is not set up for this system. Contact a shard administrator.");

		Utility.WriteConsole(ConsoleColor.Red, "[Ultima Store]: No custom currency deduction method has been implemented.");

		return 0;
	}
}
