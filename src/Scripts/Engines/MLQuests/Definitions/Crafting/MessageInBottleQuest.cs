using Server.Items;

namespace Server.Engines.Quests
{
	public class MessageInBottleQuest : BaseQuest
	{
		public MessageInBottleQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(EmptyBottle), "Empty Bottles", 50, 0xF0E));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* Message in a Bottle */
		public override object Title => 1073894;
		/* We elves are interested in trading our wines with humans but we understand human usually trade such brew in strange transparent 
        bottles. If you could provide some of these empty glass bottles, I might engage in a bit of elven winemaking. */
		public override object Description => 1074084;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me empty bottles. */
		public override object Uncomplete => 1073940;
		/* My thanks for your service.  Here is something for you to enjoy. */
		public override object Complete => 1073971;
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
