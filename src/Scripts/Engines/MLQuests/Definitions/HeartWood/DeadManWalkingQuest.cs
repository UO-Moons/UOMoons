using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class DeadManWalkingQuest : BaseQuest
{
	public DeadManWalkingQuest()
	{
		AddObjective(new SlayObjective(typeof(Zombie), "Zombies", 5));
		AddObjective(new SlayObjective(typeof(Skeleton), "Skeletons", 5));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Dead Man Walking */
	public override object Title => 1072983;
	/* Why?  I ask you why?  They walk around after they're put in the ground.  It's just wrong in so many ways.  
    Put them to proper rest, I beg you.  I'll find some way to pay you for the kindness. Just kill five zombies 
    and five skeletons. */
	public override object Description => 1073009;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
