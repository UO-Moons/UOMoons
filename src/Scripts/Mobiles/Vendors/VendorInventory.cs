using Server.Multis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Mobiles;

public class VendorInventory
{
	public static readonly TimeSpan GracePeriod = TimeSpan.FromDays(7.0);
	private readonly Timer _mExpireTimer;

	public VendorInventory(BaseHouse house, Mobile owner, string vendorName, string shopName)
	{
		House = house;
		Owner = owner;
		VendorName = vendorName;
		ShopName = shopName;

		Items = new List<Item>();

		ExpireTime = DateTime.UtcNow + GracePeriod;
		_mExpireTimer = new ExpireTimer(this, GracePeriod);
		_mExpireTimer.Start();
	}

	public BaseHouse House { get; set; }

	public string VendorName { get; set; }

	public string ShopName { get; set; }

	public Mobile Owner { get; set; }

	public List<Item> Items { get; }

	public int Gold { get; set; }

	public DateTime ExpireTime { get; }

	public void AddItem(Item item)
	{
		item.Internalize();
		Items.Add(item);
	}

	public void Delete()
	{
		foreach (Item item in Items)
		{
			item.Delete();
		}

		Items.Clear();
		Gold = 0;

		if (House != null)
			House.VendorInventories.Remove(this);

		_mExpireTimer.Stop();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(0); // version

		writer.Write(Owner);
		writer.Write(VendorName);
		writer.Write(ShopName);

		writer.Write(Items, true);
		writer.Write(Gold);

		writer.WriteDeltaTime(ExpireTime);
	}

	public VendorInventory(BaseHouse house, GenericReader reader)
	{
		House = house;

		reader.ReadEncodedInt();

		Owner = reader.ReadMobile();
		VendorName = reader.ReadString();
		ShopName = reader.ReadString();

		Items = reader.ReadStrongItemList();
		Gold = reader.ReadInt();

		ExpireTime = reader.ReadDeltaTime();

		if (Items.Count == 0 && Gold == 0)
		{
			Timer.DelayCall(TimeSpan.Zero, Delete);
		}
		else
		{
			TimeSpan delay = ExpireTime - DateTime.UtcNow;
			_mExpireTimer = new ExpireTimer(this, delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
			_mExpireTimer.Start();
		}
	}

	private class ExpireTimer : Timer
	{
		private readonly VendorInventory _mInventory;

		public ExpireTimer(VendorInventory inventory, TimeSpan delay) : base(delay)
		{
			_mInventory = inventory;

			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			BaseHouse house = _mInventory.House;

			if (house != null)
			{
				if (_mInventory.Gold > 0)
				{
					if (house.MovingCrate == null)
						house.MovingCrate = new MovingCrate(house);

					Banker.Deposit(house.MovingCrate, _mInventory.Gold);
				}

				foreach (var item in _mInventory.Items.Where(item => !item.Deleted))
				{
					house.DropToMovingCrate(item);
				}

				_mInventory.Gold = 0;
				_mInventory.Items.Clear();
			}

			_mInventory.Delete();
		}
	}
}
