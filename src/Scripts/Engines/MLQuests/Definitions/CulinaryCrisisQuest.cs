using Server.Items;

namespace Server.Engines.Quests
{
	public class CulinaryCrisisQuest : BaseQuest
	{
		public CulinaryCrisisQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Dates), "Bunch of Dates", 20, 0x1727));
			AddObjective(new ObtainObjective(typeof(CheeseWheel), "Wheels of Cheese", 5, 0x97E));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Culinary Crisis */
		public override object Title => 1074755;
		/* You have NO idea how impossible this is.  Simply intolerable!  How can one expect an artiste' like me to 
        create masterpieces of culinary delight without the best, fresh ingredients?  Ever since this whositwhatsit 
        started this uproar, my thrice-daily produce deliveries have ended.  I can't survive another hour without 
        produce! */
		public override object Description => 1074756;
		/* You have no artistry in your soul. */
		public override object Refuse => 1074757;
		/* I must have fresh produce and cheese at once! */
		public override object Uncomplete => 1074758;
		/* Those dates look bruised!  Oh no, and you fetched a soft cheese.  *deep pained sigh*  Well, even I can only 
        do so much with inferior ingredients.  BAM! */
		public override object Complete => 1074759;
		public override bool CanOffer()
		{
			return MondainsLegacy.Bedlam;
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
