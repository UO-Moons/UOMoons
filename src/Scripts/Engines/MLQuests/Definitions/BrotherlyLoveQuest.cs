using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class BrotherlyLoveQuest : BaseQuest
	{
		public BrotherlyLoveQuest()
			: base()
		{
			AddObjective(new DeliverObjective(typeof(PersonalLetterAhie), "Letter", 1, typeof(Ahie), "Ahie (The Heartwood)", 1800));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		public override bool DoneOnce => true;
		/* Brotherly Love */
		public override object Title => 1072369;
		/* *looks around nervously*  Do you travel to The Heartwood?  I have an urgent letter that must be delivered 
        there in the next 30 minutes - to Ahie the Cloth Weaver.  Will you undertake this journey? */
		public override object Description => 1072585;
		/* *looks disappointed* Let me know if you change your mind. */
		public override object Refuse => 1072587;
		/* You haven't lost the letter have you?  It must be delivered to Ahie directly.  Give it into no other hands. */
		public override object Uncomplete => 1072588;
		/* Yes, can I help you? */
		public override object Complete => 1074579;
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
