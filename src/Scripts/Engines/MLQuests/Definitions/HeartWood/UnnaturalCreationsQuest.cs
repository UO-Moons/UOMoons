using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class UnnaturalCreationsQuest : BaseQuest
{
	public UnnaturalCreationsQuest()
	{
		AddObjective(new SlayObjective(typeof(ExodusOverseer), "Exodus Overseers", 5));
		AddObjective(new SlayObjective(typeof(ExodusMinion), "Exodus Minions", 2));

		AddReward(new BaseReward(typeof(ArcaneCircleScroll), 1071026)); // Arcane Circle			
		AddReward(new BaseReward(typeof(GiftOfRenewalScroll), 1071027)); // Gift of Renewal
		AddReward(new BaseReward(typeof(SpellweavingBook), 1031600)); // Spellweaving Spellbook
	}

	public override QuestChain ChainId => QuestChain.Spellweaving;
	/* Unnatural Creations */
	public override object Title => 1072758;
	/* You have proven your desire to contribute to the community and serve the 
    people.  Now you must demonstrate your willingness to defend Sosaria from 
    the greatest blight that plagues her.  Unnatural creatures, brought to a 
    sort of perverted life, despoil our fair world.  Destroy them -- 5 Exodus 
    Overseers and 2 Exodus Minions. */
	public override object Description => 1072780;
	/* You must serve Sosaria with all your heart and strength.  
    Your unwillingness does not reflect favorably upon you. */
	public override object Refuse => 1072771;
	/* Every moment you procrastinate, these unnatural creatures damage Sosaria. */
	public override object Uncomplete => 1072779;
	/* Well done!  Well done, indeed.  You are worthy to become an arcanist! */
	public override object Complete => 1074167;
	public override bool CanOffer()
	{
		return MondainsLegacy.Spellweaving;
	}

	public override void GiveRewards()
	{
		Owner.Spellweaving = true;

		base.GiveRewards();
	}
}
