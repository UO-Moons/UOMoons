using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class CreepyCrawliesQuest : BaseQuest
	{
		public CreepyCrawliesQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(GiantSpider), "Giant Spiders", 12));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Creepy Crawlies */
		public override object Title => 1072987;
		/* Disgusting!  The way they scuttle on those hairy legs just makes me want to gag. I hate 
        spiders!  Rid the world of twelve and I'll find something nice to give you in thanks. */
		public override object Description => 1073016;
		/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
		public override object Refuse => 1072270;
		/* You're not quite done yet.  Get back to work! */
		public override object Uncomplete => 1072271;
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
