using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class DreadhornQuest : BaseQuest
{
	public DreadhornQuest()
	{
		AddObjective(new SlayObjective(typeof(DreadHorn), "Dread Horn", 1));

		AddReward(new BaseReward(typeof(RewardBox), 1072584));
	}

	/* Dreadhorn */
	public override object Title => 1074645;
	/* Can you comprehend it? I cannot, I confess.  The most pristine and perfect Lord of Sosaria 
    has fallen prey to the blight.  From the depths of my heart I mourn his corruption; my thoughts 
    are filled with pity for this glorious creature now tainted.  And my blood boils with fury at 
    those responsible for the innocent creature's undoing.  Will you find Dread Horn, as he is now 
    called, and free him from this misery? */
	public override object Description => 1074646;
	/* How can you not feel as I do? */
	public override object Refuse => 1074647;
	/* The lush and fertile land where Dread Horn now lives is twisted and tainted, a result of his 
    corruption.  The fey folk have sealed the land off through their magics, but you can enter through 
    an enchanted mushroom fairy circle. */
	public override object Uncomplete => 1074648;
	/* Thank you.  I haven't the words to express my gratitude. */
	public override object Complete => 1074649;
	public override bool CanOffer()
	{
		return MondainsLegacy.TwistedWeald;
	}
}
