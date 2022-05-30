using Server.Items;

namespace Server.Engines.Quests
{
	public class TheFarEyeQuest : BaseQuest
	{
		public TheFarEyeQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Spyglass), "Spyglasses", 20, 0x14F5));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* The Far Eye */
		public override object Title => 1073908;
		/* The wonders of human invention! Turning sand and metal into a far-seeing eye! This is 
        something I must experience for myself. Bring me some of these spyglasses friend human. */
		public override object Description => 1074098;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me spyglasses. */
		public override object Uncomplete => 1073954;
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
