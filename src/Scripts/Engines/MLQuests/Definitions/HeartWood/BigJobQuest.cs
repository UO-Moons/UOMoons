using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class BigJobQuest : BaseQuest
	{
		public BigJobQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Ogre), "Ogres", 5));
			AddObjective(new SlayObjective(typeof(Ettin), "Ettins", 5));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* A Big Job */
		public override object Title => 1072988;
		/* It's a big job but you look to be just the adventurer to do it! I'm so glad you came by ... 
        I'm paying well for the death of five ogres and five ettins.  Hop to it, if you're so inclined. */
		public override object Description => 1073017;
		/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
		public override object Refuse => 1072270;
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
