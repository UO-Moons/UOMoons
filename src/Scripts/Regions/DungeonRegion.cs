using System.Xml;

namespace Server.Regions;

public class DungeonRegion : BaseRegion
{
	public override bool YoungProtected => false;

	private Point3D _mEntranceLocation;

	public Point3D EntranceLocation { get => _mEntranceLocation; set => _mEntranceLocation = value; }
	public Map EntranceMap { get; set; }

	public DungeonRegion(XmlElement xml, Map map, Region parent) : base(xml, map, parent)
	{
		XmlElement entrEl = xml["entrance"];

		Map entrMap = map;
		ReadMap(entrEl, "map", ref entrMap, false);

		if (ReadPoint3D(entrEl, entrMap, ref _mEntranceLocation, false))
			EntranceMap = entrMap;
	}

	public override bool AllowHousing(Mobile from, Point3D p)
	{
		return false;
	}

	public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
	{
		global = LightCycle.DungeonLevel;
	}

	public override bool CanUseStuckMenu(Mobile m)
	{
		if (Map == Map.Felucca)
			return false;

		return base.CanUseStuckMenu(m);
	}
}
