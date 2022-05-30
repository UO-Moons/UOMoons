using Server.Items;

namespace Server.Engines.Quests
{
	public class WaitingToBeFilledQuest : BaseQuest
	{
		public WaitingToBeFilledQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(EmptyBottle), "Empty Bottles", 20, 0xF0E));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* Waiting to be Filled */
		public override object Title => 1074036;
		/* The only good thing I can say about human made bottles is that they are empty and may yet still be filled 
        with elven wine. Go now, fetch a number of empty bottles so that I might save them from the fate of carrying 
        human-made wine. */
		public override object Description => 1074130;
		/* Fine then, I'm shall find another to run my errands then. */
		public override object Refuse => 1074063;
		/* Hurry up! I don't have all day to wait for you to bring what I desire! */
		public override object Uncomplete => 1074064;
		/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
		public override object Complete => 1074065;
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
