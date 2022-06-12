using Server.Accounting;
using Server.Engines.PlayerDonation;
using Server.Gumps;
using Server.Network;
using System.Collections;

namespace Server.Items
{
	public class DonationGiftStone : Item
	{
		public override string DefaultName
		{
			get { return "Double click this stone to redeem your donation gift here"; }
		}

		[Constructable]
		public DonationGiftStone() : base(0xED4)
		{
			Movable = false;
			Hue = 0x489;
		}

		public override void OnDoubleClick(Mobile from)
		{
			//check database for this player's account
			Account account = from.Account as Account;
			string accountName = account.Username;

			from.SendGump(new DonationStoreGump(from));
		}

		public DonationGiftStone(Serial serial) : base(serial)
		{
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



namespace Server.Gumps
{
	public class DonationStoreGump : Gump
	{
		private readonly Mobile m_From;
		private readonly long[] m_GiftIDs = new long[5];

		public DonationStoreGump(Mobile from)
			: base(0, 0)
		{
			m_From = from;

			from.CloseGump(typeof(DonationStoreGump));

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;
			AddPage(0);
			AddBackground(26, 25, 397, 434, 9200);
			AddLabel(141, 34, 38, @"Donation Store");
			AddHtml(62, 62, 325, 60, @"If you have donated to this shard, you can retrieve your item here. Thank you for keeping this shard running!", true, true);
			AddLabel(62, 130, 38, @"Select to retrieve your item:");

			GeneratGiftList(from);

		}

		private void GeneratGiftList(Mobile acct)
		{
			string username = acct.Account.Username;
			int offset = 40;

			ArrayList giftList = DonationStore.GetDonationGiftList(username);
			if (giftList.Count == 0)
			{
				AddHtml(62, 162, 325, 60, @"Thank you for playing! You have no donation gift to claim now. Consider donating to this shard to keep this shard running.", true, true);
				return;
			}

			for (int i = 1; i < 6 && i <= giftList.Count; i++)
			{
				DonationGift giftInfo = (DonationGift)giftList[i - 1];
				m_GiftIDs[i - 1] = giftInfo.Id;
				AddGiftOption(giftInfo.Name, i, offset);
			}
		}

		private void AddGiftOption(string itemName, int index, int offset)
		{
			AddButton(62, 166 + (offset * (index - 1)), 4005, 4007, index, GumpButtonType.Reply, index);    //check this to get the gift
			AddLabel(94, 166 + (offset * (index - 1)), 0, itemName);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID > 0 && info.ButtonID <= 5)
			{
				m_From.CloseGump(typeof(DonationStoreGump));

				long giftId = m_GiftIDs[info.ButtonID - 1];
				IEntity gift = DonationStore.RedeemGift(giftId, m_From.Account.Username);
				if (gift != null)
				{
					m_From.AddToBackpack((Item)gift);
					m_From.SendMessage("{0} has been placed in your backpack. Thank you for your donation!", ((Item)gift).Name);
				}
			}
		}

	}
}
