using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class WarriorCasteQuest : BaseQuest
{
	public WarriorCasteQuest()
	{
		AddObjective(new SlayObjective(typeof(TerathanWarrior), "Terathan Warriors", 10));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* Warrior Caste */
	public override object Title => 1073078;
	/* The Terathan are an aggressive species. Left unchecked, they will swarm across our lands. 
    And where will that leave us? Compost in the hive, that's what! Stop them, stop them cold my 
    friend. Kill their warriors and you'll check their movement, that is certain. */
	public override object Description => 1073568;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* Unless you kill at least 10 Terathan Warriors, you won't have any impact on their hive. */
	public override object Uncomplete => 1073588;
}
