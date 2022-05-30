using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ImpishDelightsQuest : BaseQuest
	{
		public ImpishDelightsQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Imp), "Imps", 12));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Impish Delights */
		public override object Title => 1073077;
		/* Imps! Do you hear me? Imps! They're everywhere! They're in everything! Oh, don't be fooled 
        by their size - they vicious little devils! Half-sized evil incarnate, they are! Somebody 
        needs to send them back to where they came from, if you know what I mean. */
		public override object Description => 1073567;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* Don't let the little devils scare you! You  kill 12 imps - then we'll talk reward. */
		public override object Uncomplete => 1073587;
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
