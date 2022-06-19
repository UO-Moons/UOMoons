using Server.Items;

namespace Server.Engines.Quests;

public sealed class DragonProtectionQuest : BaseQuest
{
	public DragonProtectionQuest()
	{
		AddObjective(new ObtainObjective(typeof(DragonHelm), "Dragon Helms", 10, 0x2645));

		AddReward(new BaseReward(typeof(SmithsCraftsmanSatchel), 1074282));
	}

	/* Dragon Protection */
	public override object Title => 1073915;
	/* Mankind, I am told, knows how to take the scales of a terrible dragon and forge them into powerful 
    armor. Such a feat of craftsmanship! I would give anything to view such a creation - I would even teach 
    some of the prize secrets of the elven people. */
	public override object Description => 1074105;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me dragon armor. */
	public override object Uncomplete => 1073961;
	/* Enjoy my thanks for your service. */
	public override object Complete => 1073978;
}
