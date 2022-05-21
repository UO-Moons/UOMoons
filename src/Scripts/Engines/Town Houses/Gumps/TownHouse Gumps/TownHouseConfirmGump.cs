using System;

namespace Server.Engines.TownHouses
{
	public class TownHouseConfirmGump : GumpPlusLight
	{
		private readonly TownHouseSign m_CSign;
		private bool m_CItems;

		public TownHouseConfirmGump(Mobile m, TownHouseSign sign) : base(m, 100, 100)
		{
			m_CSign = sign;
		}

		protected override void BuildGump()
		{
			const int width = 200;
			var y = 0;

			AddHtml(0, y += 10, width, $"<CENTER>{(m_CSign.RentByTime == TimeSpan.Zero ? "Purchase" : "Rent")} this House?");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			if (m_CSign.RentByTime == TimeSpan.Zero)
			{
				AddHtml(0, y += 25, width, $"<CENTER>Price: {(m_CSign.Free ? "Free" : "" + m_CSign.Price)}");
			}
			else if (m_CSign.RecurRent)
			{
				AddHtml(0, y += 25, width, $"<CENTER>{"Recurring " + m_CSign.PriceType}: {m_CSign.Price}");
			}
			else
			{
				AddHtml(0, y += 25, width, $"<CENTER>{"One " + m_CSign.PriceTypeShort}: {m_CSign.Price}");
			}

			if (m_CSign.KeepItems)
			{
				AddHtml(0, y += 20, width, "<CENTER>Cost of Items: " + m_CSign.ItemsPrice);
				AddButton(20, y, m_CItems ? 0xD3 : 0xD2, "Items", Items);
			}

			AddHtml(0, y += 20, width, "<CENTER>Lockdowns: " + m_CSign.Locks);
			AddHtml(0, y += 20, width, "<CENTER>Secures: " + m_CSign.Secures);

			AddButton(10, y += 25, 0xFB1, 0xFB3, "Cancel", Cancel);
			AddButton(width - 40, y, 0xFB7, 0xFB9, "Confirm", Confirm);

			AddBackgroundZero(0, 0, width, y + 40, 0x13BE);
		}

		private void Items()
		{
			m_CItems = !m_CItems;

			NewGump();
		}

		private void Cancel()
		{
		}

		private void Confirm()
		{
			m_CSign.Purchase(Owner, m_CItems);
		}
	}
}
