using Server.Items;

namespace Server.Engines.Quests;

public sealed class NewHavenAlchemistEscortQuest : BaseQuest
{
	public NewHavenAlchemistEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Alchemist"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Alchemist in The bottled Imp */
	public override object Title => 1072314;
	/* I need some potions before I set out for a long journey. Can you take me to the alchemist in The Bottled Imp? */
	public override object Description => 1042769;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Alchemist in The Bottled Imp. Let's keep going. */
	public override object Uncomplete => 1072326;
}

public sealed class NewHavenBardEscortQuest : BaseQuest
{
	public NewHavenBardEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Bard"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Bard */
	public override object Title => 1072315;
	/* I fear my talent for music is less than my desire to learn, yet still I would like to try. Can you take me 
    * to the local music shop? */
	public override object Description => 1042772;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Bard.  Let's keep going. */
	public override object Uncomplete => 1072327;
}

public sealed class NewHavenWarriorEscortQuest : BaseQuest
{
	public NewHavenWarriorEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Warrior"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Warrior */
	public override object Title => 1072316;
	/* I need someone to help me rid my home of mongbats. Please take me to the local swordfighter. */
	public override object Description => 1042787;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Warrior.  Let's keep going. */
	public override object Uncomplete => 1072328;
}

public sealed class NewHavenTailorEscortQuest : BaseQuest
{
	public NewHavenTailorEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Tailor"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Tailor */
	public override object Title => 1072317;
	/* I need new clothes for a party, and I was wondering if you could take me to the tailor? */
	public override object Description => 1042781;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Tailor.  Let's keep going. */
	public override object Uncomplete => 1072329;
}

public sealed class NewHavenCarpenterEscortQuest : BaseQuest
{
	public NewHavenCarpenterEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Carpenter"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Carpenter */
	public override object Title => 1072318;
	/* I need a hammer and nails. Never mind for what. Take me to the local carpenter or leave me be. */
	public override object Description => 1042775;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Carpenter.  Let's keep going. */
	public override object Uncomplete => 1072330;
}

public sealed class NewHavenMapmakerEscortQuest : BaseQuest
{
	public NewHavenMapmakerEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Mapmaker"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Mapmaker */
	public override object Title => 1072319;
	/* Where am I? Who am I? Do you know me? Hmmm - on second thought, I think I best stick with where I am first. 
    * Do you know where I can get a map? */
	public override object Description => 1042793;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Mapmaker.  Let's keep going. */
	public override object Uncomplete => 1072331;
}

public sealed class NewHavenMageEscortQuest : BaseQuest
{
	public NewHavenMageEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Mage"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Mage */
	public override object Title => 1072320;
	/* You there. Take me to see a sorcerer so I can turn a friend back in to a human. He is currently a cat 
    * and keeps demanding milk. */
	public override object Description => 1042790;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Mage.  Let's keep going. */
	public override object Uncomplete => 1072332;
}

public sealed class NewHavenInnEscortQuest : BaseQuest
{
	public NewHavenInnEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Inn"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Inn */
	public override object Title => 1072321;
	/* I need something to eat. I am starving. Can you take me to the inn? */
	public override object Description => 1042796;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Inn.  Let's keep going. */
	public override object Uncomplete => 1072333;
}

public sealed class NewHavenFarmEscortQuest : BaseQuest
{
	public NewHavenFarmEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Farm"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Farm */
	public override object Title => 1072322;
	/* I am hoping to swap soil stories with a local farmer, but I cannot find one. Can you take me to one? */
	public override object Description => 1042799;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Farm.  Let's keep going. */
	public override object Uncomplete => 1072334;
}

public sealed class NewHavenDocksEscortQuest : BaseQuest
{
	public NewHavenDocksEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Docks"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Docks */
	public override object Title => 1072323;
	/* I have heard of a magical fish that grants wishes. I bet THAT fisherman knows where the fish is. Please take me to him. */
	public override object Description => 1042802;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Docks.  Let's keep going. */
	public override object Uncomplete => 1072335;
}

public sealed class NewHavenBowyerEscortQuest : BaseQuest
{
	public NewHavenBowyerEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Bowyer"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Bowyer */
	public override object Title => 1072324;
	/* You there. Do you know the way to the local archer? */
	public override object Description => 1042805;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Bowyer.  Let's keep going. */
	public override object Uncomplete => 1072336;
}

public sealed class NewHavenBankEscortQuest : BaseQuest
{
	public NewHavenBankEscortQuest()
	{
		AddObjective(new EscortObjective("the New Haven Bank"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to the New Haven Bank */
	public override object Title => 1072325;
	/* I have a debt I need to pay off at the bank. Do you know the way there? */
	public override object Description => 1042784;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived at the New Haven Bank.  Let's keep going. */
	public override object Uncomplete => 1072337;
}
