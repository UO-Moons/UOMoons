namespace Server.Targeting;

public class LandTarget : IPoint3D
{
	private Point3D _location;

	public LandTarget(Point3D location, Map map)
	{
		_location = location;

		if (map != null)
		{
			_location.Z = map.GetAverageZ(_location.X, _location.Y);
			TileID = map.Tiles.GetLandTile(_location.X, _location.Y).Id & TileData.MaxLandValue;
		}
	}

	[CommandProperty(AccessLevel.Counselor)]
	public string Name => TileData.LandTable[TileID].Name;

	[CommandProperty(AccessLevel.Counselor)]
	public TileFlag Flags => TileData.LandTable[TileID].Flags;

	[CommandProperty(AccessLevel.Counselor)]
	public int TileID { get; }

	[CommandProperty(AccessLevel.Counselor)]
	public Point3D Location => _location;

	[CommandProperty(AccessLevel.Counselor)]
	public int X => _location.X;

	[CommandProperty(AccessLevel.Counselor)]
	public int Y => _location.Y;

	[CommandProperty(AccessLevel.Counselor)]
	public int Z => _location.Z;
}
