using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class SomethingToWailAboutQuest : BaseQuest
	{
		public SomethingToWailAboutQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(WailingBanshee), "Wailing Banshees", 12));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Something to Wail About */
		public override object Title => 1073071;
		/* Can you hear them? The never-ending howling? The incessant wailing? These banshees, they never cease! Never! They haunt 
        my nights. Please, I beg you -- will you silence them? I would be ever so grateful. */
		public override object Description => 1073561;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* Until you kill 12 Wailing Banshees, there will be no peace. */
		public override object Uncomplete => 1073581;
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
