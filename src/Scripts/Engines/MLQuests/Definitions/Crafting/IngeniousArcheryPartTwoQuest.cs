using Server.Items;

namespace Server.Engines.Quests;

public sealed class IngeniousArcheryPartTwoQuest : BaseQuest
{
	public IngeniousArcheryPartTwoQuest()
	{
		AddObjective(new ObtainObjective(typeof(HeavyCrossbow), "Heavy Crossbows", 8, 0x13FD));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Ingenious Archery, Part II */
	public override object Title => 1073879;
	/* These human "crossbows" are complex and clever. The "heavy crossbow" is a remarkable 
    instrument of war. I am interested in seeing one up close, if you could arrange for one 
    to make its way to my hands. */
	public override object Description => 1074069;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me heavy crossbows. */
	public override object Uncomplete => 1073925;
	/* My thanks for your service. Now, I shall teach you of elven archery. */
	public override object Complete => 1073968;
}
