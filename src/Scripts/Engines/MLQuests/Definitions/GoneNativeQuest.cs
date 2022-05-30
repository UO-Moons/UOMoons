using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class GoneNativeQuest : BaseQuest
	{
		public GoneNativeQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(MasterTheophilus), "Master Theophilus", 1));

			AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
		}

		/* Gone Native */
		public override object Title => 1074855;
		/* Pathetic really.  I must say, a senior instructor going native -- forgetting about his students and 
        peers and engaging in such disgraceful behavior!  I'm speaking, of course, of Theophilus.  Master Theophilus 
        to you. He may have gone native but he still holds a Mastery Degree from Bedlam College!  But, well, that's 
        neither here nor there.  I need you to take care of my colleague.  Convince him of the error of his ways.  
        He may resist.  In fact, assume he will and kill him.  We'll get him resurrected and be ready to cure his 
        folly.  What do you say? */
		public override object Description => 1074856;
		/* I understand.  A Master of Bedlam, even one entirely off his rocker, is too much for you to handle. */
		public override object Refuse => 1074857;
		/* You had better get going.  Master Theophilus isn't likely to kill himself just to save me this embarrassment. */
		public override object Uncomplete => 1074858;
		/* You look a bit worse for wear!  He put up a good fight did he?  Hah!  That's the spirit … a Master 
        of Bedlam is a match for most. */
		public override object Complete => 1074859;
		public override bool CanOffer()
		{
			return MondainsLegacy.Bedlam;
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
