using Server.Items;

namespace Server.Engines.Quests
{
	public class StopHarpingOnMeQuest : BaseQuest
	{
		public StopHarpingOnMeQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(LapHarp), "Lap Harp", 20, 0xEB2));

			AddReward(new BaseReward(typeof(CarpentersCraftsmanSatchel), 1074282));
		}

		/* Stop Harping on Me */
		public override object Title => 1073881;
		/* Humans artistry can be a remarkable thing. For instance, I have heard of a wonderful 
        instrument which creates the most melodious of music. A lap harp. I would be ever so 
        grateful if I could examine one in person. */
		public override object Description => 1074071;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me lap harp. */
		public override object Uncomplete => 1073927;
		/* My thanks for your service. Now, I will show you something of elven carpentry. */
		public override object Complete => 1073969;
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
