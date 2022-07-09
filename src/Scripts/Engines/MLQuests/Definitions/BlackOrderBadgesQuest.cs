using Server.Items;

namespace Server.Engines.Quests;

public class BlackOrderBadgesQuest : BaseQuest
{
	public BlackOrderBadgesQuest()
	{
		AddObjective(new ObtainObjective(typeof(SerpentFangSectBadge), "Serpent Fang Badges", 5));
		AddObjective(new ObtainObjective(typeof(TigerClawSectBadge), "Tiger Claw Badges", 5));
		AddObjective(new ObtainObjective(typeof(DragonFlameSectBadge), "Dragon Flame Badges", 5));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* Black Order Badges */
	public override object Title => 1072903;
	/* What's that? *alarmed gasp*  Do not speak of the Black Order so loudly, they might hear and take offense.  *whispers*  
    I collect the badges of their sects, if you wish to seek them out and slay them.  Bring five of each and I will reward you. */
	public override object Description => 1072962;
	/* *whisper* It's a very dangerous task.  Let me know if you change your mind. */
	public override object Refuse => 1072971;
	/* *whisper* The Citadel entrance is disguised as a fishing village.  The magical portal into the stronghold itself is 
    moved frequently.  You'll need to search for it. */
	public override object Uncomplete => 1072972;
	public override bool CanOffer()
	{
		return MondainsLegacy.Citadel;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}