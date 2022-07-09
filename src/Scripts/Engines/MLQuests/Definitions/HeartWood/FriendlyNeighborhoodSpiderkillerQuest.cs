using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class FriendlyNeighborhoodSpiderkillerQuest : BaseQuest
{
	public FriendlyNeighborhoodSpiderkillerQuest()
	{
		AddObjective(new SlayObjective(typeof(DreadSpider), "Dread Spiders", 8));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Friendly Neighborhood Spider-killer */
	public override object Title => 1073662;
	/* They aren't called Dread Spiders because they're fluffy and cuddly now, are they? No, there's nothing 
    appealing about those wretches so I sure wouldn't lose any sleep if you were to exterminate a few. I'd 
    even part with a generous amount of gold, I would. */
	public override object Description => 1073701;
	/* Perhaps you'll change your mind and return at some point. */
	public override object Refuse => 1073733;
	/* Dread Spiders? I say keep exterminating the arachnid vermin. */
	public override object Uncomplete => 1073742;
}
