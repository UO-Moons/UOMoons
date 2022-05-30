using Server.Items;

namespace Server.Engines.Quests
{
	public class TheSongOfTheWindQuest : BaseQuest
	{
		public TheSongOfTheWindQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(FancyWindChimes), "Fancy Wind Chimes", 10, 0x2833));

			AddReward(new BaseReward(typeof(TinkersCraftsmanSatchel), 1074282));
		}

		/* The Song of the Wind */
		public override object Title => 1073910;
		/* To give voice to the passing wind, this is an idea worthy of an elf! Friend, bring me some of the amazing fancy 
        wind chimes so that I may listen to the song of the passing breeze. Do this, and I will share with you treasured 
        elven secrets. */
		public override object Description => 1074100;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me fancy wind chimes. */
		public override object Uncomplete => 1073956;
		/* Such a delightful sound, I think I shall never tire of it. */
		public override object Complete => 1073980;
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
