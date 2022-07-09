using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class TroubleOnTheWingQuest : BaseQuest
{
	public TroubleOnTheWingQuest()
	{
		AddObjective(new SlayObjective(typeof(Gargoyle), "Gargoyles", 12, "Sanctuary"));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Trouble on the Wing */
	public override object Title => 1072371;
	/* Those gargoyles need to get knocked down a peg or two, if you ask me.  They're always flying 
    over here and lobbing things at us. What a nuisance.  Drop a dozen of them for me, would you? */
	public override object Description => 1072593;
	/* Don't tell me you're a gargoyle sympathizer?  *spits* */
	public override object Refuse => 1072594;
	/* Those blasted gargoyles hang around the old tower.  That's the best place to hunt them down. */
	public override object Uncomplete => 1072595;
	public override bool CanOffer()
	{
		return MondainsLegacy.Sanctuary;
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