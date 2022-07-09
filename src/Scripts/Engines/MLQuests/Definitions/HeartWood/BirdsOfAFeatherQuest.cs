using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class BirdsOfAFeatherQuest : BaseQuest
{
	public BirdsOfAFeatherQuest()
	{
		AddObjective(new SlayObjective(typeof(Harpy), "Harpies", 10));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Birds of a Feather */
	public override object Title => 1073007;
	/* I bet you can't kill ... ten harpies!  I bet they're too much 
    for you.  You may as well confess you can't ... */
	public override object Description => 1073022;
	/* Hahahaha!  I knew it! */
	public override object Refuse => 1073019;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
