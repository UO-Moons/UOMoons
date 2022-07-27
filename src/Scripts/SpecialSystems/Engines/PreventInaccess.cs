using System.Collections.Generic;

namespace Server.Misc
{
	public static class PreventInaccess
	{
		private static readonly bool Enabled = Settings.Configuration.Get<bool>("Misc", "PreventInaccess");

		private static readonly LocationInfo[] m_Destinations = {
				new( new Point3D( 5275, 1163, 0 ), Map.Felucca ), // Jail
				new( new Point3D( 5275, 1163, 0 ), Map.Trammel ),
				new( new Point3D( 5445, 1153, 0 ), Map.Felucca ), // Green acres
				new( new Point3D( 5445, 1153, 0 ), Map.Trammel )
			};

		private static Dictionary<Mobile, LocationInfo> _moveHistory;

		public static void Initialize()
		{
			_moveHistory = new Dictionary<Mobile, LocationInfo>();

			if (Enabled)
				EventSink.OnLogin += OnLogin;
		}

		private static void OnLogin(Mobile from)
		{
			if (from == null || from.AccessLevel < AccessLevel.Counselor)
				return;

			if (HasDisconnected(from))
			{
				if (!_moveHistory.ContainsKey(from))
					_moveHistory[from] = new LocationInfo(from.Location, from.Map);

				LocationInfo dest = GetRandomDestination();

				from.Location = dest.Location;
				from.Map = dest.Map;
			}
			else if (_moveHistory.ContainsKey(from))
			{
				LocationInfo orig = _moveHistory[from];
				from.SendMessage("Your character was moved from {0} ({1}) due to a detected client crash.", orig.Location, orig.Map);

				_moveHistory.Remove(from);
			}
		}

		private static bool HasDisconnected(Mobile m)
		{
			return m.NetState == null || m.NetState.Socket == null;
		}

		private static LocationInfo GetRandomDestination()
		{
			return m_Destinations[Utility.Random(m_Destinations.Length)];
		}

		private class LocationInfo
		{
			public Point3D Location { get; }
			public Map Map { get; }

			public LocationInfo(Point3D loc, Map map)
			{
				Location = loc;
				Map = map;
			}
		}
	}
}
