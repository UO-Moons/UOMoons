using Server.Items;
using System;

namespace Server.Engines.Quests
{
	public class PatienceQuest : BaseQuest
	{
		public PatienceQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(MiniatureMushroom), "Miniature Mushrooms", 20, 0xD16, 3600));

			AddReward(new BaseReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}

		public override QuestChain ChainId => QuestChain.Spellweaving;
		public override Type NextQuest => typeof(NeedsOfManyPartOneQuest);
		/* Patience */
		public override object Title => 1072753;
		/* Learning to weave spells and control the forces of nature requires sacrifice, 
        discipline, focus, and an unwavering dedication to Sosaria herself.  We do not 
        teach the unworthy.  They do not comprehend the lessons nor the dedication 
        required.  If you would walk the path of the Arcanist, then you must do as I 
        require without hesitation or question.  Your first task is to gather miniature 
        mushrooms ... 20 of them from the branches of our mighty home.  I give you one 
        hour to complete the task. */
		public override object Description => 1072762;
		/* * nods* Not everyone has the temperment to undertake the way of the Arcanist. */
		public override object Refuse => 1072767;
		/* The mushrooms I seek can be found growing here in The Heartwood. Seek them out 
        and gather them.  You are running out of time. */
		public override object Uncomplete => 1072774;
		/* Have you gathered the mushrooms? */
		public override object Complete => 1074166;
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
