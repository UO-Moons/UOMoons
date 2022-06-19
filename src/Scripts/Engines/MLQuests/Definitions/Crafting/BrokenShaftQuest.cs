using Server.Items;

namespace Server.Engines.Quests;

public sealed class BrokenShaftQuest : BaseQuest
{
	public BrokenShaftQuest()
	{
		AddObjective(new ObtainObjective(typeof(Arrow), "Arrows", 10, 0xF3F));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Broken Shaft */
	public override object Title => 1074018;
	/* What do humans know of archery? Humans can barely shoot straight. Why, your efforts are 
    absurd. In fact, I will make a wager - if these so called human arrows I've heard about are 
    really as effective and innovative as human braggarts would have me believe, then I'll trade 
    you something useful.  I might even teach you something of elven craftsmanship. */
	public override object Description => 1074112;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}
