using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class SpecimensQuest : BaseQuest
	{
		public SpecimensQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(RedSolenWorker), "Red Solen Workers", 12));
			AddObjective(new SlayObjective(typeof(BlackSolenWorker), "Black Solen Workers", 12));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		public override bool AllObjectives => false;
		/* Specimens */
		public override object Title => 1072999;
		/* I admire them, you know.  The solen have their place -- regimented, organized.  They're fascinating 
        to watch with their constant strife between red and black.  I can't help but want to stir things up from 
        time to time.  And that's where you come in.  Kill either twelve red or twelve black solen workers and 
        let's see what happens next! */
		public override object Description => 1073032;
		/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
		public override object Refuse => 1072270;
		/* You're not quite done yet.  Get back to work! */
		public override object Uncomplete => 1072271;
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
