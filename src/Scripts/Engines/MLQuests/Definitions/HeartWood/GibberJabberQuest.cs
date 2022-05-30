using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class GibberJabberQuest : BaseQuest
	{
		public GibberJabberQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Gibberling), "Gibberlings", 10));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Gibber Jabber */
		public override object Title => 1073004;
		/* I bet you can't kill ... ten gibberlings!  I bet they're too much for you.  You may as 
        well confess you can't ... */
		public override object Description => 1073023;
		/* Hahahaha!  I knew it! */
		public override object Refuse => 1073019;
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
