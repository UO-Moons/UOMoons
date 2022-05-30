using Server.Items;

namespace Server.Engines.Quests
{
	public class InTheBellyOfTheBeastQuest : BaseQuest
	{
		public InTheBellyOfTheBeastQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(LuckyDagger), "Lucky Dagger", 1));

			AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
		}

		/* In the Belly of the Beast */
		public override object Title => 1073049;
		/* Oh, the trauma!  *weeps loudly*  My lucky dagger has been lost.  It was given to me by my father, as a 
        final gift before he died.  That blade has been an heirloom of my family for generations.  I must have it 
        back.  *sniffles pathetically*  Please, find my lucky dagger. */
		public override object Description => 1074658;
		/* *wailing cries* Then begone if you will not help a poor man in need. */
		public override object Refuse => 1074659;
		/* *sniffles*  The dagger was stolen by some dishonest man.  Or perhaps I dropped it.  That doesn't matter 
        though.  All that matters is that you find my dagger and return it. */
		public override object Uncomplete => 1074660;
		/* You've found it?  My lucky dagger! */
		public override object Complete => 1074661;
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
