using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class OverpopulationQuest : BaseQuest
    {
        public OverpopulationQuest()
            : base()
        {
            AddObjective(new SlayObjective(typeof(Hind), "Hinds", 10));

            AddReward(new BaseReward(typeof(SmallTrinketBag), 1072268));
        }

        /* Overpopulation */
        public override object Title => 1072252;
        /* I just can't bear it any longer.  Sure, it's my job to thin the deer out so 
        they don't overeat the area and starve themselves come winter time.  Sure, I 
        know we killed off the predators that would do this naturally so now we have 
        to make up for it.  But they're so graceful and innocent.  I just can't do it.  
        Will you? */
        public override object Description => 1072267;
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
