using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.TownHouses
{
	public class Errors
	{
		public static List<string> ErrorLog { get; } = new();
		public static List<Mobile> Checked { get; } = new();

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
				&& ErrorLog.Count != 0
				&& !Checked.Contains(m))
			{
				_ = new ErrorsNotifyGump(m);
			}
		}

		public static void Report(string error)
		{
			ErrorLog.Add($"<B>{DateTime.UtcNow}</B><BR>{error}<BR>");
			Checked.Clear();
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
