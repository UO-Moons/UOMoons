using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class TheAfterlifeQuest : BaseQuest
{
	public TheAfterlifeQuest()
	{
		AddObjective(new SlayObjective(typeof(Mummy), "Mummies", 15));

		AddReward(new BaseReward(typeof(TreasureBag), 1072583));
	}

	/* The Afterlife */
	public override object Title => 1073073;
	/* Nobody told me about the Mummy's Curse. How was I supposed to know you shouldn't disturb the tombs? 
    Oh, sure, now all I hear about is the curse of the vengeful dead. I'll tell you what - make a few of 
    these mummies go away and we'll keep this between you and me. */
	public override object Description => 1073563;
	/* I hope you'll reconsider. Until then, farwell. */
	public override object Refuse => 1073580;
	/* Uh, I don't think you're quite done killing Mummies yet. */
	public override object Uncomplete => 1073583;
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