using Server.Items;
using System;

namespace Server.Engines.Quests;

public class ChopChopOnTheDoubleQuest : BaseQuest
{
	public ChopChopOnTheDoubleQuest()
	{
		AddObjective(new ObtainObjective(typeof(Log), "Log", 60, 0x1BDD));

		AddReward(new BaseReward(typeof(LumberjacksSatchel), 1074282)); // Craftsman's Satchel
	}

	public override TimeSpan RestartDelay => TimeSpan.FromMinutes(3);
	/* Chop Chop, On The Double! */
	public override object Title => 1075537;
	/* That's right, move it! I need sixty logs on the double, and they need to be freshly cut! If you can get them to 
    me fast I'll have your payment in your hands before you have the scent of pine out from beneath your nostrils. Just 
    get a sharp axe and hack away at some of the trees in the land and your lumberjacking skill will rise in no time. */
	public override object Description => 1075538;
	/* Or perhaps you'd rather not. */
	public override object Refuse => 1072981;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
	/* Ahhh! The smell of fresh cut lumber. And look at you, all strong and proud, as if you had done an honest days work! */
	public override object Complete => 1075539;
}
