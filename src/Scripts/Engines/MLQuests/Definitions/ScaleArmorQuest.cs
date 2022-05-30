using Server.Items;

namespace Server.Engines.Quests
{
	public class ScaleArmorQuest : BaseQuest
	{
		public ScaleArmorQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(ThrashersTail), "Thrasher's Tail", 1));
			AddObjective(new ObtainObjective(typeof(HydraScale), "Hydra Scales", 10));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Scale Armor */
		public override object Title => 1074711;
		/* Here's what I need ... there are some creatures called hydra, fearsome beasts, whose scales are especially 
        suitable for a new sort of armor that I'm developing.  I need a few such pieces and then some supple alligator 
        skin for the backing.  I'm going to need a really large piece that's shaped just right ... the tail I think 
        would do nicely.  I appreciate your help. */
		public override object Description => 1074712;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* Hydras have been spotted in the Blighted Grove.  You won't get those scales without getting your feet wet, 
        I'm afraid. */
		public override object Uncomplete => 1074724;
		/* I can't wait to get to work now that you've returned with my scales. */
		public override object Complete => 1074725;
		public override bool CanOffer()
		{
			return MondainsLegacy.BlightedGrove;
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
}
