using Server.Items;

namespace Server.Engines.Quests;

public sealed class ImpressivePlaidQuest : BaseQuest
{
	public ImpressivePlaidQuest()
	{
		AddObjective(new ObtainObjective(typeof(Kilt), "Kilts", 10, 0x1537));

		AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
	}

	/* An Impressive Plaid */
	public override object Title => 1074044;
	/* I do not believe humans are so ridiculous as to wear something called a "kilt". Bring for me some of these 
    kilts, if they truly exist, and I will offer you meager reward. */
	public override object Description => 1074138;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class NiceShirtQuest : BaseQuest
{
	public NiceShirtQuest()
	{
		AddObjective(new ObtainObjective(typeof(FancyShirt), "Fancy Shirt", 10, 0x1EFD));

		AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
	}

	/* A Nice Shirt */
	public override object Title => 1074045;
	/* Humans call that a fancy shirt? I would wager the ends are frayed, the collar worn, the buttons loosely 
    stitched. Bring me fancy shirts and I will demonstrate the many ways in which they are inferior. */
	public override object Description => 1074139;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class LeatherAndLaceQuest : BaseQuest
{
	public LeatherAndLaceQuest()
	{
		AddObjective(new ObtainObjective(typeof(StuddedBustierArms), "Studded Bustiers", 10, 0x1C0C));

		AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
	}

	/* Leather and Lace */
	public override object Title => 1074047;
	/* No self respecting elf female would ever wear a studded bustier! I will prove it - bring me such clothing and I 
    will show you how ridiculous they are! */
	public override object Description => 1074141;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class FeyHeadgearQuest : BaseQuest
{
	public FeyHeadgearQuest()
	{
		AddObjective(new ObtainObjective(typeof(FlowerGarland), "Flower Garlands", 10, 0x2306));

		AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
	}

	/* Fey Headgear */
	public override object Title => 1074043;
	/* Humans do not deserve to wear a thing such as a flower garland. Help me prevent such things from falling into the 
    clumsy hands of humans -- bring me flower garlands! */
	public override object Description => 1074137;
	/* Fine then, I'm shall find another to run my errands then. */
	public override object Refuse => 1074063;
	/* Hurry up! I don't have all day to wait for you to bring what I desire! */
	public override object Uncomplete => 1074064;
	/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
	public override object Complete => 1074065;
}

public sealed class NewCloakQuest : BaseQuest
{
	public NewCloakQuest()
	{
		AddObjective(new ObtainObjective(typeof(GrobusFur), "Grobu's Fur", 1));

		AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
	}

	/* New Cloak */
	public override object Title => 1074684;
	/* I have created a masterpiece!  And all I need to finish it off is the soft fur of a wolf.  But not just 
    ANY wolf -- oh no, no, that wouldn't do.  I've heard tales of a mighty beast, Grobu, who is bonded to the 
    leader of the troglodytes.  Only Grobu's fur will do.  Will you retrieve it for me?  */
	public override object Description => 1074685;
	/* Perhaps I thought too highly of you. */
	public override object Refuse => 1074655;
	/* I've told you all I know of the creature.  Until you return with Grobu's fur I can't finish my cloak. */
	public override object Uncomplete => 1074686;
	/* Ah! So soft, so supple.  What a wonderful texture.  Here you are ... my thanks. */
	public override object Complete => 1074687;
	public override bool CanOffer()
	{
		return MondainsLegacy.PaintedCaves;
	}
}
