using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class BigWormsQuest : BaseQuest
	{
		public BigWormsQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(IceSerpent), "Giant Ice Serpents", 10));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Big Worms */
		public override object Title => 1073088;
		/* It makes no sense! Cold blooded serpents cannot live in the ice! It's a biological impossibility! 
        They are an abomination against reason! Please, I beg you - kill them! Make them disappear for me! Do 
        this and I will reward you. */
		public override object Description => 1073578;
		/* I hope you'll reconsider. Until then, farwell. */
		public override object Refuse => 1073580;
		/* You wouldn't try and just pretend you murdered 10 Giant Ice Worms, would you? */
		public override object Uncomplete => 1073598;
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
