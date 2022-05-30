using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class CrimeAndPunishmentQuest : BaseQuest
	{
		public CrimeAndPunishmentQuest()
			: base()
		{
			AddObjective(new InternalObjective());

			AddReward(new BaseReward(typeof(TreasureBag), 1072583));
		}

		/* Crime and Punishment */
		public override object Title => 1072914;
		/* The Tiger's Claw sect members have gone too far! Express to them my displeasure by slaying 
        ten of them. But remember, I do not condone war on women, so I will only accept the deaths of 
        men, human and elf. */
		public override object Description => 1072968;
		/* As you wish. */
		public override object Refuse => 1072979;
		/* The Black Order's fortress home is well hidden.  Legend has it that a humble fishing village 
        disguises the magical portal. */
		public override object Uncomplete => 1072980;
		public override bool CanOffer()
		{
			return MondainsLegacy.Citadel;
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

		private class InternalObjective : SlayObjective
		{
			public InternalObjective()
				: base(typeof(TigersClawThief), "Male Tiger's Claw Thieves", 10, "TheCitadel")
			{
			}

			public override bool IsObjective(Mobile mob)
			{
				if (mob.Female)
					return false;

				return base.IsObjective(mob);
			}
		}
	}
}
