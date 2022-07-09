using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ThreeWishesQuest : BaseQuest
{
	public ThreeWishesQuest()
	{
		AddObjective(new SlayObjective(typeof(Efreet), "Efreets", 8));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Three Wishes */
	public override object Title => 1073660;
	/* If I had but one wish, it would be to rid myself of these dread Efreet! Fire and ash, they are cunning and 
    deadly! You look a brave soul - would you be interested in earning a rich reward for slaughtering a few of the 
    smoky devils? */
	public override object Description => 1073699;
	/* Perhaps you'll change your mind and return at some point. */
	public override object Refuse => 1073733;
	/* Those smoky devils, the Efreets, are still about. */
	public override object Uncomplete => 1073740;
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