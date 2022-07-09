using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class FrightmaresQuest : BaseQuest
{
	public FrightmaresQuest()
	{
		AddObjective(new SlayObjective(typeof(PlagueSpawn), "Plague Spawns", 10));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Frightmares */
	public override object Title => 1073000;
	/* I bet you can't handle ten plague spawns!  I bet they're too 
    much for you.  You may as well confess you can't ... */
	public override object Description => 1073036;
	/* Hahahaha!  I knew it! */
	public override object Refuse => 1073019;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
