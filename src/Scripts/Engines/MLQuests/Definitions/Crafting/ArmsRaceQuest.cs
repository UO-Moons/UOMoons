using Server.Items;

namespace Server.Engines.Quests;

public sealed class ArmsRaceQuest : BaseQuest
{
	public ArmsRaceQuest()
	{
		AddObjective(new ObtainObjective(typeof(Crossbow), "Crossbows", 10, 0xF50));

		AddReward(new BaseReward(typeof(FletcherCraftsmanSatchel), 1074282));
	}

	/* Arms Race */
	public override object Title => 1074020;
	/* Leave it to a human to try and improve upon perfection. To take a bow and turn it into a mechanical 
	contraption like a crossbow. I wish to see more of this sort of "invention". Fetch for me a crossbow, 
	human. */
	public override object Description => 1074114;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}
