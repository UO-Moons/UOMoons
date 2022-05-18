using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.TownHouses
{
	public class Errors
	{
		private static readonly List<string> s_ErrorLog = new List<string>();
		private static readonly List<Mobile> s_Checked = new List<Mobile>();

		public static List<string> ErrorLog => s_ErrorLog;

		public static List<Mobile> Checked => s_Checked;

		public static void Initialize()
		{
			VersionCommand.AddCommand("TownHouseErrors", AccessLevel.Counselor, OnErrors);
			VersionCommand.AddCommand("the", AccessLevel.Counselor, OnErrors);

			EventSink.OnLogin += OnLogin;
		}

		private static void OnErrors(CommandInfo e)
		{
			if (string.IsNullOrEmpty(e.ArgString))
			{
				_ = new ErrorsGump(e.Mobile);
			}
			else
			{
				Report(e.ArgString + " - " + e.Mobile.Name);
			}
		}

		private static void OnLogin(Mobile m)
		{
			if (m.AccessLevel != AccessLevel.Player
				&& s_ErrorLog.Count != 0
				&& !s_Checked.Contains(m))
			{
				new ErrorsNotifyGump(m);
			}
		}

		public static void Report(string error)
		{
			s_ErrorLog.Add(string.Format("<B>{0}</B><BR>{1}<BR>", DateTime.UtcNow, error));

			s_Checked.Clear();

			Notify();
		}

		private static void Notify()
		{
			foreach (NetState state in NetState.Instances.Where(state => state.Mobile != null).Where(state => state.Mobile.AccessLevel != AccessLevel.Player))
			{
				Notify(state.Mobile);
			}
		}

		private static void Notify(Mobile m)
		{
			if (m.HasGump(typeof(ErrorsGump)))
			{
				_ = new ErrorsGump(m);
			}
			else
			{
				_ = new ErrorsNotifyGump(m);
			}
		}
	}
}
