using Server.Items;

namespace Server.Engines.Quests
{
	public class ThePuffyShirtQuest : BaseQuest
	{
		public ThePuffyShirtQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(FancyShirt), "Fancy Shirts", 10, 0x1EFD));

			AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
		}

		/* The Puffy Shirt */
		public override object Title => 1073903;
		/* We elves believe that beauty is expressed in all things, including the garments we 
        wear. I wish to understand more about human aesthetics, so please kind traveler - could 
        you bring to me magnificent examples of human fancy shirts? For my thanks, I could teach 
        you more about the beauty of elven vestements. */
		public override object Description => 1074093;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me fancy shirts. */
		public override object Uncomplete => 1073949;
		/* I appreciate your service. Now, see what elven hands can create. */
		public override object Complete => 1073973;
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
