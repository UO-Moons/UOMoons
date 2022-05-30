using Server.Items;
using System;

namespace Server.Engines.Quests
{
	public class LethalDartsQuest : BaseQuest
	{
		public LethalDartsQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(CrossBowBolt), "Crossbow Bolts", 10, 0x1BFB));

			AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
		}

		public override TimeSpan RestartDelay => TimeSpan.FromMinutes(2);

		/* Lethal Darts */
		public override object Title => 1073876;
		/* We elves are no strangers to archery but I would be interested in learning whether there 
        is anything to learn from the human approach. I would gladly trade you something I have if 
        you could teach me of the deadly crossbow bolt. */
		public override object Description => 1074066;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me crossbow bolts. */
		public override object Uncomplete => 1073922;
		/* My thanks for your service. Now, I shall teach you of elven archery. */
		public override object Complete => 1073968;
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
