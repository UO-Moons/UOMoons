using Server.Items;
using System;

namespace Server.Engines.Quests;

public class FleeAndFatigueQuest : BaseQuest
{
	public FleeAndFatigueQuest()
	{
		AddObjective(new ObtainObjective(typeof(RefreshPotion), "Refresh Potion", 10, 0xF0B));

		AddReward(new BaseReward(typeof(AlchemistsSatchel), 1074282)); // Craftsman's Satchel
	}

	public override TimeSpan RestartDelay => TimeSpan.FromMinutes(3);
	/* Flee and Fatigue */
	public override object Title => 1075487;
	/* I was just *coughs* ambushed near the moongate. *wheeze* Why do I pay my taxes? Where were the guards? 
    You then, you an Alchemist? If you can make me a few Refresh potions, I will be back on my feet and can 
    give those lizards the what for! Find a mortar and pestle, a good amount of black pearl, and ten empty 
    bottles to store the finished potions in. Just use the mortar and pestle and the rest will surely come 
    to you. When you return, the favor will be repaid. */
	public override object Description => 1075488;
	/* Fine fine, off with *cough* thee then! The next time you see a lizardman though, give him a whallop for me, eh? */
	public override object Refuse => 1075489;
	/* Just remember you need to use your mortar and pestle while you have empty bottles and some black pearl. 
    Refresh potions are what I need. */
	public override object Uncomplete => 1075490;
	/* *glug* *glug* Ahh... Yes! Yes! That feels great! Those lizardmen will never know what hit 'em! Here, take 
    this, I can get more from the lizards. */
	public override object Complete => 1075491;
}
