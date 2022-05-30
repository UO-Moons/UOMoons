using Server.Items;

namespace Server.Engines.Quests
{
	public class SeasonsQuest : BaseQuest
	{
		public SeasonsQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(RawFishSteak), "Raw Fish Steaks", 20, 0x097A));

			AddReward(new BaseReward(1072803)); // The boon of Maul.
		}

		public override bool ForceRemember => true;
		/* Seasons */
		public override object Title => 1072782;
		/* *rumbling growl* *sniff* ... not-smell ... seek-fight ... not-smell ... fear-stench ... *rumble* ... cold-soon-time 
        comes ... hungry ... eat-fish ... sleep-soon-time ... *deep fang-filled yawn* ... much-fish. */
		public override object Description => 1072802;
		/* *yawn* ... cold-soon-time ... *growl* */
		public override object Refuse => 1072810;
		/* *sniff* *sniff* ... not-much-fish ... hungry ... *grumble* */
		public override object Uncomplete => 1072811;
		/* *sniff* fish! much-fish! */
		public override object Complete => 1074174;
		public override void GiveRewards()
		{
			base.GiveRewards();

			Owner.SendLocalizedMessage(1074940, null, 0x23); // You have gained the boon of Maul!  Your understanding of the seasons grows.  You are one step closer to claiming your elven heritage.
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
