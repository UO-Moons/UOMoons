using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class SquishyQuest : BaseQuest
{
	public SquishyQuest()
	{
		AddObjective(new SlayObjective(typeof(Slime), "Slimes", 12));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Squishy */
	public override object Title => 1072998;
	/* Have you ever seen what a slime can do to good gear?  Well, it's not pretty, let me tell 
    you!  If you take on my task to destroy twelve of them, bear that in mind.  They'll corrode 
    your equipment faster than anything. */
	public override object Description => 1073031;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
