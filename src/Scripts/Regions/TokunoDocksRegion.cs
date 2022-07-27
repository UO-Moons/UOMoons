using System.Xml;

namespace Server.Regions;

public class TokunoDocksRegion : GuardedRegion
{
	public static TokunoDocksRegion Instance { get; private set; }

	public TokunoDocksRegion(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{
		Instance = this;
	}

	public override bool AllowHousing(Mobile from, Point3D p)
	{
		return false;
	}
}
