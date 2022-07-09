using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class UnholyConstructQuest : BaseQuest
{
	public UnholyConstructQuest()
	{
		AddObjective(new SlayObjective(typeof(Golem), "Golems", 10));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706)); // A large bag of treasure.
	}

	/* Unholy Construct */
	public override object Title => 1073666;
	/* They're unholy, I say. Golems, a walking mockery of all life, born of 
    blackest magic. They're not truly alive, so destroying them isn't a crime, 
    it's a service. A service I will gladly pay for. */
	public override object Description => 1073705;
	/* Perhaps you'll change your mind and return at some point. */
	public override object Refuse => 1073733;
	/* The unholy brutes, the Golems, must be smited! */
	public override object Uncomplete => 1073746;
	/* Reduced those Golems to component parts? Good, then -- you deserve this reward! */
	public override object Complete => 1073787;
}
