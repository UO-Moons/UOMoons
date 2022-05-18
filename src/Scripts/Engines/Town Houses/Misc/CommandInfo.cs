namespace Server.Engines.TownHouses
{
	public delegate void TownHouseCommandHandler(CommandInfo info);

	public class CommandInfo
	{
		public Mobile Mobile { get; }

		public string Command { get; }

		public string ArgString { get; }

		public string[] Arguments { get; }

		public CommandInfo(Mobile m, string com, string args, string[] arglist)
		{
			Mobile = m;
			Command = com;
			ArgString = args;
			Arguments = arglist;
		}

		public string GetString(int num)
		{
			return Arguments.Length > num ? Arguments[num] : "";
		}
	}
}
