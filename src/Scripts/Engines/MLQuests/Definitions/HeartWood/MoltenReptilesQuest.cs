using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class MoltenReptilesQuest : BaseQuest
{
	public MoltenReptilesQuest()
	{
		AddObjective(new SlayObjective(typeof(LavaLizard), "Lava Lizards", 10));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Molten Reptiles */
	public override object Title => 1072989;
	/* I bet you can't kill ... say ten ... lava lizards!  I bet they're 
    too much for you.  You may as well confess you can't ... */
	public override object Description => 1073018;
	/* Hahahaha!  I knew it! */
	public override object Refuse => 1073019;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
