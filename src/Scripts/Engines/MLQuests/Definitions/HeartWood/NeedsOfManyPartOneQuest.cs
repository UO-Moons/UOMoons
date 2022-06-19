using Server.Items;
using System;

namespace Server.Engines.Quests
{
	public class NeedsOfManyPartOneQuest : BaseQuest
	{
		public NeedsOfManyPartOneQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Cotton), "Bale of Cotton", 10, 0xDF9));

			AddReward(new BaseReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}

		public override QuestChain ChainId => QuestChain.Spellweaving;
		public override Type NextQuest => typeof(NeedsOfManyPartTwoQuest);
		/* Needs of the Many - The Heartwood */
		public override object Title => 1072797;
		/* The way of the Arcanist involves cooperation with others and a strong 
        committment to the community of your people.  We have run low on the 
        cotton we use to pack wounds and our people have need.  Bring 10 
        bales of cotton to me. */
		public override object Description => 1072763;
		/* You endanger your progress along the path with your unwillingness. */
		public override object Refuse => 1072768;
		/* I care not where you acquire the cotton, merely that you provide it. */
		public override object Uncomplete => 1072775;
		/* Well, where are the cotton bales? */
		public override object Complete => 1074110;
		public override bool CanOffer()
		{
			return MondainsLegacy.Spellweaving;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
