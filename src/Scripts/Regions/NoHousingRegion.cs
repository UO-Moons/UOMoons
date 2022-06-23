using System.Xml;

namespace Server.Regions;

public class NoHousingRegion : BaseRegion
{
	/*  - False: this uses 'OSI' house placement checking: part of the house may be placed here provided that the center is not in the region
	 *  -  True: this uses 'smart UOMoons' house placement checking: no part of the house may be in the region
	 */
	private readonly bool _mSmartChecking;

	public bool SmartChecking => _mSmartChecking;

	public NoHousingRegion(XmlElement xml, Map map, Region parent) : base(xml, map, parent)
	{
		ReadBoolean(xml["smartNoHousing"], "active", ref _mSmartChecking, false);
	}

	public override bool AllowHousing(Mobile from, Point3D p)
	{
		return _mSmartChecking;
	}
}
