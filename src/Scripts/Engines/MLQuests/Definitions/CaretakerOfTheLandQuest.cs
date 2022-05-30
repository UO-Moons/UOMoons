using Server.Items;

namespace Server.Engines.Quests
{
	public class CaretakerOfTheLandQuest : BaseQuest
	{
		public CaretakerOfTheLandQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(SosariaSap), "Sap of Sosaria", 1, 0x1848));

			AddReward(new BaseReward(1072804)); // The boon of Strongroot.
		}

		public override bool ForceRemember => true;
		/* Caretaker of the Land */
		public override object Title => 1072783;
		/* Hrrrrr.  Hurrrr.  Huuuman.  *creaking branches*  Suuun on baaark, roooooots diiig deeeeeep, wiiind caaaresses 
        leeeaves … Hrrrrr.  Saaap of Sooosaria feeeeeeds us.  Hrrrrr.  Huuuman leeearn.  Caaaretaker of plaaants … teeend 
        … prooove.<br> */
		public override object Description => 1072812;
		/* Hrrrrr.  Hrrrrr.  Huuuman. */
		public override object Refuse => 1072813;
		/* Hrrrr. Hrrrr.  Roooooots neeeeeed saaap of Sooosaria.  Hrrrrr.  Roooooots tiiingle neeeaaar Yeeew.  Seeeaaarch.  Hrrrr! */
		public override object Uncomplete => 1072814;
		/* Thiiirsty. Hurrr. Hurrr. */
		public override object Complete => 1074175;
		public override void GiveRewards()
		{
			base.GiveRewards();

			Owner.SendLocalizedMessage(1074941, null, 0x23); // You have gained the boon of Strongroot!  You have been approved by one whose roots touch the bones of Sosaria.  You are one step closer to claiming your elven heritage.
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
