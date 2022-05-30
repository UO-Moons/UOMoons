using Server.Items;

namespace Server.Engines.Quests
{
	public class WatchYourStepQuest : BaseQuest
	{
		public WatchYourStepQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Gold), "Gold", 5000, 0xEED));

			AddReward(new BaseReward(typeof(MagicalRope), 1074338)); // Magical Rope
		}

		/* Watch Your Step */
		public override object Title => 1074454;
		/* You won't be able to get into the sinkhole with any normal rope.  It's a big drop, sure, but the real problem is the acid 
        and poison gas.  See, most ropes can't stand up to that kind of treatment, even for a short period of time.  So you'll need 
        a special rope -- a magical rope. Like these ones I happen to have available here for a mere 5,000 gold each. */
		public override object Description => 1074455;
		/* I sure don't want to go down there either.  You won't hear me chiding you. In fact, I commend your intellect. */
		public override object Refuse => 1074456;
		/* The price isn't going to change.  It's 5,000 per rope please. */
		public override object Uncomplete => 1074457;
		/* Be careful down there. I don't like losing customers. */
		public override object Complete => 1074458;
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
