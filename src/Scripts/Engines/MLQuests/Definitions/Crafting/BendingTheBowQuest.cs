using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class BendingTheBowQuest : BaseQuest
    {
        public BendingTheBowQuest()
            : base()
        {
            AddObjective(new ObtainObjective(typeof(Bow), "Bows", 10, 0x13B2));

            AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
        }

        /* Bending the Bow */
        public override object Title => 1074019;
        /* Human craftsmanship! Ha! Why, take an elven bow. It will last for a lifetime, never break and 
        always shoot an arrow straight and true. Can't say the same for a human, can you? Bring me some 
        of these human made bows, and I will show you. */
        public override object Description => 1074113;
        /* Fine then, I'm shall find another to run my errands then. */
        public override object Refuse => 1074063;
        /* Hurry up! I don't have all day to wait for you to bring what I desire! */
        public override object Uncomplete => 1074064;
        /* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
        public override object Complete => 1074065;
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
