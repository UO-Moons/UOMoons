using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ForkedTongueQuest : BaseQuest
	{
		public ForkedTongueQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(OphidianKnight), "Ophidian Knight-Errants", 10));
			AddObjective(new SlayObjective(typeof(OphidianMage), "Ophidian Mages", 10));

			AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
		}

		public override bool AllObjectives => false;
		/* Forked Tongue */
		public override object Title => 1073655;
		/* I must implore you, brave traveler, to do battle with the vile reptiles which haunt these parts. Those hideous 
        abominations, the Ophidians, are a blight across the land. If you were able to put down a host of the scaly 
        warriors, the Knights or the Avengers, I would forever be in your debt. */
		public override object Description => 1073694;
		/* Perhaps you'll change your mind and return at some point. */
		public override object Refuse => 1073733;
		/* Have you killed the Ophidian Knights or Avengers? */
		public override object Uncomplete => 1073735;
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
