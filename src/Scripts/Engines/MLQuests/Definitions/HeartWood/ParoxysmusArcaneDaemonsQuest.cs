using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ParoxysmusArcaneDaemonsQuest : BaseQuest
{
	public ParoxysmusArcaneDaemonsQuest()
	{
		AddObjective(new SlayObjective(typeof(ArcaneDaemon), "Arcane Daemons", 10, "Palace of Paroxysmus"));

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Paroxysmus' Arcane Daemons */
	public override object Title => 1073070;
	/* The arcane daemons that worship Paroxysmus pose a tremendous danger. Will you enter the lair and 
    see to their destruction? */
	public override object Description => 1074697;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
	public override bool CanOffer()
	{
		return MondainsLegacy.PalaceOfParoxysmus;
	}
}
