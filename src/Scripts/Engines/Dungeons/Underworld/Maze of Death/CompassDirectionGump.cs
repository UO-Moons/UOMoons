using System.Collections.Generic;

namespace Server.Gumps;

public class CompassDirectionGump : Gump
{
	public CompassDirectionGump(IEntity from)
		: base(100, 100)
	{
		List<Point2D> pointList = Regions.MazeOfDeathRegion.Path;

		Point2D cur = new(from.Location.X, from.Location.Y);
		Point2D northLoc = new(cur.X, cur.Y - 1);
		Point2D eastLoc = new(cur.X + 1, cur.Y);
		Point2D southLoc = new(cur.X, cur.Y + 1);
		Point2D westLoc = new(cur.X - 1, cur.Y);
            
		//Empty radar
		AddImage(0, 0, 9007);
		AddAlphaRegion(0, 0, 200, 200);

		//Arrows
		if (pointList.Contains(northLoc))
			AddImage(100, 50, 4501);

		if (pointList.Contains(eastLoc))
			AddImage(100, 100, 4503);

		if (pointList.Contains(southLoc))
			AddImage(50, 100, 4505);

		if (pointList.Contains(westLoc))
			AddImage(50, 50, 4507);
	}
}
