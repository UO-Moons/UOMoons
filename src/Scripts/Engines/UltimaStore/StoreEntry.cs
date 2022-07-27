using Server.Items;
using System;

namespace Server.Engines.UOStore;

public class StoreEntry
{
	public Type ItemType { get; }
	public TextDefinition[] Name { get; }
	public int Tooltip { get; }
	public int GumpId { get; }
	public int ItemId { get; }
	public int Hue { get; }
	public int Price { get; }
	public StoreCategory Category { get; }
	private Func<Mobile, StoreEntry, Item> Constructor { get; }

	public int Cost => (int)Math.Ceiling(Price * Configuration.CostMultiplier);

	public StoreEntry(Type itemType, TextDefinition name, int tooltip, int itemId, int gumpId, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
		: this(itemType, new[] { name }, tooltip, itemId, gumpId, hue, cost, cat, constructor)
	{ }

	public StoreEntry(Type itemType, TextDefinition[] name, int tooltip, int itemId, int gumpId, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
	{
		ItemType = itemType;
		Name = name;
		Tooltip = tooltip;
		ItemId = itemId;
		GumpId = gumpId;
		Hue = hue;
		Price = cost;
		Category = cat;
		Constructor = constructor;
	}

	public bool Construct(Mobile m, bool test = false)
	{
		Item item;

		if (Constructor != null)
		{
			item = Constructor(m, this);
		}
		else
		{
			item = Activator.CreateInstance(ItemType) as Item;
		}

		if (item != null)
		{
			if (item is IAccountRestricted restricted)
			{
				restricted.Account = m.Account.Username;
			}

			if (m.Backpack == null || !m.Alive || !m.Backpack.TryDropItem(m, item, false))
			{
				UltimaStore.AddPendingItem(m, item);

				// Your purchased will be delivered to you once you free up room in your backpack.
				// Your purchased item will be delivered to you once you are resurrected.
				m.SendLocalizedMessage(m.Alive ? 1156846 : 1156848);
			}
			else if (item is IPromotionalToken {ItemName: { }} token)
			{
				// A token has been placed in your backpack. Double-click it to redeem your ~1_PROMO~.
				m.SendLocalizedMessage(1075248, token.ItemName.ToString());
			}
			else if (item.LabelNumber > 0 || item.Name != null)
			{
				string name = item.LabelNumber > 0 ? "#" + item.LabelNumber : item.Name;

				// Your purchase of ~1_ITEM~ has been placed in your backpack.
				m.SendLocalizedMessage(1156844, name);
			}
			else
			{
				// Your purchased item has been placed in your backpack.
				m.SendLocalizedMessage(1156843);
			}

			if (test)
			{
				item.Delete();
			}

			return true;
		}

		Utility.WriteConsole(ConsoleColor.Red, $"[Ultima Store Warning]: {ItemType.Name} failed to construct.");

		return false;
	}
}