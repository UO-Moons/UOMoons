using Server.Items;

namespace Server.Engines.Quests;

public sealed class IngeniousArcheryPartThreeQuest : BaseQuest
{
	public IngeniousArcheryPartThreeQuest()
	{
		AddObjective(new ObtainObjective(typeof(RepeatingCrossbow), "Repeating Crossbows", 10, 0x26C3));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Ingenious Archery, Part III */
	public override object Title => 1073880;
	/* My friend, I am in search of a device, a instrument of remarkable human ingenuity. It is a 
    repeating crossbow. If you were to obtain such a device, I would gladly reveal to you some of 
    the secrets of elven craftsmanship. */
	public override object Description => 1074070;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me repeating crossbows. */
	public override object Uncomplete => 1073926;
	/* My thanks for your service. Now, I shall teach you of elven archery. */
	public override object Complete => 1073968;
}
