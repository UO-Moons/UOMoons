namespace Server.Engines.BulkOrders
{
	public class BodSystem
	{
		public static bool Enabled => Settings.Configuration.Get<bool>("Misc", "BODEnabled");

	}
}
