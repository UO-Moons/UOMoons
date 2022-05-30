using Server.Items;

namespace Server.Engines.Quests
{
	public class TheKingOfClothingQuest : BaseQuest
	{
		public TheKingOfClothingQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(Kilt), "Kilts", 10, 0x1537));

			AddReward(new BaseReward(typeof(TailorsCraftsmanSatchel), 1074282));
		}

		/* The King of Clothing */
		public override object Title => 1073902;
		/* I have heard noble tales of a fine and proud human garment. An article of clothing 
        fit for both man and god alike. It is called a "kilt" I believe? Could you fetch for 
        me some of these kilts so I that I might revel in their majesty and glory? */
		public override object Description => 1074092;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me kilts. */
		public override object Uncomplete => 1073948;
		/* I say truly - that is a magnificent garment! You have more than earned a reward. */
		public override object Complete => 1073974;
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
