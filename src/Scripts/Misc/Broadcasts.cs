namespace Server.Misc
{
	public class Broadcasts
	{
		public static readonly bool LoginEnabled = Settings.Configuration.Get<bool>("Misc", "LoginBroadcast");
		public static readonly bool LogoutEnabled = Settings.Configuration.Get<bool>("Misc", "LogoutBroadcast");
		public static readonly bool NewPlayerEnabled = Settings.Configuration.Get<bool>("Misc", "NewPlayerBroadcast");

		//{0} is the name of the player
		private readonly static string LoginMessage = "{0} has logged in.";//Login Message
		private readonly static string LogoutMessage = "{0} has logged out.";//Logout Message
		private readonly static int LoginHue = 0x482;//Login Message Hue
		private readonly static int LogoutHue = 0x482;//Logout Message Hue
		private readonly static string NewPlayerMessage = "Welcome the newest member of our shard, {0}!"; //New Player Message
		private readonly static int NewPlayerHue = 33; //New Player Message Hue													 
		private static readonly AccessLevel AnnounceLevel = AccessLevel.Player;//maximum access level to announce

		public static void Initialize()
		{
			if (LoginEnabled)
			{
				EventSink.OnLogin += EventSink_Login;
			}

			if (LogoutEnabled)
			{
				EventSink.OnLogout += EventSink_Logout;
			}

			if (NewPlayerEnabled)
			{
				EventSink.CharacterCreated += EventSink_CharacterCreated;
			}
			EventSink.OnCrashed += EventSink_Crashed;
			EventSink.OnShutdown += EventSink_Shutdown;
		}

		public static void EventSink_Logout(Mobile m)
		{
			if (m.Player)
			{
				if (m.AccessLevel <= AnnounceLevel)
					World.Broadcast(LogoutHue, true, string.Format(LogoutMessage, m.Name));
				else //broadcast any other level to the staff
					World.Broadcast(LogoutHue, true, string.Format(LogoutMessage, m.Name));
			}
		}

		public static void EventSink_Login(Mobile m)
		{
			if (m.Player)
			{
				if (m.AccessLevel <= AnnounceLevel)
					World.Broadcast(LoginHue, true, string.Format(LoginMessage, m.Name));
				else //broadcast any other level to the staff
					World.Broadcast(LoginHue, true, string.Format(LoginMessage, m.Name));
			}
		}
		/// <summary> 
		/// On new player login, broadcast a message.
		/// </summary>
		public static void EventSink_CharacterCreated(CharacterCreatedEventArgs e)
		{
			if (e.Mobile != null)
			{
				if (e.Mobile.AccessLevel == AccessLevel.Player)
				{
					World.Broadcast(NewPlayerHue, true, string.Format(NewPlayerMessage, e.Mobile.Name));
				}
			}
		}

		public static void EventSink_Crashed(CrashedEventArgs e)
		{
			try
			{
				World.Broadcast(0x35, true, "The server has crashed.");
			}
			catch
			{
			}
		}

		public static void EventSink_Shutdown()
		{
			try
			{
				World.Broadcast(0x35, true, "The server has shut down.");
			}
			catch
			{
			}
		}
	}
}
