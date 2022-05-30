using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class NewLeadershipQuest : BaseQuest
    {
        public NewLeadershipQuest()
            : base()
        {
            AddObjective(new SlayObjective(typeof(SerpentsFangHighExecutioner), "Serpent's Fang High Executioner", 1, "TheCitadel"));
            AddObjective(new SlayObjective(typeof(TigersClawThief), "Tiger's Claw Thief", 1, "TheCitadel"));
            AddObjective(new SlayObjective(typeof(DragonsFlameGrandMage), "Dragon's Flame Mage", 1, "TheCitadel"));

            AddReward(new BaseReward(typeof(RewardBox), 1072584));
        }

        /* New Leadership */
        public override object Title => 1072905;
        /* I have a task for you ... adventurer.  Will you risk all to win great renown?  The 
        Black Order is organized into three sects, each with their own speciality.  The Dragon's 
        Flame serves the will of the Grand Mage, the Tiger's Claw answers to the Master Thief, 
        and the Serpent's Fang kills at the direction of the High Executioner.  Slay all three 
        and you will strike the order a devastating blow! */
        public override object Description => 1072963;
        /* I do not fault your decision. */
        public override object Refuse => 1072973;
        /* Once you gain entrance into The Citadel, you will need to move cautiously to find 
        the sect leaders. */
        public override object Uncomplete => 1072974;
        public override bool CanOffer()
        {
            return MondainsLegacy.Citadel;
        }

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
