using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class TheBalanceOfNatureQuest : BaseQuest
	{
		public TheBalanceOfNatureQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(TimberWolf), "Timber Wolves", 15, "Huntsman's Forest"));

			AddReward(new BaseReward(1072807)); // The boon of the Huntsman.
		}

		public override bool ForceRemember => true;
		/* The Balance of Nature */
		public override object Title => 1072786;
		/* Ho, there human.  Why do you seek out the Huntsman?  The hunter serves the land by culling both predators and prey.  The hunter 
        maintains the essential balance of life and does not kill for sport or glory.  If you seek my favor, human, then demonstrate you 
        are capable of the duty.  Cull the wolves nearby. */
		public override object Description => 1072829;
		/* Then begone. I have no time to waste on you, human. */
		public override object Refuse => 1072830;
		/* The timber wolves are easily tracked, human. */
		public override object Uncomplete => 1072831;
		public override void GiveRewards()
		{
			base.GiveRewards();

			Owner.SendLocalizedMessage(1074943, null, 0x23); // You have gained the boon of the Huntsman!  You have been given a taste of the bittersweet duty of those who guard the balance.  You are one step closer to claiming your elven heritage.
		}

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
