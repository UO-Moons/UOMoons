using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class MongbatMenaceQuest : BaseQuest
	{
		public MongbatMenaceQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Mongbat), "Mongbats", 10));
			AddObjective(new SlayObjective(typeof(GreaterMongbat), "Greater Mongbats", 4));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Mongbat Menace! */
		public override object Title => 1073003;
		/* I imagine you don't know about the mongbats.  Well, you may think you do, but I know more than just about anyone.  
        You see they come in two varieties ... the stronger and the weaker.  Either way, they're a menace.  Exterminate ten 
        of the weaker ones and four of the stronger and I'll pay you an honest wage. */
		public override object Description => 1073033;
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
