using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class WildBoarCullQuest : BaseQuest
    {
        public WildBoarCullQuest()
            : base()
        {
            AddObjective(new SlayObjective(typeof(Boar), "Boars", 10));

            AddReward(new BaseReward(typeof(SmallTrinketBag), 1072268));
        }

        /* Wild Boar Cull */
        public override object Title => 1072245;
        /* A pity really.  With the balance of nature awry, we have no choice but to accept 
        the responsibility of making it all right.  It's all a part of the circle of life, 
        after all. So, yes, the boars are running rampant. There are far too many in the 
        region.  Will you shoulder your obligations as a higher life form? */
        public override object Description => 1072260;
        /* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
        public override object Refuse => 1072270;
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete => 1072271;
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
        }
    }
}
