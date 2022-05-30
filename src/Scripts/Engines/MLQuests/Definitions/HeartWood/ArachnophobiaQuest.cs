using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ArachnophobiaQuest : BaseQuest
	{
		public ArachnophobiaQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(GiantBlackWidow), "Giant Black Widows", 12));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Arachnophobia */
		public override object Title => 1073079;
		/* I've seen them hiding in their webs among the woods. Glassy eyes, spindly legs, poisonous fangs. Monsters, 
        I say! Deadly horrors, these black widows. Someone must exterminate the abominations! If only I could find a 
        worthy hero for such a task, then I could give them this considerable reward. */
		public override object Description => 1073569;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* You've got a good start, but to stop the black-eyed fiends, you need to kill a dozen. */
		public override object Uncomplete => 1073589;
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
