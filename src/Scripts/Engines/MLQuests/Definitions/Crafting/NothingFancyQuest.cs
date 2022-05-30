using Server.Items;

namespace Server.Engines.Quests
{
	public class NothingFancyQuest : BaseQuest
	{
		public NothingFancyQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Bascinet), "Bascinets", 15, 0x140C));

			AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
		}

		/* Nothing Fancy */
		public override object Title => 1073911;
		/* I am curious to see the results of human blacksmithing. To examine the care and quality 
        of a simple item. Perhaps, a simple bascinet helmet? Yes, indeed -- if you could bring to 
        me some bascinet helmets, I would demonstrate my gratitude. */
		public override object Description => 1074101;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me bascinets. */
		public override object Uncomplete => 1073957;
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
