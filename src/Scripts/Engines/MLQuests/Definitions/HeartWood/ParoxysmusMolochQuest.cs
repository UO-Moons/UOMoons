using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ParoxysmusMolochQuest : BaseQuest
	{
		public ParoxysmusMolochQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(Moloch), "Molochs", 3, "Palace of Paroxysmus"));

			AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
		}

		/* Paroxysmus' Moloch */
		public override object Title => 1073068;
		/* The moloch daemons that have congregated to worship Paroxysmus pose a tremendous danger. Will 
        you enter the lair and see to their destruction? */
		public override object Description => 1074695;
		/* Well, okay. But if you decide you are up for it after all, c'mon back and see me. */
		public override object Refuse => 1072270;
		/* You're not quite done yet.  Get back to work! */
		public override object Uncomplete => 1072271;
		public override bool CanOffer()
		{
			return MondainsLegacy.PalaceOfParoxysmus;
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
