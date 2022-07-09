using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class GuileIrkAndSpiteQuest : BaseQuest
{
	public GuileIrkAndSpiteQuest()
	{
		AddObjective(new SlayObjective(typeof(Guile), "Guile", 1));
		AddObjective(new SlayObjective(typeof(Irk), "Irk", 1));
		AddObjective(new SlayObjective(typeof(Spite), "Spite", 1));

		AddReward(new BaseReward(typeof(RewardBox), 1072584));
	}

	/* Guile, Irk and Spite */
	public override object Title => 1074739;
	/* You know them, don't you.  The three?  They look like you, you'll see. They looked like me, I remember, they 
    looked like, well, you'll see.  The three.  They'll drive you mad too, if you let them.  They are trouble, and 
    they need to be slain.  Seek them out. */
	public override object Description => 1074740;
	/* You just don't understand the gravity of the situation.  If you did, you'd agree to my task. */
	public override object Refuse => 1074745;
	/* Perhaps I was unclear.  You'll know them when you see them, because you'll see you, and you, and you.  Hurry now. */
	public override object Uncomplete => 1074746;
	/* Are you one of THEM?  Ahhhh!  Oh, wait, if you were them, then you'd be me.  So you're -- you.  Good job! */
	public override object Complete => 1074747;
	public override bool CanOffer()
	{
		return MondainsLegacy.TwistedWeald;
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