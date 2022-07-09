using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class TroglodytesQuest : BaseQuest
{
	public TroglodytesQuest()
	{
		AddObjective(new SlayObjective(typeof(Troglodyte), "Troglodytes", 12));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Troglodytes! */
	public override object Title => 1074688;
	/* Oh nevermind, you don't look capable of my task afterall.  Haha! What was I thinking - you could never handle 
    killing troglodytes.  It'd be suicide.  What?  I don't know, I don't want to be responsible ... well okay if 
    you're really sure? */
	public override object Description => 1074689;
	/* Probably the wiser course of action. */
	public override object Refuse => 1074690;
	/* You still need to kill those troglodytes, remember? */
	public override object Uncomplete => 1074691;
	public override bool CanOffer()
	{
		return MondainsLegacy.PaintedCaves;
	}
}
