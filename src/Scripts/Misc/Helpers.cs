namespace Server.Misc
{
	public class Helpers
	{
		public static readonly int[] Offsets = new int[]
		{
			-1, -1,
			-1,  0,
			-1,  1,
			0, -1,
			0,  1,
			1, -1,
			1,  0,
			1,  1
		};

		private static readonly Point3D[] m_BritanniaLocs = {
			new(1470, 843, 0),
			new(1857, 865, -1),
			new(4220, 563, 36),
			new(1732, 3528, 0),
			new(1300, 644, 8),
			new(3355, 302, 9),
			new(1606, 2490, 5),
			new(2500, 3931, 3),
			new(4264, 3707, 0)
};
		private static readonly Point3D[] m_IllshLocs = {
			new(1222, 474, -17),
			new(718, 1360, -60),
			new(297, 1014, -19),
			new(986, 1006, -36),
			new(1180, 1288, -30),
			new(1538, 1341, -3),
			new(528, 223, -38)
		};
		private static readonly Point3D[] m_MalasLocs = {
			new(976, 517, -30)
		};
		private static readonly Point3D[] m_TokunoLocs = {
			new(710, 1162, 25),
			new(1034, 515, 18),
			new(295, 712, 55)
		};

		public static Point3D GetNearestShrine(Mobile m, ref Map map)
		{
			Point3D[] locList;

			if (map == Map.Felucca || map == Map.Trammel)
				locList = m_BritanniaLocs;
			else if (map == Map.Ilshenar)
				locList = m_IllshLocs;
			else if (map == Map.Tokuno)
				locList = m_TokunoLocs;
			else if (map == Map.Malas)
				locList = m_MalasLocs;
			else
			{
				// No map, lets use trammel
				locList = m_BritanniaLocs;
				map = Map.Trammel;
			}

			Point3D closest = Point3D.Zero;
			double minDist = double.MaxValue;

			for (int i = 0; i < locList.Length; i++)
			{
				Point3D p = locList[i];

				double dist = m.GetDistanceToSqrt(p);
				if (minDist > dist)
				{
					closest = p;
					minDist = dist;
				}
			}

			return closest;
		}
	}
}
