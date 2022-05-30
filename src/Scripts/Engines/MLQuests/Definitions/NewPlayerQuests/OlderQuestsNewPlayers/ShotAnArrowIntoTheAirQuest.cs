using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ShotAnArrowIntoTheAirQuest : BaseQuest
	{
		public ShotAnArrowIntoTheAirQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Sheep), "Sheep", 10));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341)); // A bag of trinkets.
		}

		/*  I Shot an Arrow Into the Air... */
		public override object Title => 1075486;
		/* Truth be told, the only way to get a feel for the bow is to shoot one and there's no better practice target than a 
        sheep. If ye can shoot ten of them I think ye will have proven yer abilities. Just grab a bow and make sure to take 
        enough ammunition. Bows tend to use arrows and crossbows use bolts. Ye can buy 'em or have someone craft 'em. How 
        about it then? Come back here when ye are done. */
		public override object Description => 1075482;
		/* Fair enough, the bow isn't for everyone. Good day then. */
		public override object Refuse => 1075483;
		/* Return once ye have killed ten sheep with a bow and not a moment before. */
		public override object Uncomplete => 1075484;
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
