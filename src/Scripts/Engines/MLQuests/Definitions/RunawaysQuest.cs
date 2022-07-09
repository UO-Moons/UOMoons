using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class RunawaysQuest : BaseQuest
{
	public RunawaysQuest()
	{
		AddObjective(new SlayObjective(typeof(FrenziedOstard), "Frenzied Ostards", 12));

		AddReward(new BaseReward(typeof(TrinketBag), 1072341));
	}

	/* Runaways! */
	public override object Title => 1072993;
	/* You've got to help me out! Those wild ostards have been causing absolute havok around here.  Kill them 
    off before they destroy my land.  There are around twelve of them. */
	public override object Description => 1073026;
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