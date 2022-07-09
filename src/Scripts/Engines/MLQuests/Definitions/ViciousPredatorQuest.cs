using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class ViciousPredatorQuest : BaseQuest
{
	public ViciousPredatorQuest()
	{
		AddObjective(new SlayObjective(typeof(DireWolf), "Dire Wolves ", 10));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Vicious Predator */
	public override object Title => 1072994;
	/* You've got to help me out! Those dire wolves have been causing absolute havok around here.  Kill them off 
    before they destroy my land.  They run around in a pack of around ten.<br> */
	public override object Description => 1073028;
	/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
	public override object Refuse => 1072270;
	/* You're not quite done yet.  Get back to work! */
	public override object Uncomplete => 1072271;
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