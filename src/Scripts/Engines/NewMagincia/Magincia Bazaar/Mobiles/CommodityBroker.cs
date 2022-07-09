using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.NewMagincia;

public class CommodityBroker : BaseBazaarBroker
{
	public List<CommodityBrokerEntry> CommodityEntries { get; } = new();

	public static readonly int MaxEntries = 50;

	public CommodityBroker(MaginciaBazaarPlot plot) : base(plot)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.InRange(Location, 4) && Plot != null)
		{
			if (Plot.Owner == from)
			{
				from.CloseGump(typeof(CommodityBrokerGump));
				from.SendGump(new CommodityBrokerGump(this, from));
			}
			else
			{
				from.CloseGump(typeof(CommodityInventoryGump));
				from.SendGump(new CommodityInventoryGump(this));
			}
		}
		else
			base.OnDoubleClick(from);
	}

	public override int GetWeeklyFee()
	{
		int total = CommodityEntries.Where(entry => entry.SellPricePer > 0).Sum(entry => entry.Stock * entry.SellPricePer);

		double perc = total * .05;
		return (int)perc;
	}

	public bool TryAddBrokerEntry(Item item, Mobile from)
	{
		Item realItem = item;

		if (item is CommodityDeed deed)
			realItem = deed.Commodity;

		Type type = realItem.GetType();
		int amount = realItem.Amount;

		if (HasType(type))
			return false;

		CommodityBrokerEntry entry = new(realItem, this, amount);
		CommodityEntries.Add(entry);

		if (amount > 0)
			from.SendLocalizedMessage(1150220,
				$"{amount}\t#{entry.Label}\t{Plot.ShopName ?? "an unnamed shop"}"); // You have added ~1_QUANTITY~ units of ~2_ITEMNAME~ to the inventory of "~3_SHOPNAME~"

		item.Delete();

		return true;
	}

	public void RemoveEntry(Mobile from, CommodityBrokerEntry entry)
	{
		if (CommodityEntries.Contains(entry))
		{
			if (entry.Stock > 0)
				WithdrawInventory(from, GetStock(entry), entry);
			CommodityEntries.Remove(entry);
		}
	}

	public override bool HasValidEntry(Mobile m)
	{
		return CommodityEntries.Any(entry => entry.Stock > 0);
	}

	public void AddInventory(Mobile from, Item item)
	{
		Type type = item.GetType();
		int amountToAdd = item.Amount;

		if (item is CommodityDeed deed)
		{
			type = deed.Commodity.GetType();
			amountToAdd = deed.Commodity.Amount;
		}

		foreach (var entry in CommodityEntries.Where(entry => entry.CommodityType == type))
		{
			entry.Stock += amountToAdd;
			item.Delete();

			if (from != null && Plot.Owner == from)
				from.SendLocalizedMessage(1150220,
					$"{amountToAdd}\t#{entry.Label}\t{Plot.ShopName ?? "an unnamed shop"}"); // You have added ~1_QUANTITY~ units of ~2_ITEMNAME~ to the inventory of "~3_SHOPNAME~"
			break;
		}
	}

	private bool HasType(Type type)
	{
		return CommodityEntries.Any(entry => entry.CommodityType == type);
	}

	public static int GetStock(CommodityBrokerEntry entry)
	{
		return entry.Stock;
	}

	public void WithdrawInventory(Mobile from, int amount, CommodityBrokerEntry entry)
	{
		if (from == null || Plot == null || entry == null || !CommodityEntries.Contains(entry))
			return;

		Container pack = from.Backpack;

		if (pack != null)
		{
			while (amount > 60000)
			{
				CommodityDeed deed = new();
				Item item = Loot.Construct(entry.CommodityType);
				item.Amount = 60000;
				deed.SetCommodity(item);
				pack.DropItem(deed);
				amount -= 60000;
				entry.Stock -= 60000;
			}

			CommodityDeed deed2 = new();
			Item newitem = Loot.Construct(entry.CommodityType);
			newitem.Amount = amount;
			deed2.SetCommodity(newitem);
			pack.DropItem(deed2);
			entry.Stock -= amount;
		}

		if (Plot != null && from == Plot.Owner)
			from.SendLocalizedMessage(1150221,
				$"{amount}\t#{entry.Label}\t{Plot.ShopName ?? "a shop with no name"}"); // You have removed ~1_QUANTITY~ units of ~2_ITEMNAME~ from the inventory of "~3_SHOPNAME~"
	}

	public int GetBuyCost(Mobile from, CommodityBrokerEntry entry, int amount)
	{
		int totalCost = entry.SellPricePer * amount;
		int toDeduct = totalCost + (int)(totalCost * (ComissionFee / 100.0));

		return toDeduct;
	}

	// Called when a player BUYS the commodity from teh broker...this is fucking confusing
	public void TryBuyCommodity(Mobile from, CommodityBrokerEntry entry, int amount)
	{
		int totalCost = entry.SellPricePer * amount;
		int toAdd = totalCost + (int)(totalCost * (ComissionFee / 100.0));

		if (Banker.Withdraw(from, totalCost, true))
		{
			from.SendLocalizedMessage(1150643, $"{amount:###,###,###}\t#{entry.Label}"); // A commodity deed worth ~1_AMOUNT~ ~2_ITEM~ has been placed in your backpack.
			WithdrawInventory(from, amount, entry);
			BankBalance += toAdd;
		}
		else
		{
			from.SendLocalizedMessage(1150252); // You do not have the funds needed to make this trade available in your bank box. Brokers are only able to transfer funds from your bank box. Please deposit the necessary funds into your bank box and try again.
		}
	}

	public bool SellCommodityControl(Mobile from, CommodityBrokerEntry entry, int amount)
	{
		//int totalCost = entry.BuyPricePer * amount;
		Type type = entry.CommodityType;

		if (from.Backpack != null)
		{
			int typeAmount = from.Backpack.GetAmount(type);
			int commodityAmount = GetCommodityType(from.Backpack, type);

			if (typeAmount + commodityAmount >= amount)
				return true;
		}

		return false;
	}

	// Called when a player SELLs the commodity from teh broker...this is fucking confusing
	public void TrySellCommodity(Mobile from, CommodityBrokerEntry entry, int amount)
	{
		int totalCost = entry.BuyPricePer * amount;
		Type type = entry.CommodityType;

		if (BankBalance < totalCost)
		{
			//No message, this should have already been handled elsewhere
		}
		else if (from.Backpack != null)
		{
			int total = amount;
			int typeAmount = from.Backpack.GetAmount(type);
			int commodityAmount = GetCommodityType(from.Backpack, type);

			if (typeAmount + commodityAmount < total)
				from.SendLocalizedMessage(1150667); // You do not have the requested amount of that commodity (either in item or deed form) in your backpack to trade. Note that commodities cannot be traded from your bank box.
			else if (Banker.Deposit(from, totalCost))
			{
				TakeItems(from.Backpack, type, ref total);

				if (total > 0)
					TakeCommodities(from.Backpack, type, ref total);

				BankBalance -= totalCost + (int)(totalCost * (ComissionFee / 100.0));
				from.SendLocalizedMessage(1150668, $"{amount}\t#{entry.Label}"); // You have sold ~1_QUANTITY~ units of ~2_COMMODITY~ to the broker. These have been transferred from deeds and/or items in your backpack.
			}
			else
				from.SendLocalizedMessage(1150265); // Your bank box cannot hold the proceeds from this transaction.
		}
	}

	private void TakeCommodities(Container c, Type type, ref int amount)
	{
		if (c == null)
			return;

		Item[] items = c.FindItemsByType(typeof(CommodityDeed));
		List<Item> toSell = new();

		foreach (Item item in items)
		{
			if (item is CommodityDeed {Commodity: { }} commodityDeed && commodityDeed.Commodity.GetType() == type)
			{
				Item commodity = commodityDeed.Commodity;

				if (commodity.Amount <= amount)
				{
					toSell.Add(item);
					amount -= commodity.Amount;
				}
				else
				{
					CommodityDeed newDeed = new();
					Item newItem = Loot.Construct(type);
					newItem.Amount = amount;
					newDeed.SetCommodity(newItem);

					commodity.Amount -= amount;
					commodityDeed.InvalidateProperties();
					toSell.Add(newDeed);
					amount = 0;
				}
			}
		}

		foreach (Item item in toSell)
		{
			AddInventory(null, item);
		}
	}

	private void TakeItems(Container c, Type type, ref int amount)
	{
		if (c == null)
			return;

		Item[] items = c.FindItemsByType(type);
		List<Item> toSell = new();

		foreach (Item item in items)
		{
			if (amount <= 0)
				break;

			if (item.Amount <= amount)
			{
				toSell.Add(item);
				amount -= item.Amount;
			}
			else
			{
				Item newItem = Loot.Construct(type);
				newItem.Amount = amount;
				item.Amount -= amount;
				toSell.Add(newItem);
				amount = 0;
			}
		}

		foreach (Item item in toSell)
		{
			AddInventory(null, item);
		}
	}

	public int GetCommodityType(Container c, Type type)
	{
		if (c == null)
			return 0;

		Item[] items = c.FindItemsByType(typeof(CommodityDeed));
		int amt = 0;

		foreach (Item item in items)
		{
			if (item is CommodityDeed {Commodity: { }} deed && deed.Commodity.GetType() == type)
				amt += deed.Commodity.Amount;
		}

		return amt;
	}

	public int GetLabelId(CommodityBrokerEntry entry)
	{
		/*Item[] items = BuyPack.FindItemsByType(typeof(CommodityDeed));
		
		foreach(Item item in items)
		{
			if(item is CommodityDeed)
			{
				CommodityDeed deed = (CommodityDeed)item;
				
				if(deed.Commodity != null && deed.Commodity.GetType() == entry.CommodityType)
					return deed.Commodity.ItemID;
			}
		}
		
		Item item = Loot.Construct(entry.CommodityType);
		int id = 0;
		
		if(item != null)
		{
			id = item.ItemID;
			item.Delete();
		}*/

		return entry?.Label ?? 1;
	}

	public CommodityBroker(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(CommodityEntries.Count);
		foreach (CommodityBrokerEntry entry in CommodityEntries)
			entry.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		int count = reader.ReadInt();
		for (var i = 0; i < count; i++)
			CommodityEntries.Add(new CommodityBrokerEntry(reader));
	}
}
