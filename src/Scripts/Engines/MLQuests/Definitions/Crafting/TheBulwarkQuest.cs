using Server.Items;

namespace Server.Engines.Quests
{
	public class TheBulwarkQuest : BaseQuest
	{
		public TheBulwarkQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(HeaterShield), "Heater Shields", 10, 0x1B76));

			AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
		}

		/* The Bulwark */
		public override object Title => 1073912;
		/* The clank of human iron and steel is strange to elven ears. For instance, the metallic heater shield 
        which human warriors carry into battle. It is odd to an elf, but nevertheless intriguing. Tell me friend, 
        could you bring me such an example of human smithing skill? */
		public override object Description => 1074102;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me heater shields. */
		public override object Uncomplete => 1073958;
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
