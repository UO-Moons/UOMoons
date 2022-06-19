using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class GenericSellInfo : IShopSellInfo
{
	private readonly Dictionary<Type, int> _mTable = new();
	private Type[] _mTypes;

	public GenericSellInfo()
	{
	}

	public void Add(Type type, int price)
	{
		_mTable[type] = price;
		_mTypes = null;
	}

	public int GetSellPriceFor(Item item)
	{
		_mTable.TryGetValue(item.GetType(), out int price);

		if (item is BaseArmor)
		{
			BaseArmor armor = (BaseArmor)item;

			price = armor.Quality switch
			{
				ItemQuality.Low => (int) (price * 0.60),
				ItemQuality.Exceptional => (int) (price * 1.25),
				_ => price
			};

			price += 100 * (int)armor.Durability;

			price += 100 * (int)armor.ProtectionLevel;

			if (price < 1)
				price = 1;
		}
		else if (item is BaseWeapon)
		{
			BaseWeapon weapon = (BaseWeapon)item;

			price = weapon.Quality switch
			{
				ItemQuality.Low => (int) (price * 0.60),
				ItemQuality.Exceptional => (int) (price * 1.25),
				_ => price
			};

			price += 100 * (int)weapon.DurabilityLevel;

			price += 100 * (int)weapon.DamageLevel;

			if (price < 1)
				price = 1;
		}
		else if (item is BaseBeverage)
		{
			int price1 = price, price2 = price;

			switch (item)
			{
				case Pitcher:
					price1 = 3; price2 = 5;
					break;
				case BeverageBottle:
					price1 = 3; price2 = 3;
					break;
				case Jug:
					price1 = 6; price2 = 6;
					break;
			}

			BaseBeverage bev = (BaseBeverage)item;

			if (bev.IsEmpty || bev.Content == BeverageType.Milk)
				price = price1;
			else
				price = price2;
		}

		return price;
	}

	public int GetBuyPriceFor(Item item)
	{
		return (int)(1.90 * GetSellPriceFor(item));
	}

	public Type[] Types
	{
		get
		{
			if (_mTypes != null) return _mTypes;
			_mTypes = new Type[_mTable.Keys.Count];
			_mTable.Keys.CopyTo(_mTypes, 0);

			return _mTypes;
		}
	}

	public string GetNameFor(Item item)
	{
		return item.Name ?? item.LabelNumber.ToString();
	}

	public bool IsSellable(Item item)
	{
		return !item.Nontransferable && IsInList(item.GetType());

		//if ( item.Hue != 0 )
		//return false;
	}

	public bool IsResellable(Item item)
	{
		return !item.Nontransferable && IsInList(item.GetType());

		//if ( item.Hue != 0 )
		//return false;
	}

	public bool IsInList(Type type)
	{
		return _mTable.ContainsKey(type);
	}
}
