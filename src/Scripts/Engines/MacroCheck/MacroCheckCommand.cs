using Server.Engines;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Commands;

public class MacroCheckCommand
{
	public static readonly TimeSpan TimeBetweenMacroChecks = TimeSpan.FromMinutes(5);

	public static void Initialize()
	{
		CommandSystem.Register("MC", AccessLevel.Counselor, MacroCheck_OnCommand);
		CommandSystem.Register("MacroCheck", AccessLevel.Counselor, MacroCheck_OnCommand);
	}

	[Usage("MacroCheck")]
	[Description("we're sending a gump to a player that can macro.")]
	private static void MacroCheck_OnCommand(CommandEventArgs e)
	{
		e.Mobile.Target = new MacroCheckTarget();
	}

	private class MacroCheckTarget : Target
	{
		public MacroCheckTarget() : base(-1, true, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is PlayerMobile mobile)
			{
				string text = CheckOpenGump(mobile, from);
				if (text != null)
				{
					from.SendMessage(text);
				}
				else
				{
					CheckPlayer check = new(mobile, from);
				}
			}
		}
	}

	public static string CheckOpenGump(Mobile m, Mobile gm)
	{
		PlayerMobile pm = m as PlayerMobile;
		if (m.NetState == null)
		{
			return "This Player has already logged out.";
		}

		if (gm == m)
		{
			return "You cant send yourself a macro check.";
		}

		if (m.HasGump(typeof(MacroCheckGump)))
		{
			return "This Player Already has one macro chat on the screen!";
		}

		if (pm != null && pm.LastMacroCheck + TimeBetweenMacroChecks > DateTime.UtcNow)
		{
			return
				$"This player already had a macro check you need to wait more {TimeFormat((TimeBetweenMacroChecks - (DateTime.Now - pm.LastMacroCheck)))}.";
		}

		return null;
	}

	public static string TimeFormat(TimeSpan time)
	{
		return $"{time.Minutes} min i {time.Seconds} sek";
	}
}
