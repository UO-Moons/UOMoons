using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ItsGhastlyJobQuest : BaseQuest
{
	public ItsGhastlyJobQuest()
	{
		AddObjective(new SlayObjective(typeof(Ghoul), "Ghouls", 12));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* It's a Ghastly Job */
	public override object Title => 1073008;
	/* Why?  I ask you why?  They walk around after they're put in the ground.  It's just wrong in so many ways.  
    Put them to proper rest, I beg you.  I'll find some way to pay you for the kindness. Just kill twelve ghouls. */
	public override object Description => 1073012;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
