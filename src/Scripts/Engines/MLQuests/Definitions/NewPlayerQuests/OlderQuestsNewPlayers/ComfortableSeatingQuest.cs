using Server.Items;

namespace Server.Engines.Quests;

public class ComfortableSeatingQuest : BaseQuest
{
	public ComfortableSeatingQuest()
	{
		AddObjective(new ObtainObjective(typeof(BambooChair), "Straw Chair", 1, 0xB5B));

		AddReward(new BaseReward(typeof(CarpentersSatchel), 1074282)); // Craftsman's Satchel
	}

	/* Comfortable Seating */
	public override object Title => 1075517;
	/* Hail friend, hast thou a moment? A mishap with a saw hath left me in a sorry state, for it shall be a while 
    before I canst return to carpentry. In the meantime, I need a comfortable chair that I may rest. Could thou craft 
    a straw chair?  Only a tool, such as a dovetail saw, a few boards, and some skill as a carpenter is needed. Remember, 
    this is a piece of furniture, so please pay attention to detail. */
	public override object Description => 1075518;
	/* I quite understand your reluctance.  If you reconsider, I'll be here. */
	public override object Refuse => 1072687;
	/* Is all going well? I look forward to the simple comforts in my very own home. */
	public override object Uncomplete => 1075509;
	/* This is perfect! */
	public override object Complete => 1074720;
}
