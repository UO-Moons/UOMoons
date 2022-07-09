using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ColdHeartedQuest : BaseQuest
{
	public ColdHeartedQuest()
	{
		AddObjective(new SlayObjective(typeof(IceSerpent), "Giant Ice Serpents", 6));
		AddObjective(new SlayObjective(typeof(FrostSpider), "Frost Spiders", 6));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Cold Hearted */
	public override object Title => 1072991;
	/* It's a big job but you look to be just the adventurer to do it! I'm so glad you came by ... I'm paying 
    well for the death of six giant ice serpents and six frost spiders.  Hop to it, if you're so inclined. */
	public override object Description => 1073027;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
