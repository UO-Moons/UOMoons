using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class CircleOfLifeQuest : BaseQuest
{
	public CircleOfLifeQuest()
	{
		AddObjective(new SlayObjective(typeof(BogThing), "Bog Things", 8));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Circle of Life */
	public override object Title => 1073656;
	/* There's been a bumper crop of evil with the Bog Things in these parts, my friend. Though they are 
    foul creatures, they are also most fecund. Slay one and you make the land more fertile. Even better, 
    slay several and I will give you whatever coin I can spare. */
	public override object Description => 1073695;
	/* Perhaps you'll change your mind and return at some point. */
	public override object Refuse => 1073733;
	/* Continue to seek and kill the Bog Things. */
	public override object Uncomplete => 1073736;
}
