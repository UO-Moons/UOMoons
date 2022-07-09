using Server.Items;

namespace Server.Engines.Quests;

public class TaleOfTailQuest : BaseQuest
{
	public TaleOfTailQuest()
	{
		AddObjective(new ObtainObjective(typeof(AbscessTail), "Abscess' Tail", 1));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* A Tale of Tail */
	public override object Title => 1074726;
	/* I've heard of you, adventurer.  Your reputation is impressive, and now I'll put it to the test. This is not 
    something I ask lightly, for this task is fraught with danger, but it is vital.  Seek out the vile hydra Abscess, 
    slay it, and return to me with it's tail. */
	public override object Description => 1074727;
	/* Well, the beast will still be there when you are ready I suppose. */
	public override object Refuse => 1074728;
	/* Em, I thought I had explained already.  Abscess, the hydra, you know? Lots of heads but just the one tail.  
    I need the tail. I have my reasons. Go go go. */
	public override object Uncomplete => 1074729;
	/* Ah, the tail.  You did it!  You know the rumours about dried ground hydra tail powder are all true?  
    Thank you so much! */
	public override object Complete => 1074730;
	public override bool CanOffer()
	{
		return MondainsLegacy.BlightedGrove;
	}
}
