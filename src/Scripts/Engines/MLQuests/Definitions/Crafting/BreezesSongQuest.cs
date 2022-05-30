using Server.Items;

namespace Server.Engines.Quests
{
	public class BreezesSongQuest : BaseQuest
	{
		public BreezesSongQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(FancyWindChimes), "fancy wind chimes", 10, 0x2833));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* Breeze's Song */
		public override object Title => 1074052;
		/* I understand humans cruely enslave the very wind to their selfish whims! Fancy wind chimes, what a monstrous 
        idea! You must bring me proof of this terrible depredation - hurry, bring me wind chimes! */
		public override object Description => 1074146;
		/* Fine then, I'm shall find another to run my errands then. */
		public override object Refuse => 1074063;
		/* Hurry up! I don't have all day to wait for you to bring what I desire! */
		public override object Uncomplete => 1074064;
		/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
		public override object Complete => 1074065;
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
}
