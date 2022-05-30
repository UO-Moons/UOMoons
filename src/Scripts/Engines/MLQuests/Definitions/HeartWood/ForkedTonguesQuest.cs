using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ForkedTonguesQuest : BaseQuest
	{
		public ForkedTonguesQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Lizardman), "Lizardmen", 10));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Forked Tongues */
		public override object Title => 1072984;
		/* You can't trust them, you know.  Lizardmen I mean.  They have forked tongues ... and you know 
        what that means.  Exterminate ten of them and I'll reward you. */
		public override object Description => 1073013;
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
