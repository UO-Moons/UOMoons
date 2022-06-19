using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class GenericBuyInfo : IBuyItemInfo
{
	private class DisplayCache : Container
	{
		private static DisplayCache _mCache;

		public static DisplayCache Cache
		{
			get
			{
				if (_mCache == null || _mCache.Deleted)
					_mCache = new DisplayCache();

				return _mCache;
			}
		}

		private Dictionary<Type, IEntity> _mTable;
		private List<Mobile> _mMobiles;

		public DisplayCache() : base(0)
		{
			_mTable = new Dictionary<Type, IEntity>();
			_mMobiles = new List<Mobile>();
		}

		public IEntity Lookup(Type key)
		{
			_mTable.TryGetValue(key, out IEntity e);
			return e;
		}

		public void Store(Type key, IEntity obj, bool cache)
		{
			if (cache)
				_mTable[key] = obj;

			if (obj is Item)
				AddItem((Item)obj);
			else if (obj is Mobile)
				_mMobiles.Add((Mobile)obj);
		}

		public DisplayCache(Serial serial) : base(serial)
		{
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			for (var i = 0; i < _mMobiles.Count; ++i)
				_mMobiles[i].Delete();

			_mMobiles.Clear();

			for (var i = Items.Count - 1; i >= 0; --i)
				if (i < Items.Count)
					Items[i].Delete();

			if (_mCache == this)
				_mCache = null;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(_mMobiles);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			_mMobiles = reader.ReadStrongMobileList();

			for (var i = 0; i < _mMobiles.Count; ++i)
				_mMobiles[i].Delete();

			_mMobiles.Clear();

			for (var i = Items.Count - 1; i >= 0; --i)
				if (i < Items.Count)
					Items[i].Delete();

			if (_mCache == null)
				_mCache = this;
			else
				Delete();

			_mTable = new Dictionary<Type, IEntity>();
		}
	}

	private int _mPrice;
	private int _mAmount;
	private IEntity _mDisplayEntity;

	public virtual int ControlSlots => 0;

	public virtual bool CanCacheDisplay => false;  //return ( m_Args == null || m_Args.Length == 0 ); }

	private bool IsDeleted(IEntity obj)
	{
		return obj.Deleted;
	}

	public void DeleteDisplayEntity()
	{
		if (_mDisplayEntity == null)
			return;

		_mDisplayEntity.Delete();
		_mDisplayEntity = null;
	}

	public IEntity GetDisplayEntity()
	{
		if (_mDisplayEntity != null && !IsDeleted(_mDisplayEntity))
			return _mDisplayEntity;

		bool canCache = CanCacheDisplay;

		if (canCache)
			_mDisplayEntity = DisplayCache.Cache.Lookup(Type);

		if (_mDisplayEntity == null || IsDeleted(_mDisplayEntity))
			_mDisplayEntity = GetEntity();

		DisplayCache.Cache.Store(Type, _mDisplayEntity, canCache);

		return _mDisplayEntity;
	}

	public Type Type { get; set; }
	public string Name { get; set; }
	public int DefaultPrice { get; private set; }

	public int PriceScalar
	{
		get => DefaultPrice;
		set => DefaultPrice = value;
	}

	public int Price
	{
		get
		{
			if (DefaultPrice == 0) return _mPrice;
			if (_mPrice <= 5000000) return (((_mPrice * DefaultPrice) + 50) / 100);
			long price = _mPrice;

			price *= DefaultPrice;
			price += 50;
			price /= 100;

			if (price > int.MaxValue)
				price = int.MaxValue;

			return (int)price;

		}
		set => _mPrice = value;
	}

	public int ItemId { get; set; }

	public int Hue { get; set; }

	public int Amount
	{
		get => _mAmount;
		set { if (value < 0) value = 0; _mAmount = value; }
	}

	public int MaxAmount { get; set; }

	public object[] Args { get; set; }

	public GenericBuyInfo(Type type, int price, int amount, int itemId, int hue) : this(null, type, price, amount, itemId, hue, null)
	{
	}

	public GenericBuyInfo(string name, Type type, int price, int amount, int itemId, int hue) : this(name, type, price, amount, itemId, hue, null)
	{
	}

	public GenericBuyInfo(Type type, int price, int amount, int itemId, int hue, object[] args) : this(null, type, price, amount, itemId, hue, args)
	{
	}

	public GenericBuyInfo(string name, Type type, int price, int amount, int itemId, int hue, object[] args)
	{
		Type = type;
		_mPrice = price;
		MaxAmount = _mAmount = amount;
		ItemId = itemId;
		Hue = hue;
		Args = args;

		if (name == null)
			Name = itemId < 0x4000 ? (1020000 + itemId).ToString() : (1078872 + itemId).ToString();
		else
			Name = name;
	}

	//get a new instance of an object (we just bought it)
	public virtual IEntity GetEntity()
	{
		if (Args == null || Args.Length == 0)
			return (IEntity)Activator.CreateInstance(Type);

		return (IEntity)Activator.CreateInstance(Type, Args);
		//return (Item)Activator.CreateInstance( m_Type );
	}

	//Attempt to restock with item, (return true if restock sucessfull)
	public bool Restock(Item item, int amount)
	{
		return false;
		/*if ( item.GetType() == m_Type )
		{
			if ( item is BaseWeapon )
			{
				BaseWeapon weapon = (BaseWeapon)item;

				if ( weapon.Quality == EquipmentQuality.Low || weapon.Quality == EquipmentQuality.Exceptional || (int)weapon.DurabilityLevel > 0 || (int)weapon.DamageLevel > 0 || (int)weapon.AccuracyLevel > 0 )
					return false;
			}

			if ( item is BaseArmor )
			{
				BaseArmor armor = (BaseArmor)item;

				if ( armor.Quality == EquipmentQuality.Low || armor.Quality == EquipmentQuality.Exceptional || (int)armor.Durability > 0 || (int)armor.ProtectionLevel > 0 )
					return false;
			}

			m_Amount += amount;

			return true;
		}
		else
		{
			return false;
		}*/
	}

	public void OnRestock()
	{
		if (_mAmount <= 0)
		{
			/*
				Core.ML using this vendor system is undefined behavior, so being
				as it lends itself to an abusable exploit to cause ingame havok
				and the stackable items are not found to be over 20 items, this is
				changed until there is a better solution.
			*/

			object objDisp = GetDisplayEntity();

			if (Core.ML && objDisp is Item && !(objDisp as Item).Stackable)
			{
				MaxAmount = Math.Min(20, MaxAmount);
			}
			else
			{
				MaxAmount = Math.Min(999, MaxAmount * 2);
			}
		}
		else
		{
			/* NOTE: According to UO.com, the quantity is halved if the item does not reach 0
			 * Here we implement differently: the quantity is halved only if less than half
			 * of the maximum quantity was bought. That is, if more than half is sold, then
			 * there's clearly a demand and we should not cut down on the stock.
			 */

			int halfQuantity = MaxAmount;

			switch (halfQuantity)
			{
				case >= 999:
					halfQuantity = 640;
					break;
				case > 20:
					halfQuantity /= 2;
					break;
			}

			if (_mAmount >= halfQuantity)
				MaxAmount = halfQuantity;
		}

		_mAmount = MaxAmount;
	}
}
