using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server;

public class RegionRect : IComparable<RegionRect>
{
	public Region Region { get; }
	public Rectangle3D Rect { get; }

	public RegionRect(Region region, Rectangle3D rect)
	{
		Region = region;
		Rect = rect;
	}

	public bool Contains(Point3D loc)
	{
		return Rect.Contains(loc);
	}

	public int CompareTo(RegionRect other)
	{
		if (other == null)
			return 1;

		return Region.CompareTo(other.Region);
	}
}

public class Sector
{
	private List<Mobile> _mobiles;
	private List<Mobile> _players;
	private List<Item> _items;
	private List<NetState> _clients;
	private List<BaseMulti> _multis;
	private List<RegionRect> _regionRects;
	private bool _active;

	// TODO: Can we avoid this?
	private static readonly List<Mobile> m_DefaultMobileList = new();
	private static readonly List<Item> m_DefaultItemList = new();
	private static readonly List<NetState> m_DefaultClientList = new();
	private static readonly List<BaseMulti> m_DefaultMultiList = new();
	private static readonly List<RegionRect> m_DefaultRectList = new();

	public Sector(int x, int y, Map owner)
	{
		X = x;
		Y = y;
		Owner = owner;
		_active = false;
	}

	private static void Add<T>(ref List<T> list, T value)
	{
		list ??= new List<T>();

		list.Add(value);
	}

	private static void Remove<T>(ref List<T> list, T value)
	{
		if (list != null)
		{
			list.Remove(value);

			if (list.Count == 0)
			{
				list = null;
			}
		}
	}

	private static void Replace<T>(ref List<T> list, T oldValue, T newValue)
	{
		if (oldValue != null && newValue != null)
		{
			int index = list?.IndexOf(oldValue) ?? -1;

			if (index >= 0)
			{
				if (list != null) list[index] = newValue;
			}
			else
			{
				Add(ref list, newValue);
			}
		}
		else if (oldValue != null)
		{
			Remove(ref list, oldValue);
		}
		else if (newValue != null)
		{
			Add(ref list, newValue);
		}
	}

	public void OnClientChange(NetState oldState, NetState newState)
	{
		Replace(ref _clients, oldState, newState);
	}

	public void OnEnter(Item item)
	{
		Add(ref _items, item);
	}

	public void OnLeave(Item item)
	{
		Remove(ref _items, item);
	}

	public void OnEnter(Mobile mob)
	{
		Add(ref _mobiles, mob);

		if (mob.NetState != null)
		{
			Add(ref _clients, mob.NetState);
		}

		if (mob.Player)
		{
			if (_players == null)
			{
				Owner.ActivateSectors(X, Y);
			}

			Add(ref _players, mob);
		}
	}

	public void OnLeave(Mobile mob)
	{
		Remove(ref _mobiles, mob);

		if (mob.NetState != null)
		{
			Remove(ref _clients, mob.NetState);
		}

		if (mob.Player && _players != null)
		{
			Remove(ref _players, mob);

			if (_players == null)
			{
				Owner.DeactivateSectors(X, Y);
			}
		}
	}

	public void OnEnter(Region region, Rectangle3D rect)
	{
		Add(ref _regionRects, new RegionRect(region, rect));

		_regionRects.Sort();

		UpdateMobileRegions();
	}

	public void OnLeave(Region region)
	{
		if (_regionRects != null)
		{
			for (int i = _regionRects.Count - 1; i >= 0; i--)
			{
				RegionRect regRect = _regionRects[i];

				if (regRect.Region == region)
				{
					_regionRects.RemoveAt(i);
				}
			}

			if (_regionRects.Count == 0)
			{
				_regionRects = null;
			}
		}

		UpdateMobileRegions();
	}

	private void UpdateMobileRegions()
	{
		if (_mobiles != null)
		{
			List<Mobile> sandbox = new(_mobiles);

			foreach (Mobile mob in sandbox)
			{
				mob.UpdateRegion();
			}
		}
	}

	public void OnMultiEnter(BaseMulti multi)
	{
		Add(ref _multis, multi);
	}

	public void OnMultiLeave(BaseMulti multi)
	{
		Remove(ref _multis, multi);
	}

	public void Activate()
	{
		if (!Active && Owner != Map.Internal)
		{
			if (_items != null)
			{
				foreach (Item item in _items)
				{
					item.OnSectorActivate();
				}
			}

			if (_mobiles != null)
			{
				foreach (Mobile mob in _mobiles)
				{
					mob.OnSectorActivate();
				}
			}

			_active = true;
		}
	}

	public void Deactivate()
	{
		if (Active)
		{
			if (_items != null)
			{
				foreach (Item item in _items)
				{
					item.OnSectorDeactivate();
				}
			}

			if (_mobiles != null)
			{
				foreach (Mobile mob in _mobiles)
				{
					mob.OnSectorDeactivate();
				}
			}

			_active = false;
		}
	}

	public List<RegionRect> RegionRects => _regionRects ?? m_DefaultRectList;

	public List<BaseMulti> Multis => _multis ?? m_DefaultMultiList;

	public List<Mobile> Mobiles => _mobiles ?? m_DefaultMobileList;

	public List<Item> Items => _items ?? m_DefaultItemList;

	public List<NetState> Clients => _clients ?? m_DefaultClientList;

	public List<Mobile> Players => _players ?? m_DefaultMobileList;

	public bool Active => _active && Owner != Map.Internal;

	public Map Owner { get; }

	public int X { get; }

	public int Y { get; }
}
