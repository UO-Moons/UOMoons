using Server.Items;

namespace Server.Engines.Quests
{
	public class ArchSupportQuest : BaseQuest
	{
		public ArchSupportQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(FootStool), "Foot Stools", 10, 0xB5E));

			AddReward(new BaseReward(typeof(CarpentersCraftsmanSatchel), 1074282));
		}

		/* Arch Support */
		public override object Title => 1073882;
		/* How clever humans are - to understand the need of feet to rest from time to time!  Imagine creating 
        a special stool just for weary toes.  I would like to examine and learn the secret of their making.  
        Would you bring me some foot stools to examine? */
		public override object Description => 1074072;
		/* I will patiently await your reconsideration. */
		public override object Refuse => 1073921;
		/* I will be in your debt if you bring me foot stools. */
		public override object Uncomplete => 1073928;
		/* My thanks for your service. Now, I will show you something of elven carpentry. */
		public override object Complete => 1073969;
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
