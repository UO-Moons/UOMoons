using Server.Items;
using System;

namespace Server.Engines.Quests;

public class StitchInTimeQuest : BaseQuest
{
	public StitchInTimeQuest()
	{
		AddObjective(new ObtainObjective(typeof(FancyDress), "Fancy Dress", 1, 0x1EFF));

		AddReward(new BaseReward(typeof(OldRing), 1075524)); // an old ring
		AddReward(new BaseReward(typeof(OldNecklace), 1075525)); // an old necklace
	}

	public override TimeSpan RestartDelay => TimeSpan.FromMinutes(2);
	/* A Stitch in Time */
	public override object Title => 1075523;
	/* Oh how I wish I had a fancy dress like the noble ladies of Castle British! I don't have much... but I 
    have a few trinkets I might trade for it. It would mean the world to me to go to a fancy ball and dance 
    the night away. Oh, and I could tell you how to make one! You just need to use your sewing kit on enough 
    cut cloth, that's all. */
	public override object Description => 1075522;
	/* Won't you reconsider? It'd mean the world to me, it would! */
	public override object Refuse => 1075526;
	/* Hello again! Do you need anything? You may want to visit the tailor's shop for cloth and a sewing kit, 
    if you don't already have them. */
	public override object Uncomplete => 1075527;
	/* It's gorgeous! I only have a few things to give you in return, but I can't thank you enough! Maybe I'll 
    even catch Uzeraan's eye at the, er, *blushes* I mean, I can't wait to wear it to the next town dance! */
	public override object Complete => 1075528;
}
