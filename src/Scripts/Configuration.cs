using Server.Accounting;
using Server.Network;

namespace Server;

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
		AccountGold.Enabled = Core.EJ;
		AccountGold.ConvertOnBank = true;
		AccountGold.ConvertOnTrade = false;
		VirtualCheck.UseEditGump = true;

		bool enabled = Core.AOS;
		bool enabled2 = Core.LBR;

		Mobile.InsuranceEnabled = enabled;
		ObjectPropertyList.Enabled = enabled2;
		Mobile.VisibleDamageType = enabled ? VisibleDamageType.Related : VisibleDamageType.None;
		Mobile.GuildClickMessage = !enabled || !enabled2;
		Mobile.AsciiClickMessage = !enabled || !enabled2;

		if (!enabled) return;
		AOS.DisableStatInfluences();

		if (ObjectPropertyList.Enabled)
			PacketHandlers.SingleClickProps = true; // single click for everything is overriden to check object property list

		Mobile.ActionDelay = 1000;
		Mobile.AOSStatusHandler = AOS.GetStatus;
	}
}
