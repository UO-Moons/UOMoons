using Server.Items;

namespace Server.Engines.Quests;

public sealed class SimpleBowQuest : BaseQuest
{
	public SimpleBowQuest()
	{
		AddObjective(new ObtainObjective(typeof(Bow), "Bows", 10, 0x13B2));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* A Simple Bow */
	public override object Title => 1073877;
	/* I wish to try a bow crafted in the human style. Is it possible for you to bring me 
    such a weapon? I would be happy to return this favor. */
	public override object Description => 1074067;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me bows. */
	public override object Uncomplete => 1073923;
	/* My thanks for your service. Now, I shall teach you of elven archery. */
	public override object Complete => 1073968;
}
