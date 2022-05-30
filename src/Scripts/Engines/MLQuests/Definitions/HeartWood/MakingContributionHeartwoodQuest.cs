using Server.Items;
using System;

namespace Server.Engines.Quests
{
	public class MakingContributionHeartwoodQuest : BaseQuest
	{
		public MakingContributionHeartwoodQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(SackFlour), "Sack of Flour", 1, 0x1039));
			AddObjective(new ObtainObjective(typeof(JarHoney), "Jar of Honey", 10, 0x9EC));
			AddObjective(new ObtainObjective(typeof(FishSteak), "Fish Steak", 20, 0x97B));

			AddReward(new BaseReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}

		public override QuestChain ChainID => QuestChain.Spellweaving;
		public override Type NextQuest => typeof(UnnaturalCreationsQuest);
		/* Making a Contribution - The Heartwood */
		public override object Title => 1072798;
		/* With health and defense assured, we need look to the need of the community 
        for food and drink.  We will feast on fish steaks, sweets, and wine.  You 
        will supply the ingredients, the cooks will prepare the meal.  As a Arcanist 
        relies upon others to build focus and lend their power to her workings, the 
        community needs the effort of all to survive. */
		public override object Description => 1072765;
		/* Do not falter now.  You have begun to show promise. */
		public override object Refuse => 1072770;
		/* Where are the items you've been tasked to supply for the feast? */
		public override object Uncomplete => 1072777;
		/* Ah good, you're back.  We're eager for the feast. */
		public override object Complete => 1074158;
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
