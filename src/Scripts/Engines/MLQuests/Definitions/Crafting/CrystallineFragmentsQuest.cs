using Server.Items;

namespace Server.Engines.Quests
{
	public class CrystallineFragmentsQuest : BaseQuest
	{
		public CrystallineFragmentsQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(CrystallineFragments), "Crystalline Fragments", 10));

			AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
		}

		/* Crystalline Fragments */
		public override object Title => 1073054;
		/* You look strong and brave, my friend.  Are you strong and brave?  I only ask because I am known 
        to be too generous to those that find for me interesting -- things -- to use in my smithing.  What 
        do you say? */
		public override object Description => 1074662;
		/* *nods* */
		public override object Refuse => 1074663;
		/* I can't be generous, my friend, until you bring me those crystalline fragments. */
		public override object Uncomplete => 1074665;
		/* My friend, you've returned -- with items for me, I hope?  I have a generous reward for you. */
		public override object Complete => 1074667;
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
