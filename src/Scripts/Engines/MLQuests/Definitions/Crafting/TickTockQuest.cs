using Server.Items;

namespace Server.Engines.Quests
{
	public class TickTockQuest : BaseQuest
	{
		public TickTockQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Clock), "Clock", 10, 0x104B));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* Tick Tock */
		public override object Title => 1073907;
		/* Elves find it remarkable the human preoccupation with the passage of time. To have built instruments to try and 
        capture time -- it is a fascinating notion. I would like to see how a clock is put together. Maybe you could provide 
        some clocks for my experimentation? */
		public override object Description => 1074097;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me clocks. */
		public override object Uncomplete => 1073953;
		/* Enjoy my thanks for your service. */
		public override object Complete => 1073978;
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
