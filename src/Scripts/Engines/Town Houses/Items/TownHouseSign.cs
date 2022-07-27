using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.TownHouses;

public enum Intu
{
	Neither,
	No,
	Yes
}

public enum SignIDs
{
	TownHouseSignWE = 0xC0B,
	TownHouseSignNS = 0xC0C,
	HouseSignWE = 0xBD1,
	HouseSignNS = 0xBD2,
	SignHangerWE = 0xB97,
	SignHangerNS = 0xB98,
}

[Flipable(0xC0B, 0xC0C)]
public class TownHouseSign : BaseItem
{
	public static List<TownHouseSign> AllSigns { get; } = new();

	private Point3D _cBanLoc, _cSignLoc;

	private int _cLocks,
		_cSecures,
		_cPrice,
		_cMinZ,
		_cMaxZ,
		_cMinTotalSkill,
		_cMaxTotalSkill,
		_cItemsPrice,
		_cRtoPayments;

	private bool _cYoungOnly,
		_cRecurRent, _cKeepItems,
		_cLeaveItems,
		_cRentToOwn,
		_cFree,
		_cForcePrivate,
		_cForcePublic, _cNoBanning;

	private string _cSkill;
	private double _cSkillReq;
	private List<DecoreItemInfo> _cDecoreItemInfos;
	private List<Item> _cPreviewItems;
	private Timer _cRentTimer, _cPreviewTimer;
	private TimeSpan _cRentByTime, _cOriginalRentTime;
	private Intu _cMurderers;

	public  DateTime CRentTime { get; private set; }

	public Point3D BanLoc
	{
		get => _cBanLoc;
		set
		{
			_cBanLoc = value;
			InvalidateProperties();
			if (Owned)
			{
				House.Region.GoLocation = value;
			}
		}
	}

	public Point3D SignLoc
	{
		get => _cSignLoc;
		set
		{
			_cSignLoc = value;
			InvalidateProperties();

			if (!Owned)
			{
				return;
			}
			House.Sign.Location = value;
			House.Hanger.Location = value;
		}
	}

	public int Locks
	{
		get => _cLocks;
		set
		{
			_cLocks = value;
			InvalidateProperties();
			if (Owned)
			{
				House.MaxLockDowns = value;
			}
		}
	}

	public int Secures
	{
		get => _cSecures;
		set
		{
			_cSecures = value;
			InvalidateProperties();
			if (Owned)
			{
				House.MaxSecures = value;
			}
		}
	}

	public int Price
	{
		get => _cPrice;
		set
		{
			_cPrice = value;
			InvalidateProperties();
		}
	}

	public int MinZ
	{
		get => _cMinZ;
		set
		{
			if (value > _cMaxZ)
			{
				_cMaxZ = value + 1;
			}

			_cMinZ = value;
			if (Owned)
			{
				VersionCommand.UpdateRegion(this);
			}
		}
	}

	public int MaxZ
	{
		get => _cMaxZ;
		set
		{
			if (value < _cMinZ)
			{
				value = _cMinZ;
			}

			_cMaxZ = value;
			if (Owned)
			{
				VersionCommand.UpdateRegion(this);
			}
		}
	}

	public int MinTotalSkill
	{
		get => _cMinTotalSkill;
		set
		{
			if (value > _cMaxTotalSkill)
			{
				value = _cMaxTotalSkill;
			}

			_cMinTotalSkill = value;
			ValidateOwnership();
			InvalidateProperties();
		}
	}

	public int MaxTotalSkill
	{
		get => _cMaxTotalSkill;
		set
		{
			if (value < _cMinTotalSkill)
			{
				value = _cMinTotalSkill;
			}

			_cMaxTotalSkill = value;
			ValidateOwnership();
			InvalidateProperties();
		}
	}

	public bool YoungOnly
	{
		get => _cYoungOnly;
		set
		{
			_cYoungOnly = value;

			if (_cYoungOnly)
			{
				_cMurderers = Intu.Neither;
			}

			ValidateOwnership();
			InvalidateProperties();
		}
	}

	public TimeSpan RentByTime
	{
		get => _cRentByTime;
		set
		{
			_cRentByTime = value;
			_cOriginalRentTime = value;

			if (value == TimeSpan.Zero)
			{
				ClearRentTimer();
			}
			else
			{
				ClearRentTimer();
				BeginRentTimer(value);
			}

			InvalidateProperties();
		}
	}

	public bool RecurRent
	{
		get => _cRecurRent;
		set
		{
			_cRecurRent = value;

			if (!value)
			{
				_cRentToOwn = false;
			}

			InvalidateProperties();
		}
	}

