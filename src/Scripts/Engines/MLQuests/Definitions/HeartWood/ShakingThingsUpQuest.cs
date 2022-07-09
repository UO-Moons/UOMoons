using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ShakingThingsUpQuest : BaseQuest
{
	public ShakingThingsUpQuest()
	{
		AddObjective(new SlayObjective(typeof(RedSolenWarrior), "Red Solen Warriors", 10));
		AddObjective(new SlayObjective(typeof(BlackSolenWarrior), "Black Solen Warriors", 10));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	public override bool AllObjectives => false;
	/* Shaking Things Up */
	public override object Title => 1073083;
	/* A Solen hive is a fascinating piece of ecology. It's put together like a finely crafted clock. Who knows 
    what happens if you remove something? So let's find out. Exterminate a few of the warriors and I'll make it 
    worth your while. */
	public override object Description => 1073573;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* I don't think you've gotten their attention yet -- you need to kill at least 10 Solen Warriors. */
	public override object Uncomplete => 1073593;
}
