using Server.Items;

namespace Server.Engines.Quests;

public sealed class HuteCoutureQuest : BaseQuest
{
	public HuteCoutureQuest()
	{
		AddObjective(new ObtainObjective(typeof(FlowerGarland), "Flower Garlands", 10, 0x2306));
		AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
	}

	/* H'ute Couture */
	public override object Title => 1073901;
	/* Most human apparel is interesting to elven eyes. But there is one garment - the flower garland - 
    which sounds very elven indeed. Could I see how a human crafts such an object of beauty? In exchange, 
    I could share with you the wonders of elven garments. */
	public override object Description => 1074091;
	/* I will patiently await your reconsideration. */
	public override object Refuse => 1073921;
	/* I will be in your debt if you bring me flower garlands. */
	public override object Uncomplete => 1073947;
	/* I appreciate your service. Now, see what elven hands can create. */
	public override object Complete => 1073973;
}
