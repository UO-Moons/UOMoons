using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class UndeadMagesQuest : BaseQuest
{
	public UndeadMagesQuest()
	{
		AddObjective(new SlayObjective(typeof(BoneMagi), "Bone Mages", 10));
		AddObjective(new SlayObjective(typeof(SkeletalMage), "Skeletal Mages", 10));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	public override bool AllObjectives => false;
	/* Undead Mages */
	public override object Title => 1073080;
	/* Why must the dead plague the living? With their foul necromancy and dark sorceries, the undead 
    menace the countryside. I fear what will happen if no one is strong enough to face these nightmare 
    sorcerers and thin their numbers. */
	public override object Description => 1073570;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* Surely, a brave soul like yourself can kill 10 Bone Magi and Skeletal Mages? */
	public override object Uncomplete => 1073590;
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