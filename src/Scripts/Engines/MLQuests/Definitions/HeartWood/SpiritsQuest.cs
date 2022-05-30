using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class SpiritsQuest : BaseQuest
	{
		public SpiritsQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Spectre), "Spectres", 15));
			AddObjective(new SlayObjective(typeof(Shade), "Shades", 15));
			AddObjective(new SlayObjective(typeof(Wraith), "Wraiths", 15));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		public override bool AllObjectives => false;
		/* Spirits */
		public override object Title => 1073076;
		/* It is a piteous thing when the dead continue to walk the earth. Restless spirits are known to inhabit these 
        parts, taking the lives of unwary travelers. It is about time a hero put the dead back in their graves. I'm sure 
        such a hero would be justly rewarded. */
		public override object Description => 1073566;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* The restless spirts still walk -- you must kill 15 of them. */
		public override object Uncomplete => 1073586;
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
