using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class GargoylesWrathQuest : BaseQuest
{
	public GargoylesWrathQuest()
	{
		AddObjective(new SlayObjective(typeof(GargoyleEnforcer), "Gargoyle Enforcers", 6));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Gargoyle's Wrath */
	public override object Title => 1073658;
	/* It is regretable that the Gargoyles insist upon warring with us. Their Enforcers attack men on sight, despite 
    all efforts at reason. To help maintain order in this region, I have been authorized to encourage bounty hunters 
    to reduce their numbers. Eradicate their number and I will reward you handsomely. */
	public override object Description => 1073697;
	/* Perhaps you'll change your mind and return at some point. */
	public override object Refuse => 1073733;
	/* I won't be able to pay you until you've gotten enough Gargoyle Enforcers. */
	public override object Uncomplete => 1073738;
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