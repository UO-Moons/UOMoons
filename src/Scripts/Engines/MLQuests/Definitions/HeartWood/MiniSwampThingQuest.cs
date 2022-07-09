using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class MiniSwampThingQuest : BaseQuest
{
	public MiniSwampThingQuest()
	{
		AddObjective(new SlayObjective(typeof(Bogling), "Boglings", 20));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* Mini Swamp Thing */
	public override object Title => 1073072;
	/* Some say killing a boggling brings good luck. I don't place much stock in old wives' tales, but I can say a few 
    dead bogglings would certainly be lucky for me! Help me out and I can reward you for your efforts.  */
	public override object Description => 1073562;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* Go back and kill all 20 bogglings! */
	public override object Uncomplete => 1073582;
}
