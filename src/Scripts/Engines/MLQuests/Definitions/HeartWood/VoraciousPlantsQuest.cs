using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class VoraciousPlantsQuest : BaseQuest
    {
        public VoraciousPlantsQuest()
            : base()
        {
            AddObjective(new SlayObjective(typeof(Corpser), "Corpsers", 8));
            AddObjective(new SlayObjective(typeof(SwampTentacle), "Swamp Tentacles", 2));

            AddReward(new BaseReward(typeof(TrinketBag), 1072341));
        }

        /* Voracious Plants */
        public override object Title => 1073001;
        /* I bet you can't tangle with those nasty plants ... say eight corpsers and two swamp 
        tentacles!  I bet they're too much for you. You may as well confess you can't ...*/
        public override object Description => 1073024;
        /* Hahahaha!  I knew it! */
        public override object Refuse => 1073019;
        /* You're not quite done yet.  Get back to work! */
        public override object Uncomplete => 1072271;
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
