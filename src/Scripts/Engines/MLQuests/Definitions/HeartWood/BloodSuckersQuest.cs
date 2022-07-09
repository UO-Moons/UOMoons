using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class BloodSuckersQuest : BaseQuest
{
	public BloodSuckersQuest()
	{
		AddObjective(new SlayObjective(typeof(VampireBat), "Vampire Bats", 10));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Blood Suckers */
	public override object Title => 1072997;
	/* I bet you can't tangle with those bloodsuckers ... say around ten vampire bats!  I bet 
    they're too much for you.  You may as well confess you can't ... */
	public override object Description => 1073025;
	/* Hahahaha!  I knew it! */
	public override object Refuse => 1073019;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
