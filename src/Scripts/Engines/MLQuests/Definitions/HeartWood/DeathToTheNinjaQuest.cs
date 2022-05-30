using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class DeathToTheNinjaQuest : BaseQuest
	{
		public DeathToTheNinjaQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(EliteNinja), "Elite Ninjas", 10, "TheCitadel"));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Death to the Ninja! */
		public override object Title => 1072913;
		/* I wish to make a statement of censure against the elite ninjas of the Black Order.  Deliver, in 
        the strongest manner, my disdain.  But do not make war on women, even those that take arms against 
        you.  It is not ... fitting. */
		public override object Description => 1072967;
		/* As you wish. */
		public override object Refuse => 1072979;
		/* The Black Order's fortress home is well hidden.  Legend has it that a humble fishing village 
        disguises the magical portal. */
		public override object Uncomplete => 1072980;
		public override bool CanOffer()
		{
			return MondainsLegacy.Citadel;
		}

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
