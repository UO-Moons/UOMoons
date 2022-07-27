using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Third;
using System.Xml;

namespace Server.Regions
{
	public class CusteauPerronHouseRegion : GuardedRegion
    {
        public CusteauPerronHouseRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool OnBeginSpellCast(Mobile from, ISpell s)
        {
	        if ((s is not TeleportSpell && s is not GateTravelSpell && s is not RecallSpell && s is not MarkSpell &&
	             s is not SacredJourneySpell) || !from.IsPlayer())
		        return base.OnBeginSpellCast(from, s);

	        from.SendLocalizedMessage(500015); // You do not have that spell!
	        return false;

        }
    }
}
