using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ExtinguishingTheFlameQuest : BaseQuest
{
	public ExtinguishingTheFlameQuest()
	{
		AddObjective(new InternalObjective());

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* Extinguishing the Flame */
	public override object Title => 1072911;
	/* The Dragon's Flame sect members have gone too far! Express to them my displeasure by slaying ten 
    of them. But remember, I do not condone war on women, so I will only accept the deaths of men, 
    human or elf.  Either race will do, I care not for the shape of their ears. Yes, this action will 
    properly make clear my disapproval and has a pleasing harmony. */
	public override object Description => 1072966;
	/* As you wish. */
	public override object Refuse => 1072979;
	/* The Black Order's fortress home is well hidden.  Legend has it that a humble fishing village 
    disguises the magical portal. */
	public override object Uncomplete => 1072980;
	public override bool CanOffer()
	{
		return MondainsLegacy.Citadel;
	}

	private class InternalObjective : SlayObjective
	{
		public InternalObjective()
			: base(typeof(DragonsFlameMage), "Male Dragon's Flame Mages", 10, "TheCitadel")
		{
		}

		public override bool IsObjective(Mobile mob)
		{
			return !mob.Female && base.IsObjective(mob);
		}
	}
}
