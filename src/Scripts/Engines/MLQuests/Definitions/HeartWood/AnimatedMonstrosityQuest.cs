using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class AnimatedMonstrosityQuest : BaseQuest
	{
		public AnimatedMonstrosityQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(FleshGolem), "Flesh Golems", 12));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Animated Monstrosity */
		public override object Title => 1072990;
		/* I bet you can't kill ... say twelve ... flesh golems!  I bet 
        they're too much for you.  You may as well confess you can't ... */
		public override object Description => 1073020;
		/* Hahahaha!  I knew it! */
		public override object Refuse => 1073019;
		/* You're not quite done yet.  Get back to work! */
		public override object Uncomplete => 1072271;
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
