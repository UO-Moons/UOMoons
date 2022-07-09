using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ThinningTheHerdQuest : BaseQuest
{
	public ThinningTheHerdQuest()
	{
		AddObjective(new SlayObjective(typeof(Goat), "Goats", 10));

		AddReward(new BaseReward(typeof(SmallTrinketBag), 1072268));
	}

	/* Thinning the Herd */
	public override object Title => 1072249;
	/* Psst!  Hey ... psst!  Listen, I need some help here but it's gotta be hush hush.  I 
    don't want THEM to know I'm onto them.  They watch me.  I've seen them, but they don't 
    know that I know what I know.  You know?  Anyway, I need you to scare them off by killing 
    a few of them.  That'll send a clear message that I won't suffer goats watching me! */
	public override object Description => 1072263;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
}
