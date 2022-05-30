using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class DustToDustQuest : BaseQuest
    {
        public DustToDustQuest()
            : base()
        {
            AddObjective(new SlayObjective(typeof(EarthElemental), "Earth Elementals", 12));

            AddReward(new BaseReward(typeof(TreasureBag), 1072583));
        }

        /* Dust to Dust */
        public override object Title => 1073074;
        /* You want to hear about trouble? I got trouble. How's angry piles of granite walking around for 
        trouble? Maybe they don't like the mining, maybe it's the farming. I don't know. All I know is 
        someone's got to turn them back to potting soil. And it'd be worth a pretty penny to the soul that 
        does it. */
        public override object Description => 1073564;
        /* I hope you'll reconsider. Until then, farwell. */
        public override object Refuse => 1073580;
        /* You got rocks in your head? I said to kill 12 earth elementals, okay? */
        public override object Uncomplete => 1073584;
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
