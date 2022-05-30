using Server.Accounting;
using Server.Network;

namespace Server
{
	/// <summary>
	/// All basic server configuration
	/// </summary>
	public class Configuration
	{
		private static readonly Expansion Expansion = (Expansion)Settings.Configuration.Get<int>("Server", "Expansion");
		private static readonly Publishes Publishes = (Publishes)Settings.Configuration.Get<int>("Server", "Publish");

		public static void Configure()
		{
			Core.Expansion = Expansion;
			Core.Publishes = Publishes;
			AccountGold.Enabled = Core.TOL;
			AccountGold.ConvertOnBank = true;
			AccountGold.ConvertOnTrade = false;
			VirtualCheck.UseEditGump = true;

			bool Enabled = Core.AOS;

			Mobile.InsuranceEnabled = Enabled;
			ObjectPropertyList.Enabled = Enabled;
			Mobile.VisibleDamageType = Enabled ? VisibleDamageType.Related : VisibleDamageType.None;
			Mobile.GuildClickMessage = !Enabled;
			Mobile.AsciiClickMessage = !Enabled;

			if (Enabled)
			{
				AOS.DisableStatInfluences();

				if (ObjectPropertyList.Enabled)
					PacketHandlers.SingleClickProps = true; // single click for everything is overriden to check object property list

				Mobile.ActionDelay = 1000;
				Mobile.AOSStatusHandler = new AOSStatusHandler(AOS.GetStatus);
			}
		}
	}
}
