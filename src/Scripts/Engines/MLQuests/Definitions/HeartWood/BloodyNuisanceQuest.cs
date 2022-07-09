using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class BloodyNuisanceQuest : BaseQuest
{
	public BloodyNuisanceQuest()
	{
		AddObjective(new SlayObjective(typeof(GoreFiend), "Gore Fiends", 10));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Bloody Nuisance */
	public override object Title => 1072992;
	/* I bet you can't kill ... ten gore fiends!  I bet they're too much 
    for you.  You may as well confess you can't ... */
	public override object Description => 1073021;
	/* Hahahaha!  I knew it! */
	public override object Refuse => 1073019;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
