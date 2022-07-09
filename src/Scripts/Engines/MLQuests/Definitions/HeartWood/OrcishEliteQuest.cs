using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class OrcishEliteQuest : BaseQuest
{
	public OrcishEliteQuest()
	{
		AddObjective(new SlayObjective(typeof(OrcBomber), "Orc Bombers", 6));
		AddObjective(new SlayObjective(typeof(OrcCaptain), "Orc Captain", 4));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* Orcish Elite */
	public override object Title => 1073081;
	/* Foul brutes! No one loves an orc, but some of them are worse than the rest. Their Captains and their 
    Bombers, for instance, they're the worst of the lot. Kill a few of those, and the rest are just a rabble. 
    Exterminate a few of them and you'll make the world a sunnier place, don't you know. */
	public override object Description => 1073571;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* The only good orc is a dead orc - and 4 dead Captains and 6 dead Bombers is even better! */
	public override object Uncomplete => 1073591;
}
