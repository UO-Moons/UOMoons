using Server.Items;

namespace Server.Engines.Quests;

public sealed class CutsBothWaysQuest : BaseQuest
{
	public CutsBothWaysQuest()
	{
		AddObjective(new ObtainObjective(typeof(Broadsword), "Broadswords", 12, 0xF5E));

		AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
	}

	/* Cuts Both Ways */
	public override object Title => 1073913;
	/* What would you say is a typical human instrument of war? Is a broadsword a typical example? 
    I wish to see more of such human weapons, so I would gladly trade elven knowledge for human steel. */
	public override object Description => 1074103;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me broadswords. */
	public override object Uncomplete => 1073959;
	/* Enjoy my thanks for your service. */
	public override object Complete => 1073978;
}
