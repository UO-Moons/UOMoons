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

		private static readonly Point3D[] m_BritanniaLocs = new Point3D[]
{
			new Point3D(1470, 843, 0),
			new Point3D(1857, 865, -1),
			new Point3D(4220, 563, 36),
			new Point3D(1732, 3528, 0),
			new Point3D(1300, 644, 8),
			new Point3D(3355, 302, 9),
			new Point3D(1606, 2490, 5),
			new Point3D(2500, 3931, 3),
			new Point3D(4264, 3707, 0)
};
		private static readonly Point3D[] m_IllshLocs = new Point3D[]
		{
			new Point3D(1222, 474, -17),
			new Point3D(718, 1360, -60),
			new Point3D(297, 1014, -19),
			new Point3D(986, 1006, -36),
			new Point3D(1180, 1288, -30),
			new Point3D(1538, 1341, -3),
			new Point3D(528, 223, -38)
		};
		private static readonly Point3D[] m_MalasLocs = new Point3D[]
		{
			new Point3D(976, 517, -30)
		};
		private static readonly Point3D[] m_TokunoLocs = new Point3D[]
		{
			new Point3D(710, 1162, 25),
			new Point3D(1034, 515, 18),
			new Point3D(295, 712, 55)
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
