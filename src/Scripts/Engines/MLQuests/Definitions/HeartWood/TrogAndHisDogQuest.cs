using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class TrogAndHisDogQuest : BaseQuest
	{
		public TrogAndHisDogQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Lurg), "Lurg", 1));
			AddObjective(new SlayObjective(typeof(Grobu), "Grobu", 1));

			AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
		}

		/* A Trog and His Dog */
		public override object Title => 1074681;
		/* I don't know if you can handle it, but I'll give you a go at it. Troglodyte chief - name of Lurg and his mangy 
        wolf pet need killing. Do the deed and I'll reward you. */
		public override object Description => 1074680;
		/* Perhaps I thought too highly of you. */
		public override object Refuse => 1074655;
		/* The trog chief and his mutt should be easy enough to find. Just kill them and report back.  Easy enough. */
		public override object Uncomplete => 1074682;
		/* Not half bad.  Here's your prize. */
		public override object Complete => 1074683;
		public override bool CanOffer()
		{
			return MondainsLegacy.PaintedCaves;
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
