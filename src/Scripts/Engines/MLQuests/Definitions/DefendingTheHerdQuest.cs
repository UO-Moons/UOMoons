namespace Server.Engines.Quests;

public class DefendingTheHerdQuest : BaseQuest
{
	public DefendingTheHerdQuest()
	{
		AddObjective(new EscortObjective("Bravehorn's Drinking Pool"));

		AddReward(new BaseReward(1072806)); // The boon of Bravehorn.
	}

	public override bool ForceRemember => true;
	/* Defending the Herd */
	public override object Title => 1072785;
	/* *snort* ... guard-mates ... guard-herd *hoof stomp* ... defend-with-hoof-and-horn ... thirsty-drink.  
    *proud head-toss* */
	public override object Description => 1072825;
	/* *snort* */
	public override object Refuse => 1072826;
	/* *impatient hoof stomp* ... thirsty herd ... water scent. */
	public override object Uncomplete => 1072827;
	public override void GiveRewards()
	{
		base.GiveRewards();

		Owner.SendLocalizedMessage(1074942, null, 0x23); // You have gained the boon of Bravehorn!  You have glimpsed the nobility of those that sacrifice themselves for their people.  You are one step closer to claiming your elven heritage.
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