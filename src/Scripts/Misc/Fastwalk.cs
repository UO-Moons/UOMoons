using System;

namespace Server.Misc
{
	public class Fastwalk
	{
		private static readonly int MaxSteps = Settings.Configuration.Get<int>("Fastwalk", "MaxSteps");
		private static readonly bool Enabled = Settings.Configuration.Get<bool>("Fastwalk", "Enabled");
		private static readonly bool UOTDOverride = Settings.Configuration.Get<bool>("Fastwalk", "UOTDOverride");
		private static readonly AccessLevel AccessOverride = (AccessLevel)Settings.Configuration.Get<int>("Fastwalk", "AccessOverride"); // Anyone with this or higher access level is not checked for fastwalk

		public static void Initialize()
		{
			Mobile.FwdMaxSteps = MaxSteps;
			Mobile.FwdEnabled = Enabled;
			Mobile.FwdUotdOverride = UOTDOverride;
			Mobile.FwdAccessOverride = AccessOverride;

			if (Enabled)
				EventSink.FastWalk += OnFastWalk;
		}

		public static void OnFastWalk(FastWalkEventArgs e)
		{
			e.Blocked = true;//disallow this fastwalk
			Console.WriteLine("Client: {0}: Fast movement detected (name={1})", e.NetState, e.NetState.Mobile.Name);
		}
	}
}
