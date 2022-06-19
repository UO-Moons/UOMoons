using Server.Items;

namespace Server.Engines.Quests;

public sealed class IngeniousArcheryPartOneQuest : BaseQuest
{
	public IngeniousArcheryPartOneQuest()
	{
		AddObjective(new ObtainObjective(typeof(Crossbow), "Crossbows", 10, 0xF50));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Ingenious Archery, Part I */
	public override object Title => 1073878;
	/* I have heard of a curious type of bow, you call it a "crossbow". It sounds fascinating and I would 
    very much like to examine one closely. Would you be able to obtain such an instrument for me? */
	public override object Description => 1074068;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me crossbows. */
	public override object Uncomplete => 1073924;
	/* My thanks for your service. Now, I shall teach you of elven archery. */
	public override object Complete => 1073968;
}
