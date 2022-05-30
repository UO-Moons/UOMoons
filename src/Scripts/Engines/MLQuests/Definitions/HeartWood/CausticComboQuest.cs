using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class CausticComboQuest : BaseQuest
	{
		public CausticComboQuest()
			: base()
		{
			AddObjective(new SlayObjective(typeof(PoisonElemental), "Poison Elementals", 3, "Palace of Paroxysmus"));
			AddObjective(new SlayObjective(typeof(AcidElemental), "Acid Elementals", 6, "Palace of Paroxysmus"));

			AddReward(new BaseReward(typeof(LargeTreasureBag), 1072706));
		}

		/* Caustic Combo */
		public override object Title => 1073062;
		/* Vile creatures have exited the sinkhole and begun terrorizing the surrounding area.  The demons are bad enough, 
        but the elementals are an abomination, their poisons seeping into the fertile ground here.  Will you enter the 
        sinkhole and put a stop to their depredations? */
		public override object Description => 1074693;
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
