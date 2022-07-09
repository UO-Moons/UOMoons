using Server.Items;

namespace Server.Engines.Quests;

public class TheJoysOfLifeQuest : BaseQuest
{
	public TheJoysOfLifeQuest()
	{
		AddObjective(new ObtainObjective(typeof(AriellesBauble), "Arielle's Baubles", 3, 0x23B));

		AddReward(new BaseReward(1072809)); // The boon of Arielle.
	}

	public override bool ForceRemember => true;
	/* The Joys of Life */
	public override object Title => 1072787;
	/* *giggle*  So serious, so grim!  *tickle*  Enjoy life!  Have fun!  Laugh!  Be merry!  *giggle*  Find three 
    of my baubles ... *giggle* I hid them! *giggles hysterically*  Hid them!  La la la!  Bring them quickly!  They 
    are magical and will hide themselves again if you are too slow. */
	public override object Description => 1072832;
	/* *giggle* Too serious.  Too thinky! */
	public override object Refuse => 1072833;
	/* Magical baubles hidden, find them as you're bidden!  *giggle* */
	public override object Uncomplete => 1072834;
	/* *giggle* So pretty! */
	public override object Complete => 1074177;
	public override void GiveRewards()
	{
		base.GiveRewards();

		Owner.SendLocalizedMessage(1074944, null, 0x23); // You have gained the boon of Arielle!  You have been taught the importance of laughter and light spirits.  You are one step closer to claiming your elven heritage.
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