using Server.Accounting;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Prompts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Server.Network;

namespace Server.Gumps;

public class CommunityCollectionGump : Gump
{
	private readonly PlayerMobile _mOwner;
	private readonly IComunityCollection _mCollection;
	private readonly Point3D _mLocation;
	private readonly Section _mSection;
	private readonly CollectionHuedItem _mItem;
	private int _mIndex;
	private int _mPage;
	private int _mMax;

	public CommunityCollectionGump(PlayerMobile from, IComunityCollection collection, Point3D location, Section section = Section.Donates)
		: this(from, collection, location, section, null)
	{
	}

	private CommunityCollectionGump(PlayerMobile from, IComunityCollection collection, Point3D location, Section section, CollectionHuedItem item)
		: base(250, 50)
	{
		_mOwner = from;
		_mCollection = collection;
		_mLocation = location;
		_mSection = section;
		_mItem = item;

		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);

		AddImage(0, 0, 0x1F40);
		AddImageTiled(20, 37, 300, 308, 0x1F42);
		AddImage(20, 325, 0x1F43);
		AddImage(35, 8, 0x39);
		AddImageTiled(65, 8, 257, 10, 0x3A);
		AddImage(290, 8, 0x3B);
		AddImage(32, 33, 0x2635);
		AddImageTiled(70, 55, 230, 2, 0x23C5);

		AddHtmlLocalized(70, 35, 270, 20, 1072835, 0x1, false, false); // Community Collection

		// add pages
		if (_mCollection == null)
			return;

		_mIndex = 0;
		_mPage = 1;

