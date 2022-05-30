using Server.Items;

namespace Server.Engines.Quests
{
	public class TappingTheKegQuest : BaseQuest
	{
		public TappingTheKegQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(BarrelTap), "barrel taps", 10, 0x1004));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* Tapping the Keg */
		public override object Title => 1074037;
		/* I have acquired a barrel of human brewed beer. I am loathe to drink it, but how else to prove how 
        inferior it is? I suppose I shall need a barrel tap to drink. Go, bring me a barrel tap quickly, so 
        I might get this over with. */
		public override object Description => 1074131;
		/* Fine then, I'm shall find another to run my errands then. */
		public override object Refuse => 1074063;
		/* Hurry up! I don't have all day to wait for you to bring what I desire! */
		public override object Uncomplete => 1074064;
		/* These human made goods are laughable! It offends so -- I must show you what elven skill is capable of! */
		public override object Complete => 1074065;
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
