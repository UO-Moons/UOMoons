using Server.Items;

namespace Server.Engines.Quests
{
	public class FromTheGaultierCollectionQuest : BaseQuest
	{
		public FromTheGaultierCollectionQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(StuddedBustierArms), "Studded Bustiers", 10, 0x1C0C));

			AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
		}

		/* From the Gaultier Collection */
		public override object Title => 1073905;
		/* It is my understanding, the females of humankind actually wear on certain occasions a 
        studded bustier? This is not simply a fanciful tale? Remarkable! It sounds hideously 
        uncomfortable as well as ludicrously impracticle. But perhaps, I simply do not understand 
        the nuances of human clothing. Perhaps, I need to see such a studded bustier for myself? */
		public override object Description => 1074095;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me studded bustiers. */
		public override object Uncomplete => 1073951;
		/* Truly, it is worse than I feared. Still, I appreciate your efforts on my behalf. */
		public override object Complete => 1073976;
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