	public bool KeepItems
	{
		get => _cKeepItems;
		set
		{
			_cLeaveItems = false;
			_cKeepItems = value;
			InvalidateProperties();
		}
	}

	public bool Free
	{
		get => _cFree;
		set
		{
			_cFree = value;
			_cPrice = 1;
			InvalidateProperties();
		}
	}

	public Intu Murderers
	{
		get => _cMurderers;
		set
		{
			_cMurderers = value;

			ValidateOwnership();
			InvalidateProperties();
		}
	}

	public bool ForcePrivate
	{
		get => _cForcePrivate;
		set
		{
			_cForcePrivate = value;

			if (value)
			{
				_cForcePublic = false;

				if (House != null)
				{
					House.Public = false;
				}
			}
		}
	}

	public bool ForcePublic
	{
		get => _cForcePublic;
		set
		{
			_cForcePublic = value;

			if (value)
			{
				_cForcePrivate = false;

				if (House != null)
				{
					House.Public = true;
				}
			}
		}
	}

	public bool NoBanning
	{
		get => _cNoBanning;
		set
		{
			_cNoBanning = value;

			if (value && House != null)
			{
				House.Bans.Clear();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Type Currency { get; set; }

	public List<Rectangle2D> Blocks { get; set; }

	public string Skill
	{
		get => _cSkill;
		set
		{
			_cSkill = value;
			ValidateOwnership();
			InvalidateProperties();
		}
	}

	public double SkillReq
	{
		get => _cSkillReq;
		set
		{
			_cSkillReq = value;
			ValidateOwnership();
			InvalidateProperties();
		}
	}

	public bool LeaveItems
	{
		get => _cLeaveItems;
		set
		{
			_cLeaveItems = value;
			InvalidateProperties();
		}
	}

	public bool RentToOwn
	{
		get => _cRentToOwn;
		set
		{
			_cRentToOwn = value;
			InvalidateProperties();
		}
	}

	public bool Relock { get; set; }

	public bool NoHouseTrade { get; set; }

	public int ItemsPrice
	{
		get => _cItemsPrice;
		set
		{
			_cItemsPrice = value;
			InvalidateProperties();
		}
	}

	public TownHouse House { get; set; }
	protected Timer DemolishTimer { get; private set; }
	protected DateTime DemolishTime { get; private set; }
	public bool Owned => House is {Deleted: false};
	public int Floors => (_cMaxZ - _cMinZ) / 20 + 1;
	public bool BlocksReady => Blocks.Count != 0;
	public bool FloorsReady => BlocksReady && MinZ != short.MinValue;
	public bool SignReady => FloorsReady && SignLoc != Point3D.Zero;
	public bool BanReady => SignReady && BanLoc != Point3D.Zero;
	public bool LocSecReady => BanReady && Locks != 0 && Secures != 0;
	public bool ItemsReady => LocSecReady;
	public bool LengthReady => ItemsReady;
	public bool PriceReady => LengthReady && Price != 0;

	public string PriceType
	{
		get
		{
			if (_cRentByTime == TimeSpan.Zero)
			{
				return "Sale";
			}
			if (_cRentByTime == TimeSpan.FromDays(1))
			{
				return "Daily";
			}
			if (_cRentByTime == TimeSpan.FromDays(7))
			{
				return "Weekly";
			}
			return _cRentByTime == TimeSpan.FromDays(30) ? "Monthly" : "Sale";
		}
	}

	public string PriceTypeShort
	{
		get
		{
			if (_cRentByTime == TimeSpan.Zero)
			{
				return "Sale";
			}
			if (_cRentByTime == TimeSpan.FromDays(1))
			{
				return "Day";
			}
			if (_cRentByTime == TimeSpan.FromDays(7))
			{
				return "Week";
			}
			return _cRentByTime == TimeSpan.FromDays(30) ? "Month" : "Sale";
		}
	}

	[Constructable]
	public TownHouseSign() : base(0xC0B)
	{
		Name = "This building is for sale or rent!";
		Movable = false;

		_cBanLoc = Point3D.Zero;
		_cSignLoc = Point3D.Zero;
		_cSkill = "";
		Blocks = new List<Rectangle2D>();
		_cDecoreItemInfos = new List<DecoreItemInfo>();
		_cPreviewItems = new List<Item>();
		DemolishTime = DateTime.UtcNow;
		CRentTime = DateTime.UtcNow;
		_cRentByTime = TimeSpan.Zero;
		_cRecurRent = true;

		_cMinZ = short.MinValue;
		_cMaxZ = short.MaxValue;

		AllSigns.Add(this);
	}
	/*
	private void SearchForHouse()
	{
		foreach (TownHouse house in TownHouse.AllTownHouses.Where(house => house.ForSaleSign == this))
		{
			House = house;
		}
	}*/

	public void UpdateBlocks()
	{
		if (!Owned)
		{
			return;
		}

		if (Blocks.Count == 0)
		{
			UnconvertDoors();
		}

		VersionCommand.UpdateRegion(this);
		ConvertItems(false);
		House.InitSectorDefinition();
	}

	public void ShowAreaPreview(Mobile m)
	{
		ClearPreview();

		ArrayList blocks = new();

		foreach (var rect in Blocks)
		{
			for (var x = rect.Start.X; x < rect.End.X; ++x)
			{
				for (var y = rect.Start.Y; y < rect.End.Y; ++y)
				{
					var point = new Point2D(x, y);
					if (!blocks.Contains(point))
					{
						blocks.Add(point);
					}
				}
			}
		}

		if (blocks.Count > 500)
		{
			m.SendMessage("Due to size of the area, skipping the preview.");
			return;
		}

		foreach (var item in from Point2D p in blocks
		         let avgz = Map.GetAverageZ(p.X, p.Y)
		         select new Item(0x1766)
		         {
			         Name = "Area Preview",
			         Movable = false,
			         Location = new Point3D(p.X, p.Y, avgz <= m.Z ? m.Z + 2 : avgz + 2),
			         Map = Map
		         })
		{
			_cPreviewItems.Add(item);
		}

		_cPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
	}

	public void ShowSignPreview()
	{
		ClearPreview();
		bool northSouth = (ItemId == (int)SignIDs.TownHouseSignNS);

		//Item sign = new(0xBD2) { Name = "Sign Preview", Movable = false, Location = SignLoc, Map = Map };
		int signId = (int)(northSouth ? SignIDs.HouseSignNS : SignIDs.HouseSignWE);
		Item sign = new(signId)
		{
			Name = "Sign Preview",
			Movable = false,
			Location = SignLoc,
			Map = Map
		};

		_cPreviewItems.Add(sign);

		//sign = new Item(0xB98) { Name = "Sign Preview", Movable = false, Location = SignLoc, Map = Map };

		int hangerId = (int)(northSouth ? SignIDs.SignHangerNS : SignIDs.SignHangerWE);
		sign = new Item(hangerId)
		{
			Name = "Sign Preview",
			Movable = false,
			Location = SignLoc,
			Map = Map
		};

		_cPreviewItems.Add(sign);

		_cPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
	}

	public void ShowBanPreview()
	{
		ClearPreview();

		Item ban = new(0x17EE) { Name = "Ban Loc Preview", Movable = false, Location = BanLoc, Map = Map };

		_cPreviewItems.Add(ban);

		_cPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
	}

	public void ShowFloorsPreview(Mobile m)
	{
		ClearPreview();

		Item item = new(0x7BD)
		{
			Name = "Bottom Floor Preview",
			Movable = false,
			Location = m.Location,
			Z = _cMinZ,
			Map = Map
		};

		_cPreviewItems.Add(item);

		item = new Item(0x7BD)
		{
			Name = "Top Floor Preview",
			Movable = false,
			Location = m.Location,
			Z = _cMaxZ,
			Map = Map
		};

		_cPreviewItems.Add(item);

		_cPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
	}

	public void ClearPreview()
	{
		foreach (Item item in new ArrayList(_cPreviewItems))
		{
			_cPreviewItems.Remove(item);
			item.Delete();
		}

		_cPreviewTimer?.Stop();

		_cPreviewTimer = null;
	}
	public void Purchase(Mobile m)
	{
		Purchase(m, false);
	}

	public void Purchase(Mobile m, bool sellitems = false)
	{
		try
		{
			if (Owned)
			{
				m.SendMessage("Someone already owns this house!");
				return;
			}

			if (!PriceReady)
			{
				m.SendMessage("The setup for this house is not yet complete.");
				return;
			}

			int price = _cPrice + (sellitems ? _cItemsPrice : 0);

			if (_cFree)
			{
				price = 0;
			}
			var currency = Currency != null && Currency != typeof(Gold);
			if (m.AccessLevel == AccessLevel.Player && !Banker.Withdraw(m, price))
			{
				m.SendMessage("You cannot afford this house.");
				return;
			}
			if (currency)
			{
				if (m.AccessLevel == AccessLevel.Player && !m.BankBox.ConsumeTotal(Currency, price) &&
				    !m.Backpack.ConsumeTotal(Currency, price))
				{
					m.SendMessage("You cannot afford this house.");
					return;
				}
			}
			if (m.AccessLevel == AccessLevel.Player)
			{
				m.SendLocalizedMessage(1060398, price.ToString());
				// ~1_AMOUNT~ gold has been withdrawn from your bank box.
			}

			Visible = false;

			int minX = Blocks[0].Start.X;
			int minY = Blocks[0].Start.Y;
			int maxX = Blocks[0].End.X;
			int maxY = Blocks[0].End.Y;

			foreach (Rectangle2D rect in Blocks)
			{
				if (rect.Start.X < minX)
				{
					minX = rect.Start.X;
				}
				if (rect.Start.Y < minY)
				{
					minY = rect.Start.Y;
				}
				if (rect.End.X > maxX)
				{
					maxX = rect.End.X;
				}
				if (rect.End.Y > maxY)
				{
					maxY = rect.End.Y;
				}
			}

			bool northSouth = ItemId == (int)SignIDs.TownHouseSignNS;

			House = new TownHouse(m, this, _cLocks, _cSecures);
			int signId = (int)(northSouth ? SignIDs.HouseSignNS : SignIDs.HouseSignWE);
			House.ChangeSignType(signId);

			House.Components.Resize(maxX - minX, maxY - minY);
			House.Components.Add(0x520, House.Components.Width - 1, House.Components.Height - 1, -5);

			House.Location = new Point3D(minX, minY, Map.GetAverageZ(minX, minY));
			House.Map = Map;
			House.Region.GoLocation = _cBanLoc;
			House.Sign.Location = _cSignLoc;
			int hangerId = (int)(northSouth ? SignIDs.SignHangerNS : SignIDs.SignHangerWE);
			House.Hanger = new Item(hangerId)
			{
				Location = _cSignLoc,
				Map = Map,
				Movable = false
			};
			//House.Hanger = new Item(0xB98) { Location = m_CSignLoc, Map = Map, Movable = false };

			if (_cForcePublic)
			{
				House.Public = true;
			}

			House.Price = (RentByTime == TimeSpan.FromDays(0) ? _cPrice : 1);

			VersionCommand.UpdateRegion(this);

			if (House.Price == 0)
			{
				House.Price = 1;
			}

			if (_cRentByTime != TimeSpan.Zero)
			{
				BeginRentTimer(_cRentByTime);
			}

			_cRtoPayments = 1;

			HideOtherSigns();

			_cDecoreItemInfos = new List<DecoreItemInfo>();

			ConvertItems(sellitems);
		}
		catch (Exception e)
		{
			Errors.Report(
				"An error occurred during home purchasing.  More information available on the console.");
			Console.WriteLine(e.Message);
			Console.WriteLine(e.Source);
			Console.WriteLine(e.StackTrace);
		}
	}

	private void HideOtherSigns()
	{
		foreach (var item in House.Sign.GetItemsInRange(0))
		{
			if (item is HouseSign)
				continue;

			if (item != null && (item.ItemId is 0xB95 or 0xB96 or 0xC43 or 0xC44 || item.ItemId is > 0xBA3 and < 0xC0E))
			{
				item.Visible = false;
			}
		}
	}

	protected virtual void ConvertItems(bool keep)
	{
		if (House == null)
		{
			return;
		}

		List<Item> items = new();
		foreach (Item item in Blocks.SelectMany(rect => Map.GetItemsInBounds(rect).Where(item => House.Region.Contains(item.Location) && item.RootParent == null && !items.Contains(item))))
		{
			items.Add(item);
		}

		foreach (Item item in items.Where(item => item is not HouseSign && item is not BaseMulti && item is not BaseAddon && item is not AddonComponent && item != House.Hanger && item.Visible && !item.IsLockedDown && !item.IsSecure && !item.Movable && !_cPreviewItems.Contains(item)))
		{
			if (item is BaseDoor door)
			{
				ConvertDoor(door);
			}
			else if (!_cLeaveItems)
			{
				_cDecoreItemInfos.Add(new DecoreItemInfo(item.GetType().ToString(), item.Name, item.ItemId, item.Hue,
					item.Location, item.Map));

				if (!_cKeepItems || !keep)
				{
					item.Delete();
				}
				else
				{
					item.Movable = true;
					House.LockDown(House.Owner, item, false);
				}
			}
		}
	}

	protected void ConvertDoor(BaseDoor door)
	{
		if (!Owned)
		{
			return;
		}

		if (door is ISecurable)
		{
			door.Locked = false;
			House.Doors.Add(door);
			return;
		}

		door.Open = false;

		GenericHouseDoor newdoor = new(0, door.ClosedId, door.OpenedSound, door.ClosedSound)
		{
			Offset = door.Offset,
			ClosedId = door.ClosedId,
			OpenedId = door.OpenedId,
			Location = door.Location,
			Map = door.Map
		};

		door.Delete();

		foreach (Item inneritem in newdoor.GetItemsInRange(1).Where(inneritem => inneritem is BaseDoor && inneritem != newdoor && inneritem.Z == newdoor.Z))
		{
			((BaseDoor)inneritem).Link = newdoor;
			newdoor.Link = (BaseDoor)inneritem;
		}

		House.Doors.Add(newdoor);
	}

	protected virtual void UnconvertDoors()
	{
		if (House == null)
		{
			return;
		}

		foreach (BaseDoor door in new ArrayList(House.Doors))
		{
			door.Open = false;

			if (Relock)
			{
				door.Locked = true;
			}

			BaseDoor newdoor = new StrongWoodDoor(0)
			{
				ItemId = door.ItemId,
				ClosedId = door.ClosedId,
				OpenedId = door.OpenedId,
				OpenedSound = door.OpenedSound,
				ClosedSound = door.ClosedSound,
				Offset = door.Offset,
				Location = door.Location,
				Map = door.Map
			};

			door.Delete();

			foreach (Item inneritem in newdoor.GetItemsInRange(1).Where(inneritem => inneritem is BaseDoor && inneritem != newdoor && inneritem.Z == newdoor.Z))
			{
				((BaseDoor)inneritem).Link = newdoor;
				newdoor.Link = (BaseDoor)inneritem;
			}

			House.Doors.Remove(door);
		}
	}

	private void RecreateItems()
	{
		foreach (DecoreItemInfo info in _cDecoreItemInfos)
		{
			Item item;
			if (info.TypeString.ToLower().IndexOf("static", StringComparison.Ordinal) != -1)
			{
				item = new Static(info.ItemId);
			}
			else
			{
				try
				{
					item = Activator.CreateInstance(Assembler.FindTypeByFullName(info.TypeString)) as Item;
				}
				catch
				{
					continue;
				}
			}

			if (item == null)
			{
				continue;
			}

			item.ItemId = info.ItemId;
			item.Name = info.Name;
			item.Hue = info.Hue;
			item.Location = info.Location;
			item.Map = info.Map;
			item.Movable = false;
		}
	}

	public virtual void ClearHouse()
	{
		UnconvertDoors();
		ClearDemolishTimer();
		ClearRentTimer();
		PackUpItems();
		RecreateItems();
		House = null;
		Visible = true;

		if (_cRentToOwn)
		{
			_cRentByTime = _cOriginalRentTime;
		}
	}

	public virtual void ValidateOwnership()
	{
		if (!Owned)
		{
			return;
		}

		if (House.Owner == null)
		{
			House.Delete();
			return;
		}

		if (House.Owner.AccessLevel != AccessLevel.Player)
		{
			return;
		}

		if (!CanBuyHouse(House.Owner) && DemolishTimer == null)
		{
			BeginDemolishTimer();
		}
		else
		{
			ClearDemolishTimer();
		}
	}

	public int CalcVolume()
	{
		int floors = 1;
		if (_cMaxZ - _cMinZ < 100)
		{
			floors = 1 + Math.Abs((_cMaxZ - _cMinZ) / 20);
		}

		List<Point3D> blocks = new();

		foreach (Rectangle2D rect in Blocks)
		{
			for (int x = rect.Start.X; x < rect.End.X; ++x)
			{
				for (int y = rect.Start.Y; y < rect.End.Y; ++y)
				{
					for (int z = 0; z < floors; z++)
					{
						Point3D point = new(x, y, z);
						if (!blocks.Contains(point))
						{
							blocks.Add(point);
						}
					}
				}
			}
		}
		return blocks.Count;
	}

	private void StartTimers()
	{
		if (DemolishTime > DateTime.UtcNow)
		{
			BeginDemolishTimer(DemolishTime - DateTime.UtcNow);
		}
		else if (_cRentByTime != TimeSpan.Zero)
		{
			BeginRentTimer(_cRentByTime);
		}
	}

	#region Demolish

	protected void ClearDemolishTimer()
	{
		if (DemolishTimer == null)
		{
			return;
		}

		DemolishTimer.Stop();
		DemolishTimer = null;
		DemolishTime = DateTime.UtcNow;

		if (!House.Deleted && Owned)
		{
			House.Owner.SendMessage("Demolition canceled.");
		}
	}

	public void CheckDemolishTimer()
	{
		if (DemolishTimer == null || !Owned)
		{
			return;
		}

		DemolishAlert();
	}

	private void BeginDemolishTimer()
	{
		BeginDemolishTimer(TimeSpan.FromHours(24));
	}

	protected void BeginDemolishTimer(TimeSpan time)
	{
		if (!Owned)
		{
			return;
		}

		DemolishTime = DateTime.UtcNow + time;
		DemolishTimer = Timer.DelayCall(time, PackUpHouse);

		DemolishAlert();
	}

	protected virtual void DemolishAlert()
	{
		House.Owner.SendMessage(
			"You no longer meet the requirements for your town house, which will be demolished automatically in {0}:{1}:{2}.",
			(DemolishTime - DateTime.UtcNow).Hours, (DemolishTime - DateTime.UtcNow).Minutes,
			(DemolishTime - DateTime.UtcNow).Seconds);
	}

	protected void PackUpHouse()
	{
		if (!Owned || House.Deleted)
		{
			return;
		}

		PackUpItems();

		House.Owner.BankBox.DropItem(new BankCheck(House.Price));

		House.Delete();
	}

	private void PackUpItems()
	{
		if (House == null)
		{
			return;
		}

		Container bag = new Bag
		{
			Name = "Town House Belongings"
		};

		foreach (Item item in new ArrayList(House.LockDowns))
		{
			item.IsLockedDown = false;
			item.Movable = true;
			House.LockDowns.Remove(item);
			bag.DropItem(item);
		}

		foreach (SecureInfo info in new ArrayList(House.Secures))
		{
			info.Item.IsLockedDown = false;
			info.Item.IsSecure = false;
			info.Item.Movable = true;
			info.Item.SetLastMoved();
			House.Secures.Remove(info);
			bag.DropItem(info.Item);
		}

		foreach (Item item in Blocks.SelectMany(rect => Map.GetItemsInBounds(rect).Where(item => item is not HouseSign && item is not BaseDoor && item is not BaseMulti && item is not BaseAddon && item is not AddonComponent && item.Visible && !item.IsLockedDown && !item.IsSecure && item.Movable && item.Map == House.Map && House.Region.Contains(item.Location))))
		{
			bag.DropItem(item);
		}

		if (bag.Items.Count == 0)
		{
			bag.Delete();
			return;
		}

		House.Owner.BankBox.DropItem(bag);
	}

	#endregion

	#region Rent

	public void ClearRentTimer()
	{
		if (_cRentTimer != null)
		{
			_cRentTimer.Stop();
			_cRentTimer = null;
		}

		CRentTime = DateTime.UtcNow;
	}
		
	//public void BeginRentTimer()
	//{
	//	BeginRentTimer(TimeSpan.FromDays(1));
	//}
		
	public void BeginRentTimer(TimeSpan time)
	{
		if (!Owned)
		{
			return;
		}

		_cRentTimer = Timer.DelayCall(time, RentDue);
		CRentTime = DateTime.UtcNow + time;
	}

	public void CheckRentTimer()
	{
		if (_cRentTimer == null || !Owned)
		{
			return;
		}

		House.Owner.SendMessage("This rent cycle ends in {0} days, {1}:{2}:{3}.", (CRentTime - DateTime.UtcNow).Days,
			(CRentTime - DateTime.UtcNow).Hours, (CRentTime - DateTime.UtcNow).Minutes,
			(CRentTime - DateTime.UtcNow).Seconds);
	}

	private void RentDue()
	{
		if (!Owned || House.Owner == null)
		{
			return;
		}

		if (!_cRecurRent)
		{
			House.Owner.SendMessage(
				"Your town house rental contract has expired, and the bank has once again taken possession.");
			PackUpHouse();
			return;
		}

		if (!_cFree && House.Owner.AccessLevel == AccessLevel.Player && !Banker.Withdraw(House.Owner, _cPrice))
		{
			House.Owner.SendMessage("Since you can not afford the rent, the bank has reclaimed your town house.");
			PackUpHouse();
			return;
		}

		if (!_cFree)
		{
			House.Owner.SendMessage("The bank has withdrawn {0} gold rent for your town house.", _cPrice);
		}

		OnRentPaid();

		if (_cRentToOwn)
		{
			_cRtoPayments++;

			bool complete = false;

			if (_cRentByTime == TimeSpan.FromDays(1) && _cRtoPayments >= 60)
			{
				complete = true;
				House.Price = _cPrice * 60;
			}

			if (_cRentByTime == TimeSpan.FromDays(7) && _cRtoPayments >= 9)
			{
				complete = true;
				House.Price = _cPrice * 9;
			}

			if (_cRentByTime == TimeSpan.FromDays(30) && _cRtoPayments >= 2)
			{
				complete = true;
				House.Price = _cPrice * 2;
			}

			if (complete)
			{
				House.Owner.SendMessage("You now own your rental home.");
				_cRentByTime = TimeSpan.FromDays(0);
				return;
			}
		}

		BeginRentTimer(_cRentByTime);
	}

	protected virtual void OnRentPaid()
	{
	}

	public void NextPriceType()
	{
		if (_cRentByTime == TimeSpan.Zero)
		{
			RentByTime = TimeSpan.FromDays(1);
		}
		else if (_cRentByTime == TimeSpan.FromDays(1))
		{
			RentByTime = TimeSpan.FromDays(7);
		}
		else if (_cRentByTime == TimeSpan.FromDays(7))
		{
			RentByTime = TimeSpan.FromDays(30);
		}
		else
		{
			RentByTime = TimeSpan.Zero;
		}
	}

	public void PrevPriceType()
	{
		if (_cRentByTime == TimeSpan.Zero)
		{
			RentByTime = TimeSpan.FromDays(30);
		}
		else if (_cRentByTime == TimeSpan.FromDays(30))
		{
			RentByTime = TimeSpan.FromDays(7);
		}
		else if (_cRentByTime == TimeSpan.FromDays(7))
		{
			RentByTime = TimeSpan.FromDays(1);
		}
		else
		{
			RentByTime = TimeSpan.Zero;
		}
	}

	#endregion

	private bool CanBuyHouse(Mobile m)
	{
		if (_cSkill != "")
		{
			try
			{
				SkillName index = (SkillName)Enum.Parse(typeof(SkillName), _cSkill, true);
				if (m.Skills[index].Value < _cSkillReq)
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
		}

		if (_cMinTotalSkill != 0 && m.SkillsTotal / 10 < _cMinTotalSkill)
		{
			return false;
		}

		if (_cMaxTotalSkill != 0 && m.SkillsTotal / 10 > _cMaxTotalSkill)
		{
			return false;
		}

		if (_cYoungOnly && m.Player && !((PlayerMobile)m).Young)
		{
			return false;
		}

		if (_cMurderers == Intu.Yes && m.Kills < 5)
		{
			return false;
		}

		return _cMurderers != Intu.No || m.Kills < 5;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (m.AccessLevel != AccessLevel.Player)
		{
			_ = new TownHouseSetupGump(m, this);
		}
		else if (!Visible)
		{
		}
		else if (CanBuyHouse(m) && !BaseHouse.AtAccountHouseLimit(m))
		{
			_ = new TownHouseConfirmGump(m, this);
		}
		else
		{
			m.SendMessage("You cannot purchase this house.");
		}
	}

	public override void Delete()
	{
		if (House == null || House.Deleted)
		{
			base.Delete();
		}
		else
		{
			PublicOverheadMessage(MessageType.Regular, 0x0, true, "You cannot delete this while the home is owned.");
		}

		if (Deleted)
		{
			AllSigns.Remove(this);
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (_cFree)
		{
			list.Add(1060658, "Price\tFree");
		}
		else if (_cRentByTime == TimeSpan.Zero)
		{
			list.Add(1060658, "Price\t{0}{1}", _cPrice, _cKeepItems ? " (+" + _cItemsPrice + " for the items)" : "");
		}
		else if (_cRecurRent)
		{
			list.Add(1060658, "{0}\t{1}\r{2}", PriceType + (_cRentToOwn ? " Rent-to-Own" : " Recurring"), _cPrice,
				_cKeepItems ? " (+" + _cItemsPrice + " for the items)" : "");
		}
		else
		{
			list.Add(1060658, "One {0}\t{1}{2}", PriceTypeShort, _cPrice,
				_cKeepItems ? " (+" + _cItemsPrice + " for the items)" : "");
		}

		list.Add(1060659, "Lockdowns\t{0}", _cLocks);
		list.Add(1060660, "Secures\t{0}", _cSecures);

		if (_cSkillReq != 0.0)
		{
			list.Add(1060661, "Requires\t{0}", _cSkillReq + " in " + _cSkill);
		}
		if (_cMinTotalSkill != 0)
		{
			list.Add(1060662, "Requires more than\t{0} total skills", _cMinTotalSkill);
		}
		if (_cMaxTotalSkill != 0)
		{
			list.Add(1060663, "Requires less than\t{0} total skills", _cMaxTotalSkill);
		}

		if (_cYoungOnly)
		{
			list.Add(1063483, "Must be\tYoung");
		}
		else
		{
			switch (_cMurderers)
			{
				case Intu.Yes:
					list.Add(1063483, "Must be\ta murderer");
					break;
				case Intu.No:
					list.Add(1063483, "Must be\tinnocent");
					break;
			}
		}
	}

	public TownHouseSign(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(13);

		// Version 13

		writer.Write(_cForcePrivate);
		writer.Write(_cForcePublic);
		writer.Write(NoHouseTrade);

		// Version 12

		writer.Write(_cFree);

		// Version 11

		writer.Write((int)_cMurderers);

		// Version 10

		writer.Write(_cLeaveItems);

		// Version 9
		writer.Write(_cRentToOwn);
		writer.Write(_cOriginalRentTime);
		writer.Write(_cRtoPayments);

		// Version 7
		//writer.WriteItemList( c_PreviewItems, true );
		writer.Write(_cPreviewItems.Count);
		foreach (Item item in _cPreviewItems)
		{
			writer.Write(item);
		}

		// Version 6
		writer.Write(_cItemsPrice);
		writer.Write(_cKeepItems);

		// Version 5
		writer.Write(_cDecoreItemInfos.Count);
		foreach (DecoreItemInfo info in _cDecoreItemInfos)
		{
			info.Save(writer);
		}

		writer.Write(Relock);

		// Version 4
		writer.Write(_cRecurRent);
		writer.Write(_cRentByTime);
		writer.Write(CRentTime);
		writer.Write(DemolishTime);
		writer.Write(_cYoungOnly);
		writer.Write(_cMinTotalSkill);
		writer.Write(_cMaxTotalSkill);

		// Version 3
		writer.Write(_cMinZ);
		writer.Write(_cMaxZ);

		// Version 2
		writer.Write(House);

		// Version 1
		writer.Write(_cPrice);
		writer.Write(_cLocks);
		writer.Write(_cSecures);
		writer.Write(_cBanLoc);
		writer.Write(_cSignLoc);
		writer.Write(_cSkill);
		writer.Write(_cSkillReq);
		writer.Write(Blocks.Count);
		foreach (Rectangle2D rect in Blocks)
		{
			writer.Write(rect);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		if (version >= 13)
		{
			_cForcePrivate = reader.ReadBool();
			_cForcePublic = reader.ReadBool();
			NoHouseTrade = reader.ReadBool();
		}

		if (version >= 12)
		{
			_cFree = reader.ReadBool();
		}

		if (version >= 11)
		{
			_cMurderers = (Intu)reader.ReadInt();
		}

		if (version >= 10)
		{
			_cLeaveItems = reader.ReadBool();
		}

		if (version >= 9)
		{
			_cRentToOwn = reader.ReadBool();
			_cOriginalRentTime = reader.ReadTimeSpan();
			_cRtoPayments = reader.ReadInt();
		}

		_cPreviewItems = new List<Item>();
		if (version >= 7)
		{
			var previewcount = reader.ReadInt();
			for (var i = 0; i < previewcount; ++i)
			{
				var item = reader.ReadItem();
				_cPreviewItems.Add(item);
			}
		}

		if (version >= 6)
		{
			_cItemsPrice = reader.ReadInt();
			_cKeepItems = reader.ReadBool();
		}

		_cDecoreItemInfos = new List<DecoreItemInfo>();
		if (version >= 5)
		{
			var decorecount = reader.ReadInt();
			for (var i = 0; i < decorecount; ++i)
			{
				var info = new DecoreItemInfo();
				info.Load(reader);
				_cDecoreItemInfos.Add(info);
			}

			Relock = reader.ReadBool();
		}

		if (version >= 4)
		{
			_cRecurRent = reader.ReadBool();
			_cRentByTime = reader.ReadTimeSpan();
			CRentTime = reader.ReadDateTime();
			DemolishTime = reader.ReadDateTime();
			_cYoungOnly = reader.ReadBool();
			_cMinTotalSkill = reader.ReadInt();
			_cMaxTotalSkill = reader.ReadInt();
		}

		if (version >= 3)
		{
			_cMinZ = reader.ReadInt();
			_cMaxZ = reader.ReadInt();
		}

		if (version >= 2)
		{
			House = (TownHouse)reader.ReadItem();
		}

		_cPrice = reader.ReadInt();
		_cLocks = reader.ReadInt();
		_cSecures = reader.ReadInt();
		_cBanLoc = reader.ReadPoint3D();
		_cSignLoc = reader.ReadPoint3D();
		_cSkill = reader.ReadString();
		_cSkillReq = reader.ReadDouble();

		Blocks = new List<Rectangle2D>();
		var count = reader.ReadInt();
		for (var i = 0; i < count; ++i)
		{
			Blocks.Add(reader.ReadRect2D());
		}

		if (CRentTime > DateTime.UtcNow)
		{
			BeginRentTimer(CRentTime - DateTime.UtcNow);
		}

		Timer.DelayCall(TimeSpan.Zero, StartTimers);

		ClearPreview();

		AllSigns.Add(this);
	}
}
