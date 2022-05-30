using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class DaemonicPrismQuest : BaseQuest
	{
		public DaemonicPrismQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(CrystalDaemon), "Crystal Daemons", 3, "Prism of Light"));

			AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
		}

		/* Daemonic Prism */
		public override object Title => 1073053;
		/* Good, you're here.  The presence of a twisted creature deep under the earth near Nu'Jelm has 
        corrupted the natural growth of crystals in that region.  They've become infused with the 
        twisting energy - they've come to a sort of life.  This is an abomination that festers within 
        Sosaria. You must eradicate the crystal daemons. */
		public override object Description => 1074668;
		/* These abominations must not be permitted to fester! */
		public override object Refuse => 1074671;
		/* You must not waste time. Do not suffer these crystalline abominations to live. */
		public override object Uncomplete => 1074672;
		/* You have done well.  Enjoy this reward. */
		public override object Complete => 1074673;
		public override bool CanOffer()
		{
			return MondainsLegacy.PrismOfLight;
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
