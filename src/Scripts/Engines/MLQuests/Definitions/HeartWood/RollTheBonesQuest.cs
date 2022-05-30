using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class RollTheBonesQuest : BaseQuest
	{
		public RollTheBonesQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(PatchworkSkeleton), "Patchwork Skeletons", 8));

			AddReward(new BaseReward(typeof(TrinketBag), 1072341));
		}

		/* Roll the Bones */
		public override object Title => 1073002;
		/* Why?  I ask you why?  They walk around after they're put in the ground.  It's just wrong in so many ways.  
        Put them to proper rest, I beg you.  I'll find some way to pay you for the kindness. Just kill eight patchwork 
        skeletons. */
		public override object Description => 1073011;
		/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
		public override object Refuse => 1072270;
		/* You're not quite done yet.  Get back to work! */
		public override object Uncomplete => 1072271;
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
