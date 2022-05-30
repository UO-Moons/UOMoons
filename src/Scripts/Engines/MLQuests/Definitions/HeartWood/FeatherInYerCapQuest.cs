using Server.Items;

namespace Server.Engines.Quests
{
	public class FeatherInYerCapQuest : BaseQuest
	{
		public FeatherInYerCapQuest()
			: base()
		{
			AddObjective(new ObtainObjective(typeof(SalivasFeather), "Saliva's Feather", 1));

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* A Feather in Yer Cap */
		public override object Title => 1074738;
		/* I've seen how you strut about, as if you were something special. I have some news for you, you don't impress 
        me at all. It's not enough to have a fancy hat you know.  That may impress people in the big city, but not here. 
        If you want a reputation you have to climb a mountain, slay some great beast, and then write about it. Trust me, 
        it's a long process.  The first step is doing a great feat. If I were you, I'd go pluck a feather from the harpy 
        Saliva, that would give you a good start. */
		public override object Description => 1074737;
		/* The path to greatness isn't for everyone obviously. */
		public override object Refuse => 1074736;
		/* If you're going to get anywhere in the adventuring game, you have to take some risks.  A harpy, well, it's 
        bad, but it's not a dragon. */
		public override object Uncomplete => 1074735;
		/* The hero returns from the glorious battle and - oh, such a small feather? */
		public override object Complete => 1074734;
		public override bool CanOffer()
		{
			return MondainsLegacy.BlightedGrove;
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
