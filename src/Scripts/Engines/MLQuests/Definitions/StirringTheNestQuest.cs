using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class StirringTheNestQuest : BaseQuest
	{
		public StirringTheNestQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(RedSolenQueen), "Red Solen Queens", 3));
			AddObjective(new SlayObjective(typeof(BlackSolenQueen), "Black Solen Queens", 3));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		public override bool AllObjectives => false;
		/* Stirring the Nest */
		public override object Title => 1073087;
		/* Were you the sort of child that enjoyed knocking over anthills? Well, perhaps you'd like 
        to try something a little bigger? There's a Solen nest nearby and I bet if you killed a queen 
        or two, it would be quite the sight to behold.  I'd even pay to see that - what do you say? */
		public override object Description => 1073577;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* Dead Solen Queens isn't too much to ask, is it? */
		public override object Uncomplete => 1073597;
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
