using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class BeerGogglesQuest : BaseQuest
    {
        public BeerGogglesQuest()
            : base()
        {
            AddObjective(new ObtainObjective(typeof(BarrelTap), "Barrel Tap", 25, 0x1004));

            AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
        }

        /* Beer Goggles */
        public override object Title => 1073895;
        /* Oh, the deviltry! Why would humans lock their precious liquors inside a wooden coffin? I understand I need a "keg tap" 
        to access the golden brew within such a wooden abomination. Perhaps, if you could bring me such a tap, we could share a 
        drink and I could teach you. */
        public override object Description => 1074085;
        /* I will patiently await your reconsideration. */
        public override object Refuse => 1073921;
        /* I will be in your debt if you bring me barrel taps. */
        public override object Uncomplete => 1073941;
        /* My thanks for your service.  Here is something for you to enjoy. */
        public override object Complete => 1073971;
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
