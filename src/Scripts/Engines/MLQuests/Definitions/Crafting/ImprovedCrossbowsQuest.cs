using Server.Items;

namespace Server.Engines.Quests;

public sealed class ImprovedCrossbowsQuest : BaseQuest
{
	public ImprovedCrossbowsQuest()
	{
		AddObjective(new ObtainObjective(typeof(HeavyCrossbow), "Heavy Crossbows", 10, 0x13FD));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Improved Crossbows */
	public override object Title => 1074021;
	/* How lazy is man! You cannot even be bothered to pull your own drawstring and hold an arrow ready? You 
    must invent a device to do it for you? I cannot understand, but perhaps if I examine a heavy crossbow for 
    myself, I will see their appeal. Go and bring me such a device and I will repay your meager favor. */
	public override object Description => 1074115;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}
