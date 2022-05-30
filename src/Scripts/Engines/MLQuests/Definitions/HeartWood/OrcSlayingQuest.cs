using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class OrcSlayingQuest : BaseQuest
	{
		public OrcSlayingQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Orc), "Orcs", 8));
			AddObjective(new SlayObjective(typeof(OrcCaptain), "Orc Captains", 4));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Orc Slaying */
		public override object Title => 1072986;
		/* Those green-skinned freaks have run off with more of my livestock.  I want an orc scout 
        killed for each sheep I lost and an orc for each chicken.  So that's four orc scouts and 
        eight orcs I'll pay you to slay. */
		public override object Description => 1073015;
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