		switch (_mSection)
		{
			case Section.Donates:
				GetMax(_mCollection.Donations);

				while (_mCollection.Donations != null && _mIndex < _mCollection.Donations.Count)
					DisplayDonationPage();
				break;
			case Section.Rewards:
				GetMax(_mCollection.Rewards);

				while (_mCollection.Rewards != null && _mIndex < _mCollection.Rewards.Count)
					DisplayRewardPage();
				break;
			case Section.Hues:
				while (_mItem != null && _mIndex < _mItem.Hues.Length)
					DisplayHuePage();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(section));
		}
	}

	public enum Section
	{
		Donates,
		Rewards,
		Hues,
	}

	private enum Buttons
	{
		Rewards,
		Status,
		Next,
	}

	private void GetMax(List<CollectionItem> list)
	{
		_mMax = 0;

		if (list == null) return;
		for (var i = 0; i < list.Count; i++)
		{
			if (_mMax < list[i].Width)
				_mMax = list[i].Width;
		}
	}

	private void DisplayDonationPage()
	{
		AddPage(_mPage);

		// title
		AddHtmlLocalized(50, 65, 150, 20, 1072836, 0x1, false, false); // Current Tier:
		AddLabel(230, 65, 0x64, _mCollection.Tier.ToString());
		AddHtmlLocalized(50, 85, 150, 20, 1072837, 0x1, false, false); // Current Points:
		AddLabel(230, 85, 0x64, _mCollection.Points.ToString());
		AddHtmlLocalized(50, 105, 150, 20, 1072838, 0x1, false, false); // Points Until Next Tier:
		AddLabel(230, 105, 0x64, _mCollection.CurrentTier.ToString());

		AddImageTiled(35, 125, 270, 2, 0x23C5);
		AddHtmlLocalized(35, 130, 270, 20, 1072840, 0x1, false, false); // Donations Accepted:

		// donations
		int offset = 150;
		int next = 0;

		while (offset + next < 330 && _mIndex < _mCollection.Donations.Count)
		{
			CollectionItem item = _mCollection.Donations[_mIndex];

			int height = Math.Max(item.Height, 20);

			int amount;

			/*if (item.Type == typeof(Gold) && acct != null)
                amount = acct.TotalGold + m_Owner.Backpack.GetAmount(item.Type);
            else if (item.Type == typeof(BaseScales))
                amount = GetScales(m_Owner.Backpack);
            else if (item.Type == typeof(Fish))
                amount = GetFishyItems(m_Owner.Backpack);
            else if (item.Type == typeof(Crab) || item.Type == typeof(Lobster))
                amount = GetCrabsAndLobsters(m_Owner.Backpack);
            else if (m_Owner.Backpack != null)
                amount = m_Owner.Backpack.GetAmount(item.Type);*/
			if (item.Type == typeof(Gold) && _mOwner.Account is Account acct)
			{
				amount = acct.TotalGold + _mOwner.Backpack.GetAmount(item.Type);
			}
			else
			{
				amount = GetTypes(_mOwner, item);
			}

			if (amount > 0)
			{
				AddButton(35, offset + height / 2 - 5, 0x837, 0x838, 300 + _mIndex, GumpButtonType.Reply, 0);
				AddTooltip(item.Tooltip);
			}

			int y = offset - item.Y;

			if (item.Height < 20)
				y += (20 - item.Height) / 2;

			AddItem(55 - item.X + _mMax / 2 - item.Width / 2, y, item.ItemId, item.Hue);
			AddTooltip(item.Tooltip);

			if (item.Points is < 1 and > 0)
				AddLabel(65 + _mMax, offset + height / 2 - 10, 0x64, "1 per " + ((int)Math.Pow(item.Points, -1)).ToString());
			else
				AddLabel(65 + _mMax, offset + height / 2 - 10, 0x64, item.Points.ToString(CultureInfo.InvariantCulture));

			AddTooltip(item.Tooltip);

			if (amount > 0)
				AddLabel(235, offset + height / 2 - 5, 0xB1, amount.ToString("N0", CultureInfo.GetCultureInfo("en-US")));

			offset += 5 + height;
			_mIndex += 1;

			next = _mIndex < _mCollection.Donations.Count ? Math.Max(_mCollection.Donations[_mIndex].Height, 20) : 0;
		}

		// buttons
		AddButton(50, 335, 0x15E3, 0x15E7, (int)Buttons.Rewards, GumpButtonType.Reply, 0);
		AddHtmlLocalized(75, 335, 100, 20, 1072842, 0x1, false, false); // Rewards

		if (_mPage > 1)
		{
			AddButton(150, 335, 0x15E3, 0x15E7, (int)Buttons.Next, GumpButtonType.Page, _mPage - 1);
			AddHtmlLocalized(170, 335, 60, 20, 1074880, 0x1, false, false); // Previous			
		}

		_mPage += 1;

		if (_mIndex >= _mCollection.Donations.Count) return;
		AddButton(300, 335, 0x15E1, 0x15E5, (int)Buttons.Next, GumpButtonType.Page, _mPage);
		AddHtmlLocalized(240, 335, 60, 20, 1072854, 0x1, false, false); // <div align=right>Next</div>
	}

	private void DisplayRewardPage()
	{
		int points = _mOwner.GetCollectionPoints(_mCollection.CollectionId);

		AddPage(_mPage);

		// title
		AddHtmlLocalized(50, 65, 150, 20, 1072843, 0x1, false, false); // Your Reward Points:
		AddLabel(230, 65, 0x64, points.ToString());
		AddImageTiled(35, 85, 270, 2, 0x23C5);
		AddHtmlLocalized(35, 90, 270, 20, 1072844, 0x1, false, false); // Please Choose a Reward:

		// rewards
		int offset = 110;
		int next = 0;

		while (offset + next < 300 && _mIndex < _mCollection.Rewards.Count)
		{
			CollectionItem item = _mCollection.Rewards[_mIndex];

			if (item.QuestItem && SkipQuestReward(_mOwner, item))
			{
				_mIndex++;
				continue;
			}

			int height = Math.Max(item.Height, 20);

			if (points >= item.Points && item.CanSelect(_mOwner))
			{
				AddButton(35, offset + height / 2 - 5, 0x837, 0x838, 200 + _mIndex, GumpButtonType.Reply, 0);
				AddTooltip(item.Tooltip);
			}

			int y = offset - item.Y;

			if (item.Height < 20)
				y += (20 - item.Height) / 2;

			AddItem(55 - item.X + _mMax / 2 - item.Width / 2, y, item.ItemId, points >= item.Points ? item.Hue : 0x3E9);
			AddTooltip(item.Tooltip);
			AddLabel(65 + _mMax, offset + height / 2 - 10, points >= item.Points ? 0x64 : 0x21, item.Points.ToString(CultureInfo.InvariantCulture));
			AddTooltip(item.Tooltip);

			offset += 5 + height;
			_mIndex += 1;

			next = _mIndex < _mCollection.Donations.Count ? Math.Max(_mCollection.Donations[_mIndex].Height, 20) : 0;
		}

		// buttons
		AddButton(50, 335, 0x15E3, 0x15E7, (int)Buttons.Status, GumpButtonType.Reply, 0);
		AddHtmlLocalized(75, 335, 100, 20, 1072845, 0x1, false, false); // Status

		if (_mPage > 1)
		{
			AddButton(150, 335, 0x15E3, 0x15E7, (int)Buttons.Next, GumpButtonType.Page, _mPage - 1);
			AddHtmlLocalized(170, 335, 60, 20, 1074880, 0x1, false, false); // Previous			
		}

		_mPage += 1;

		if (_mIndex >= _mCollection.Rewards.Count) return;
		AddButton(300, 335, 0x15E1, 0x15E5, (int)Buttons.Next, GumpButtonType.Page, _mPage);
		AddHtmlLocalized(240, 335, 60, 20, 1072854, 0x1, false, false); // <div align=right>Next</div>
	}

	private void DisplayHuePage()
	{
		int points = _mOwner.GetCollectionPoints(_mCollection.CollectionId);

		AddPage(_mPage);

		// title
		AddHtmlLocalized(50, 65, 150, 20, 1072843, 0x1, false, false); // Your Reward Points:
		AddLabel(230, 65, 0x64, points.ToString());

		AddImageTiled(35, 85, 270, 2, 0x23C5);

		AddHtmlLocalized(35, 90, 270, 20, 1074255, 0x1, false, false); // Please select a hue for your Reward:

		// hues
		int height = Math.Max(_mItem.Height, 20);
		int offset = 110;

		while (offset + height < 290 && _mIndex < _mItem.Hues.Length)
		{
			AddButton(35, offset + height / 2 - 5, 0x837, 0x838, 100 + _mIndex, GumpButtonType.Reply, 0);
			AddTooltip(_mItem.Tooltip);

			AddItem(55 - _mItem.X, offset - _mItem.Y, _mItem.ItemId, _mItem.Hues[_mIndex]);
			AddTooltip(_mItem.Tooltip);

			offset += 5 + height;
			_mIndex += 1;
		}

		_mPage += 1;

		// buttons			
		AddButton(50, 335, 0x15E3, 0x15E7, (int)Buttons.Rewards, GumpButtonType.Reply, 0);
		AddHtmlLocalized(75, 335, 100, 20, 1072842, 0x1, false, false); // Rewards

		if (_mIndex >= _mItem.Hues.Length || _mPage <= 2) return;
		AddButton(270, 335, 0x15E1, 0x15E5, (int) Buttons.Next, GumpButtonType.Page,
			_mIndex < _mItem.Hues.Length ? _mPage : 1);

		AddHtmlLocalized(210, 335, 60, 20, 1074256, 0x1, false, false); // More Hues
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (_mCollection == null || !_mOwner.InRange(_mLocation, 2))
			return;

		switch (info.ButtonID)
		{
			case (int)Buttons.Rewards:
				_mOwner.SendGump(new CommunityCollectionGump(_mOwner, _mCollection, _mLocation, Section.Rewards));
				break;
			case (int)Buttons.Status:
				_mOwner.SendGump(new CommunityCollectionGump(_mOwner, _mCollection, _mLocation));
				break;
			case >= 300 when _mCollection.Donations != null && info.ButtonID - 300 < _mCollection.Donations.Count && _mSection == Section.Donates:
			{
				CollectionItem item = _mCollection.Donations[info.ButtonID - 300];

				_mOwner.SendLocalizedMessage(1073178); // Please enter how much of that item you wish to donate:
				_mOwner.Prompt = new InternalPrompt(_mCollection, item, _mLocation);
				break;
			}
			case >= 200 when _mCollection.Rewards != null && info.ButtonID - 200 < _mCollection.Rewards.Count && _mSection == Section.Rewards:
			{
				CollectionItem item = _mCollection.Rewards[info.ButtonID - 200];
				int points = _mOwner.GetCollectionPoints(_mCollection.CollectionId);

				if (item.CanSelect(_mOwner))
				{
					if (item.Points <= points)
					{
						if (item is CollectionHuedItem item1)
						{
							_mOwner.SendGump(new CommunityCollectionGump(_mOwner, _mCollection, _mLocation, Section.Hues, item1));
						}
						else
						{
							_mOwner.CloseGump(typeof(ConfirmRewardGump));
							_mOwner.SendGump(new ConfirmRewardGump(_mCollection, _mLocation, item));
						}
					}
					else
						_mOwner.SendLocalizedMessage(1073122); // You don't have enough points for that!
				}

				break;
			}
			case >= 100 when _mItem != null && info.ButtonID - 200 < _mItem.Hues.Length && _mSection == Section.Hues:
				_mOwner.CloseGump(typeof(ConfirmRewardGump));
				_mOwner.SendGump(new ConfirmRewardGump(_mCollection, _mLocation, _mItem, _mItem.Hues[info.ButtonID - 100]));
				break;
		}
	}

	private class InternalPrompt : Prompt
	{
		private readonly IComunityCollection _collection;
		private readonly CollectionItem _mSelected;
		private readonly Point3D _location;
		public InternalPrompt(IComunityCollection collection, CollectionItem selected, Point3D location)
		{
			_collection = collection;
			_mSelected = selected;
			_location = location;
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (!from.InRange(_location, 2) || from is not PlayerMobile mobile)
				return;

			HandleResponse(mobile, text);
			mobile.SendGump(new CommunityCollectionGump(mobile, _collection, _location));
		}

		private void HandleResponse(Mobile from, string text)
		{
			int amount = Utility.ToInt32(text);

			if (amount <= 0)
			{
				from.SendLocalizedMessage(1073181); // That is not a valid donation quantity.
				return;
			}

			if (from.Backpack == null)
				return;

			if (_mSelected.Type == typeof(Gold))
			{
				if (amount * _mSelected.Points < 1)
				{
					from.SendLocalizedMessage(1073167); // You do not have enough of that item to make a donation!
					from.SendGump(new CommunityCollectionGump((PlayerMobile)from, _collection, _location));
					return;
				}

				Item[] items = from.Backpack.FindItemsByType(_mSelected.Type, true);

				int accountcount = from.Account is not Account acct ? 0 : acct.TotalGold;
				int amountRemaining = amount;
				int goldcount = items.Sum(item => item.Amount);

				if (goldcount >= amountRemaining)
				{
					foreach (Item item in items)
					{
						if (item.Amount <= amountRemaining)
						{
							item.Delete();
							amountRemaining -= item.Amount;
						}
						else
						{
							item.Amount -= amountRemaining;
							amountRemaining = 0;
						}

						if (amountRemaining == 0)
							break;
					}
				}
				else if (goldcount + accountcount >= amountRemaining)
				{
					foreach (Item item in items)
					{
						amountRemaining -= item.Amount;
						item.Delete();
					}

					Banker.Withdraw(from, amountRemaining);
				}
				else
				{
					from.SendLocalizedMessage(1073182); // You do not have enough to make a donation of that magnitude!
					from.SendGump(new CommunityCollectionGump((PlayerMobile)from, _collection, _location));
					return;
				}

				from.Backpack.ConsumeTotal(_mSelected.Type, amount, true, true);
				_collection.Donate((PlayerMobile)from, _mSelected, amount);
			}
			/* Remove bank check from collection?
            else if(m_Selected.Type == typeof(BankCheck))
            {
                int count = from.Backpack.GetChecksWorth(true);

                if(count < amount)
                {
                    from.SendLocalizedMessage(1073182); // You do not have enough to make a donation of that magnitude!
                    return;
                }

                from.Backpack.TakeFromChecks(amount, true);
                _Collection.Donate((PlayerMobile)from, m_Selected, amount);
            }
            */
			else
			{
				if (amount * _mSelected.Points < 1)
				{
					from.SendLocalizedMessage(1073167); // You do not have enough of that item to make a donation!
					from.SendGump(new CommunityCollectionGump((PlayerMobile)from, _collection, _location));
					return;
				}

				var items = FindTypes((PlayerMobile)from, _mSelected);

				if (items.Count > 0)
				{
					// count items
					int count = 0;

					for (var i = 0; i < items.Count; i++)
					{
						var item = GetActual(items[i]);

						if (item is {Deleted: false})
							count += item.Amount;
					}

					// check
					if (amount > count)
					{
						from.SendLocalizedMessage(1073182); // You do not have enough to make a donation of that magnitude!
						from.SendGump(new CommunityCollectionGump((PlayerMobile)from, _collection, _location));
						return;
					}

					if (amount * _mSelected.Points < 1)
					{
						from.SendLocalizedMessage(_mSelected.Type == typeof(Gold) ? 1073182 : 1073167); // You do not have enough of that item to make a donation!
						from.SendGump(new CommunityCollectionGump((PlayerMobile)from, _collection, _location));
						return;
					}

					// donate
					int deleted = 0;

					for (int i = 0; i < items.Count && deleted < amount; i++)
					{
						var item = GetActual(items[i]);

						if (item == null || item.Deleted)
						{
							continue;
						}

						if (item.Stackable && item.Amount + deleted > amount && !item.Deleted)
						{
							item.Amount -= amount - deleted;
							deleted += amount - deleted;
						}
						else if (!item.Deleted)
						{
							deleted += item.Amount;
							items[i].Delete();
						}

						if (items[i] is CommodityDeed && !items[i].Deleted)
						{
							items[i].InvalidateProperties();
						}
					}

					_collection.Donate((PlayerMobile)from, _mSelected, amount);
				}
				else
				{
					from.SendLocalizedMessage(1073182); // You do not have enough to make a donation of that magnitude!
				}

				ColUtility.Free(items);
			}
		}

		public override void OnCancel(Mobile from)
		{
			if (from is not PlayerMobile mobile)
				return;

			mobile.SendLocalizedMessage(1073184); // You cancel your donation.

			if (mobile.InRange(_location, 2))
				mobile.SendGump(new CommunityCollectionGump(mobile, _collection, _location));
		}
	}

	private static bool SkipQuestReward(PlayerMobile pm, CollectionItem item)
	{
		if (pm.Quests == null) return true;
		foreach (var obj in pm.Quests.Where(q => !q.Completed).SelectMany(q => q.Objectives))
		{
			if (obj is CollectionsObtainObjective objective && item.Type == objective.Obtain)
				return false;
		}

		return true;
	}

	private static bool CheckType(Item item, Type type, bool checkDerives)
	{
		if (item is CommodityDeed {Commodity: { }} deed)
		{
			item = deed.Commodity;
		}

		var t = item.GetType();

		if (type == t)
		{
			return true;
		}

		if (!checkDerives)
		{
			return false;
		}

		// if (type == typeof(Lobster) && BaseHighseasFish.Lobsters.Any(x => x == t))
		//{
		//    return true;
		//}
		//else if (type == typeof(Crab) && BaseHighseasFish.Crabs.Any(x => x == t))
		//{
		//    return true;
		//}
		//else if (type == typeof(Fish) && t != typeof(BaseCrabAndLobster) && !t.IsSubclassOf(typeof(BaseCrabAndLobster)) && (t.IsSubclassOf(type) || t == typeof(BaseHighseasFish) || t.IsSubclassOf(typeof(BaseHighseasFish))))
		//{
		//    return true;
		//}
		//else
		//{
		return t.IsSubclassOf(type);
		//}
	}

	private static int GetTypes(PlayerMobile pm, CollectionItem colItem)
	{
		var type = colItem.Type;
		bool derives = type == typeof(BaseScales) || type == typeof(Fish) /*|| type == typeof(Crab) || type == typeof(Lobster)*/;

		int count = 0;

		foreach (var item in pm.Backpack.Items.Where(item => CheckType(item, type, derives) && colItem.Validate(pm, GetActual(item))))
		{
			/*
                if (type == typeof(BankCheck))
                {
                    count += pm.Backpack.GetChecksWorth(true);
                }
                */
			if (item is CommodityDeed deed)
			{
				count += deed.Commodity.Amount;
			}
			else
			{
				count += item.Amount;
			}
		}

		return count;
	}

	private static List<Item> FindTypes(PlayerMobile pm, CollectionItem colItem)
	{
		var type = colItem.Type;
		bool derives = type == typeof(BaseScales) || type == typeof(Fish) /*|| type == typeof(Crab) || type == typeof(Lobster)*/;

		return pm.Backpack.Items.Where(item => CheckType(item, type, derives) && colItem.Validate(pm, GetActual(item))).ToList();
	}

	private static Item GetActual(Item item)
	{
		if (item is CommodityDeed deed)
		{
			return deed.Commodity;
		}

		return item;
	}
}
