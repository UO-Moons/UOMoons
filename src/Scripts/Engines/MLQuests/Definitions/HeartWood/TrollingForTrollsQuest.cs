using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class TrollingForTrollsQuest : BaseQuest
	{
		public TrollingForTrollsQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Troll), "Trolls", 10));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Trolling for Trolls */
		public override object Title => 1072985;
		/* They may not be bright, but they're incredibly destructive. Kill off ten trolls and I'll 
        consider it a favor done for me. */
		public override object Description => 1073014;
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
