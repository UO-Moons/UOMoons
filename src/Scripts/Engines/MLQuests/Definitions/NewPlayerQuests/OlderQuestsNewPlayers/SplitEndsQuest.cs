using Server.Items;

namespace Server.Engines.Quests;

public class SplitEndsQuest : BaseQuest
{
	public SplitEndsQuest()
	{
		AddObjective(new ObtainObjective(typeof(Arrow), "arrow", 20, 0xF3F));

		AddReward(new BaseReward(typeof(FletchersSatchel), 1074282)); // Craftsman's Satchel
	}

	/* Split Ends */
	public override object Title => 1075506;
	/* *sighs* I think bowcrafting is a might beyond my talents. Say there, you look a bit more confident with tools. 
    Can I persuade thee to make a few arrows? You could have my satchel in return... 'tis useless to me! You'll need a 
    fletching kit to start, some feathers, and a few arrow shafts. Just use the fletching kit while you have the other 
    things, and I'm sure you'll figure out the rest. */
	public override object Description => 1075507;
	/* Oh. Well. I'll just keep trying alone, I suppose... */
	public override object Refuse => 1075508;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
	/* Thanks for helping me out.  Here's the reward I promised you. */
	public override object Complete => 1072272;
}
