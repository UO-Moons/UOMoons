using Server.Items;
using System;

namespace Server.Engines.Quests
{
	public class ClockworkPuzzleQuest : BaseQuest
	{
		public ClockworkPuzzleQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(ClockParts), "Clock Parts", 5, 0x104F));

			AddReward(new BaseReward(typeof(TinkersSatchel), 1074282)); // Craftsman's Satchel
		}

		public override TimeSpan RestartDelay => TimeSpan.FromMinutes(3);
		/* A clockwork puzzle */
		public override object Title => 1075535;
		/* 'Tis a riddle, you see! "What kind of clock is only right twice per day? A broken one!" *laughs heartily* 
        Ah, yes *wipes eye*, that's one of my favorites! Ah... to business. Could you fashion me some clock parts? 
        I wish my own clocks to be right all the day long! You'll need some tinker's tools and some iron ingots, I 
        think, but from there it should be just a matter of working the metal. */
		public override object Description => 1075534;
		/* Or perhaps you'd rather not. */
		public override object Refuse => 1072981;
		/* You're not quite done yet.  Get back to work! */
		public override object Uncomplete => 1072271;
		/* Wonderful! Tick tock, tick tock, soon all shall be well with grandfather's clock! */
		public override object Complete => 1075536;
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
