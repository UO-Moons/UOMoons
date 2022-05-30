using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ItsElementalQuest : BaseQuest
	{
		public ItsElementalQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(FireElemental), "Fire Elementals", 4));
			AddObjective(new SlayObjective(typeof(WaterElemental), "Water Elementals", 4));
			AddObjective(new SlayObjective(typeof(EarthElemental), "Earth Elementals", 4));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* It's Elemental */
		public override object Title => 1073089;
		/* The universe is all about balance my friend. Tip one end, you must balance the other. That's 
        why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, 
        douse a few Water, and crush some Earth elementals and I'll pay you for your trouble. */
		public override object Description => 1073579;
		/* I hope you'll reconsider. Until then, farwell.	 */
		public override object Refuse => 1073580;
		/* Four of each, that's all I ask. Water, earth and fire. */
		public override object Uncomplete => 1073599;
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
