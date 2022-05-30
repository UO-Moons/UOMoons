using Server.Items;
using System;

namespace Server.Engines.Quests
{
	public class NeedsOfManyPartTwoQuest : BaseQuest
	{
		public NeedsOfManyPartTwoQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Board), "Boards", 250, 0x1BD7));

			AddReward(new BaseReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}

		public override QuestChain ChainID => QuestChain.Spellweaving;
		public override Type NextQuest => typeof(MakingContributionHeartwoodQuest);
		/* Needs of the Many - The Heartwood */
		public override object Title => 1072797;
		/* We must look to the defense of our people! Bring boards for new arrows. */
		public override object Description => 1072764;
		/* The people have need of these items.  You are proving yourself inadequate 
        to the demands of a member of this community. */
		public override object Refuse => 1072769;
		/* The requirements are simple -- 250 boards. */
		public override object Uncomplete => 1072776;
		/* Well, where are the boards? */
		public override object Complete => 1074152;
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
