using Server.Items;
using System;

namespace Server.Misc
{
	public class Christmas : GiftGiver
	{
		private static readonly DateTime m_Current = DateTime.UtcNow; // returns year
		public override DateTime Start => new(m_Current.Year, 12, 24);
		public override DateTime Finish => new(m_Current.Year, 1, 1);

		private static readonly bool Enabled2004 = Settings.Configuration.Get<bool>("Holidays", "Christmas2004");

		public static void Initialize()
		{
			GiftGiving.Register(new Christmas());
		}

		public override void GiveGift(Mobile mob)
		{
			if (Enabled2004)
			{
				GiftBox box = new();

				box.DropItem(new MistletoeDeed());
				box.DropItem(new PileOfGlacialSnow());
				box.DropItem(new LightOfTheWinterSolstice());

				int random = Utility.Random(100);

				switch (random)
				{
					case < 60:
						box.DropItem(new DecorativeTopiary());
						break;
					case < 84:
						box.DropItem(new FestiveCactus());
						break;
					default:
						box.DropItem(new SnowyTree());
						break;
				}

				switch (GiveGift(mob, box))
				{
					case GiftResult.Backpack:
						mob.SendMessage(0x482,
							"Happy Holidays from the team!  Gift items have been placed in your backpack.");
						break;
					case GiftResult.BankBox:
						mob.SendMessage(0x482,
							"Happy Holidays from the team!  Gift items have been placed in your bank box.");
						break;
				}
			}
		}
	}
}
