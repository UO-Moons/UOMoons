using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class EvilEyeQuest : BaseQuest
	{
		public EvilEyeQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Gazer), "Gazers", 12));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Evil Eye */
		public override object Title => 1073084;
		/* Kind traveler, hear my plea. You know of the evil orbs? The wrathful eyes? Some call 
        them gazers? They must be a nest nearby, for they are tormenting us poor folk. We need 
        to drive back their numbers. But we are not strong enough to face such horrors ourselves, 
        we need a true hero. */
		public override object Description => 1073574;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* Have you annihilated a dozen Gazers yet, kind traveler? */
		public override object Uncomplete => 1073594;
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
