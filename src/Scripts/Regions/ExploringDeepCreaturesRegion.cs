using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Third;
using System.Linq;
using System.Xml;

namespace Server.Regions
{
    public class ExploringDeepCreaturesRegion : DungeonRegion
    {
        public ExploringDeepCreaturesRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        Mobile creature;

        public override void OnEnter(Mobile m)
        {
            /*if ((m is PlayerMobile) && m.Alive)
            {
                PlayerMobile pm = m as PlayerMobile;

                if (m.Region.Name == "Ice Wyrm" && pm.ExploringTheDeepQuest == ExploringTheDeepQuestChain.CusteauPerron)
                {
                    creature = IceWyrm.Spawn(new Point3D(5805 + Utility.RandomMinMax(-5, 5), 240 + Utility.RandomMinMax(-5, 5), 0), Map.Trammel);
                }
                else if (m.Region.Name == "Mercutio The Unsavory" && pm.ExploringTheDeepQuest == ExploringTheDeepQuestChain.CollectTheComponent)
                {
                    creature = MercutioTheUnsavory.Spawn(new Point3D(2582 + Utility.RandomMinMax(-5, 5), 1118 + Utility.RandomMinMax(-5, 5), 0), Map.Trammel);
                }
                else if (m.Region.Name == "Djinn" && pm.ExploringTheDeepQuest == ExploringTheDeepQuestChain.CollectTheComponent)
                {
                    creature = Djinn.Spawn(new Point3D(1732 + Utility.RandomMinMax(-5, 5), 520 + Utility.RandomMinMax(-5, 5), 8), Map.Ilshenar);
                }
                else if (m.Region.Name == "Obsidian Wyvern" && pm.ExploringTheDeepQuest == ExploringTheDeepQuestChain.CollectTheComponent)
                {
                    creature = ObsidianWyvern.Spawn(new Point3D(5136, 966, 0), Map.Trammel);
                }
                else if (m.Region.Name == "Orc Engineer" && pm.ExploringTheDeepQuest == ExploringTheDeepQuestChain.CollectTheComponent)
                {
                    creature = OrcEngineer.Spawn(new Point3D(5311 + Utility.RandomMinMax(-5, 5), 1968 + Utility.RandomMinMax(-5, 5), 0), Map.Trammel);
                }

                if (creature == null)
                    return;
            }*/
        }
    }
}
