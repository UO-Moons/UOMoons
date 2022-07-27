using System;

namespace Server.Factions;

public interface IFactionItem
{
	FactionItem FactionItemState { get; set; }
}

public class FactionItem
{
	private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(21.0);

	public Item Item { get; }
	public Faction Faction { get; }
	public DateTime Expiration { get; private set; }

	private bool HasExpired
	{
		get
		{
			if (Item == null || Item.Deleted)
				return true;

			return Expiration != DateTime.MinValue && DateTime.UtcNow >= Expiration;
		}
	}

	private void StartExpiration()
	{
		Expiration = DateTime.UtcNow + ExpirationPeriod;
	}

	public void CheckAttach()
	{
		if (!HasExpired)
			Attach();
		else
			Detach();
	}

	private void Attach()
	{
		if (Item is IFactionItem item)
			item.FactionItemState = this;

		if (Faction != null)
			Faction.State.FactionItems.Add(this);
	}

	public void Detach()
	{
		if (Item is IFactionItem item)
			item.FactionItemState = null;

		if (Faction != null && Faction.State.FactionItems.Contains(this))
			Faction.State.FactionItems.Remove(this);
	}

	private FactionItem(Item item, Faction faction)
	{
		Item = item;
		Faction = faction;
	}

	public FactionItem(GenericReader reader, Faction faction)
	{
		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 0:
			{
				Item = reader.ReadItem();
				Expiration = reader.ReadDateTime();
				break;
			}
		}

		Faction = faction;
	}

	public void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(0);

		writer.Write(Item);
		writer.Write(Expiration);
	}

	public static int GetMaxWearables(Mobile mob)
	{
		PlayerState pl = PlayerState.Find(mob);

		if (pl == null)
			return 0;

		return pl.Faction.IsCommander(mob) ? 9 : pl.Rank.MaxWearables;
	}

	public static FactionItem Find(Item item)
	{
		if (item is not IFactionItem item1)
			return null;
		FactionItem state = item1.FactionItemState;

		if (state is not { HasExpired: true })
			return state;

		state.Detach();

		return null;

	}

	public static Item Imbue(Item item, Faction faction, bool expire, int hue)
	{
		if (item is not IFactionItem)
			return item;

		FactionItem state = Find(item);

		if (state == null)
		{
			state = new FactionItem(item, faction);
			state.Attach();
		}

		if (expire)
			state.StartExpiration();

		item.Hue = hue;
		return item;
	}
}
