using Server.ContextMenus;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Prompts;
using System;
using System.Collections.Generic;

namespace Server.Engines.NewMagincia;

public class WarehouseSuperintendent : BaseCreature
{
	public override bool IsInvulnerable => true;

	[Constructable]
	public WarehouseSuperintendent() : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
	{
		Race = Race.Human;
		Blessed = true;
		Title = "The Warehouse Superintendent";

		if (Utility.RandomBool())
		{
			Female = true;
			Body = 0x191;
			Name = NameList.RandomName("female");

			AddItem(new Skirt(Utility.RandomPinkHue()));
		}
		else
		{
			Female = false;
			Body = 0x190;
			Name = NameList.RandomName("male");
			AddItem(new ShortPants(Utility.RandomBlueHue()));
		}

		AddItem(new Tunic(Utility.RandomBlueHue()));
		AddItem(new Boots());

		Utility.AssignRandomHair(this, true);
		Utility.AssignRandomFacialHair(this, true);

		Hue = Race.RandomSkinHue();
	}

	/*public override void OnDoubleClick(Mobile from)
	{
		if(from.InRange(this.Location, 3) && from.Backpack != null)
		{
			WarehouseContainer container = MaginciaBazaar.ClaimContainer(from);
			
			if(container != null)
				TryTransferItems(from, container);
		}
	}*/

	public void TryTransfer(Mobile from, StorageEntry entry)
	{
		if (entry == null)
			return;

		int fees = entry.Funds;

		if (fees < 0)
		{
			int owed = fees * -1;
			SayTo(from,
				$"It looks like you owe {owed:###,###,###}gp as back fees. How much would you like to pay now?");
			from.Prompt = new BackfeePrompt(this, entry);
			return;
		}

		if (!TryPayFunds(from, entry))
		{
			from.SendGump(new BazaarInformationGump(1150681, 1150678)); // Some personal possessions that were equipped on the broker still remain in storage, because your backpack cannot hold them. Please free up space in your backpack and return to claim these items.
			return;
		}

		if (entry.Creatures.Count > 0)
		{
			List<BaseCreature> list = new(entry.Creatures);

			foreach (BaseCreature bc in list)
			{
				if (from.Stabled.Count < AnimalTrainer.GetMaxStabled(from))
				{
					bc.Blessed = false;
					bc.ControlOrder = OrderType.Stay;
					bc.Internalize();
					bc.IsStabled = true;
					bc.Loyalty = MaxLoyalty; // Wonderfully happy
					from.Stabled.Add(bc);
					bc.SetControlMaster(null);
					bc.SummonMaster = null;

					entry.RemovePet(bc);
				}
				else
				{
					from.SendGump(new BazaarInformationGump(1150681, 1150678)); // Some personal possessions that were equipped on the broker still remain in storage, because your backpack cannot hold them. Please free up space in your backpack and return to claim these items.
					return;
				}
			}

			ColUtility.Free(list);
		}

		if (entry.CommodityTypes.Count > 0)
		{
			Dictionary<Type, int> copy = new(entry.CommodityTypes);

			foreach (var (type, amt) in copy)
			{
				if (!GiveItems(from, type, amt, entry))
				{
					from.SendGump(new BazaarInformationGump(1150681, 1150678)); // Some personal possessions that were equipped on the broker still remain in storage, because your backpack cannot hold them. Please free up space in your backpack and return to claim these items.
					return;
				}
			}

			copy.Clear();
		}

		entry.CommodityTypes.Clear();
		ColUtility.Free(entry.Creatures);

		from.SendGump(new BazaarInformationGump(1150681, 1150677)); // There are no longer any items or funds in storage for your former bazaar stall. Thank you for your diligence in recovering your possessions.
		MaginciaBazaar.RemoveFromStorage(from);
	}

	private bool GiveItems(Mobile from, Type type, int amt, StorageEntry entry)
	{
		int amount = amt;

		while (amount > 60000)
		{
			CommodityDeed deed = new();
			Item item = Loot.Construct(type);
			item.Amount = 60000;
			deed.SetCommodity(item);
			amount -= 60000;

			if (from.Backpack == null || !from.Backpack.TryDropItem(from, deed, false))
			{
				deed.Delete();
				return false;
			}
			else
				entry.RemoveCommodity(type, 60000);
		}

		CommodityDeed deed2 = new();
		Item item2 = Loot.Construct(type);
		item2.Amount = amount;
		deed2.SetCommodity(item2);

		if (from.Backpack == null || !from.Backpack.TryDropItem(from, deed2, false))
		{
			deed2.Delete();
			return false;
		}
		else
			entry.RemoveCommodity(type, amount);

		return true;
	}

	private static bool TryPayFunds(Mobile from, StorageEntry entry)
	{
		int amount = entry.Funds;

		if (Banker.Withdraw(from, amount, true))
		{
			entry.Funds = 0;
			return true;
		}

		return false;
	}

	public void TryPayBackfee(Mobile from, string text, StorageEntry entry)
	{
		int amount = Utility.ToInt32(text);
		int owed = entry.Funds * -1;

		if (amount > 0)
		{
			int toDeduct = Math.Min(owed, amount);

			if (Banker.Withdraw(from, toDeduct))
			{
				entry.Funds += toDeduct;
				int newAmount = entry.Funds;

				if (newAmount >= 0)
				{
					TryTransfer(from, entry);
				}
				else
				{
					SayTo(from, $"Thank you! You have a remaining balance of {newAmount * -1}gp as backfees!");
				}
			}
			else
			{
				SayTo(from, "You don't have enough funds in your bankbox to support that amount.");
			}
		}

	}

	private class BackfeePrompt : Prompt
	{
		private readonly WarehouseSuperintendent _mobile;
		private readonly StorageEntry _entry;

		public BackfeePrompt(WarehouseSuperintendent mobile, StorageEntry entry)
		{
			_mobile = mobile;
			_entry = entry;
		}

		public override void OnResponse(Mobile from, string text)
		{
			_mobile.TryPayBackfee(from, text, _entry);
		}

	}

	/*private bool TransferItems(Mobile from, WarehouseContainer c)
	{
		List<Item> items = new List<Item>(c.Items);
		
		foreach(Item item in items)
		{
			from.Backpack.TryDropItem(from, item, false);
		}
		
		return c.Items.Count == 0;
	}*/

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		list.Add(new ClaimStorageEntry(from, this));
		list.Add(new ChangeMatchBidEntry(from));
	}

	private class ClaimStorageEntry : ContextMenuEntry
	{
		private readonly WarehouseSuperintendent _mobile;
		private readonly StorageEntry _entry;

		public ClaimStorageEntry(Mobile from, WarehouseSuperintendent mobile) : base(1150681, 3)
		{
			_mobile = mobile;
			_entry = MaginciaBazaar.GetStorageEntry(from);

			if (_entry == null)
				Flags |= CMEFlags.Disabled;
		}

		public override void OnClick()
		{
			Mobile from = Owner.From;

			if (from == null || _entry == null)
				return;

			_mobile.TryTransfer(from, _entry);
		}
	}

	private class ChangeMatchBidEntry : ContextMenuEntry
	{
		public ChangeMatchBidEntry(Mobile from) : base(1150587, 3)
		{
		}

		public override void OnClick()
		{
			Mobile from = Owner.From;

			from?.SendGump(new MatchBidGump(from, null));
		}
	}

	public WarehouseSuperintendent(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
