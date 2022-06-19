using Server.Items;

namespace Server.Engines.Quests;

public sealed class BuildingTheBetterCrossbowQuest : BaseQuest
{
	public BuildingTheBetterCrossbowQuest()
	{
		AddObjective(new ObtainObjective(typeof(RepeatingCrossbow), "Repeating Crossbow", 10, 0x26C3));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Building the Better Crossbow */
	public override object Title => 1074022;
	/* More is always better for a human, eh? Take these repeating crossbows. What sort of mind invents such a thing? 
    I must look at it more closely. Bring such a contraption to me and you'll receive a token for your efforts. */
	public override object Description => 1074116;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}
