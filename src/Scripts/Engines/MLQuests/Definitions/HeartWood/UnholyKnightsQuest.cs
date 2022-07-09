using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class UnholyKnightsQuest : BaseQuest
{
	public UnholyKnightsQuest()
	{
		AddObjective(new SlayObjective(typeof(BoneKnight), "Bone Knights", 16));
		AddObjective(new SlayObjective(typeof(SkeletalKnight), "Skeletal Knights", 16));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	public override bool AllObjectives => false;
	/* Unholy Knights */
	public override object Title => 1073075;
	/* Please, hear me kind traveler. You know when a knight falls, sometimes they are cursed to roam the earth as 
	undead mockeries of their former glory? That is too grim a fate for even any knight to suffer! Please, put them 
	out of their misery. I will offer you what payment I can if you will end the torment of these undead wretches. */
	public override object Description => 1073565;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* Your task is not done. Continue putting the Skeleton and Bone Knights to rest. */
	public override object Uncomplete => 1073585;
}
