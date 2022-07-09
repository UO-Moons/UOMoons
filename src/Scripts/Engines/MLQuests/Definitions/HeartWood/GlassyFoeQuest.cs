using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class GlassyFoeQuest : BaseQuest
{
	public GlassyFoeQuest()
	{
		AddObjective(new SlayObjective(typeof(CrystalLatticeSeeker), "Crystal Lattice Seekers", 5, "Prism of Light"));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Glassy Foe */
	public override object Title => 1073055;
	/* Good, you're here.  The presence of a twisted creature deep under the earth near Nu'Jelm has 
    corrupted the natural growth of crystals in that region.  They've become infused with the twisting 
    energy - they've come to a sort of life.  This is an abomination that festers within Sosaria.  You 
    must eradicate the crystal lattice seekers. */
	public override object Description => 1074669;
	/* These abominations must not be permitted to fester! */
	public override object Refuse => 1074671;
	/* You must not waste time. Do not suffer these crystalline abominations to live. */
	public override object Uncomplete => 1074672;
	/* You have done well.  Enjoy this reward. */
	public override object Complete => 1074673;
	public override bool CanOffer()
	{
		return MondainsLegacy.PrismOfLight;
	}
}
