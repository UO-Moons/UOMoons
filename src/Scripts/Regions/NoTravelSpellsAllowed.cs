using System.Xml;

namespace Server.Regions
{
	public class NoTravelSpellsAllowed : DungeonRegion
    {
        public NoTravelSpellsAllowed(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool CheckTravel(Mobile m, Point3D newLocation, Spells.TravelCheckType travelType)
        {
            return false;
        }
    }
}
