using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class PlagueLordQuest : BaseQuest
{
	public PlagueLordQuest()
	{
		AddObjective(new SlayObjective(typeof(PlagueSpawn), "Plague Spawns", 10, "Palace of Paroxysmus"));
		AddObjective(new SlayObjective(typeof(PlagueBeast), "Plague Beasts", 3, "Palace of Paroxysmus"));
		AddObjective( new SlayObjective( typeof( PlagueBeastLord ), "Plague Beast Lord", 1, "Palace of Paroxysmus" ) );

		AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
	}

	/* Plague Lord */
	public override object Title => 1073061;
	/* Some of the most horrific creatures have slithered out of the sinkhole there and begun terrorizing the surrounding 
    area. The plague creatures are one of the most destruction of the minions of Paroxysmus.  Are you willing to do 
    something about them? */
	public override object Description => 1074692;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
	public override bool CanOffer()
	{
		return MondainsLegacy.PalaceOfParoxysmus;
	}
}
