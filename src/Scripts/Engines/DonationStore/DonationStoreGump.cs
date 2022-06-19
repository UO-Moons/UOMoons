using Server.Engines.PlayerDonation;
using Server.Network;
using System.Collections;

namespace Server.Gumps;

public class DonationStoreGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly long[] _mGiftIDs = new long[5];

	public DonationStoreGump(Mobile from)
		: base(0, 0)
	{
		_mFrom = from;

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
			if (giftInfo == null) continue;
			_mGiftIDs[i - 1] = giftInfo.Id;
			AddGiftOption(giftInfo.Name, i, offset);
		}
	}

	private void AddGiftOption(string itemName, int index, int offset)
	{
		AddButton(62, 166 + offset * (index - 1), 4005, 4007, index, GumpButtonType.Reply, index);    //check this to get the gift
		AddLabel(94, 166 + offset * (index - 1), 0, itemName);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (info.ButtonID > 0 && info.ButtonID <= 5)
		{
			_mFrom.CloseGump(typeof(DonationStoreGump));

			long giftId = _mGiftIDs[info.ButtonID - 1];
			IEntity gift = DonationStore.RedeemGift(giftId, _mFrom.Account.Username);
			if (gift == null) return;
			_mFrom.AddToBackpack((Item)gift);
			_mFrom.SendMessage("{0} has been placed in your backpack. Thank you for your donation!", ((Item)gift).Name);
		}
	}

}
