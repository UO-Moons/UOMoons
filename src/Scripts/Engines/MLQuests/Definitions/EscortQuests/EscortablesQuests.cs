using Server.Items;

namespace Server.Engines.Quests;

public sealed class EscortToYewQuest : BaseQuest
{
	public EscortToYewQuest()
	{
		AddObjective(new EscortObjective("Yew"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Yew */
	public override object Title => 1072275;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Yew.  Let's keep going. */
	public override object Uncomplete => 1072289;
}

public sealed class EscortToVesperQuest : BaseQuest
{
	public EscortToVesperQuest()
	{
		AddObjective(new EscortObjective("Vesper"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Vesper */
	public override object Title => 1072276;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Vesper.  Let's keep going. */
	public override object Uncomplete => 1072290;
}

public sealed class EscortToTrinsicQuest : BaseQuest
{
	public EscortToTrinsicQuest()
	{
		AddObjective(new EscortObjective("Trinsic"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Trinsic */
	public override object Title => 1072277;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Trinsic.  Let's keep going. */
	public override object Uncomplete => 1072291;
}

public sealed class EscortToSkaraQuest : BaseQuest
{
	public EscortToSkaraQuest()
	{
		AddObjective(new EscortObjective("Skara Brae"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Skara */
	public override object Title => 1072278;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Skara.  Let's keep going. */
	public override object Uncomplete => 1072292;
}

public sealed class EscortToSerpentsHoldQuest : BaseQuest
{
	public EscortToSerpentsHoldQuest()
	{
		AddObjective(new EscortObjective("Serpent's Hold"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Serpent's Hold */
	public override object Title => 1072279;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Serpent's Hold.  Let's keep going. */
	public override object Uncomplete => 1072293;
}

public sealed class EscortToNujelmQuest : BaseQuest
{
	public EscortToNujelmQuest()
	{
		AddObjective(new EscortObjective("Nujel'm"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Nujel'm */
	public override object Title => 1072280;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Nujel'm.  Let's keep going. */
	public override object Uncomplete => 1072294;
}

public sealed class EscortToMoonglowQuest : BaseQuest
{
	public EscortToMoonglowQuest()
	{
		AddObjective(new EscortObjective("Moonglow"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Moonglow */
	public override object Title => 1072281;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Moonglow.  Let's keep going. */
	public override object Uncomplete => 1072295;
}

public sealed class EscortToMinocQuest : BaseQuest
{
	public EscortToMinocQuest()
	{
		AddObjective(new EscortObjective("Minoc"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Minoc */
	public override object Title => 1072282;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Minoc.  Let's keep going. */
	public override object Uncomplete => 1072296;
}

public sealed class EscortToMaginciaQuest : BaseQuest
{
	public EscortToMaginciaQuest()
	{
		AddObjective(new EscortObjective("Magincia"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Magincia */
	public override object Title => 1072283;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Magincia.  Let's keep going. */
	public override object Uncomplete => 1072297;
}

public sealed class EscortToJhelomQuest : BaseQuest
{
	public EscortToJhelomQuest()
	{
		AddObjective(new EscortObjective("Jhelom"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Jhelom */
	public override object Title => 1072284;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Jhelom.  Let's keep going. */
	public override object Uncomplete => 1072298;
}

public sealed class EscortToCoveQuest : BaseQuest
{
	public EscortToCoveQuest()
	{
		AddObjective(new EscortObjective("Cove"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Cove */
	public override object Title => 1072285;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Cove.  Let's keep going. */
	public override object Uncomplete => 1072299;
}

public sealed class EscortToBritainQuest : BaseQuest
{
	public EscortToBritainQuest()
	{
		AddObjective(new EscortObjective("Britain"));
		AddReward(new BaseReward(typeof(Gold), 500, 1062577));
	}

	/* An escort to Britain */
	public override object Title => 1072286;
	/* I seek a worthy escort.  I can offer some small pay to any able bodied adventurer who can assist me.  
    * It is imperative that I reach my destination. */
	public override object Description => 1072287;
	/* I wish you would reconsider my offer.  I'll be waiting right here for someone brave enough to assist me. */
	public override object Refuse => 1072288;
	/* We have not yet arrived in Britain.  Let's keep going. */
	public override object Uncomplete => 1072300;
}
