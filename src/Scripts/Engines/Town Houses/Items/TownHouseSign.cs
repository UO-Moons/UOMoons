using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.TownHouses
{
	public enum Intu
	{
		Neither,
		No,
		Yes
	}

	[Flipable(0xC0B, 0xC0C)]
	public class TownHouseSign : BaseItem
	{
		public static List<TownHouseSign> AllSigns { get; } = new List<TownHouseSign>();

		private Point3D c_BanLoc, c_SignLoc;

		private int c_Locks,
			c_Secures,
			c_Price,
			c_MinZ,
			c_MaxZ,
			c_MinTotalSkill,
			c_MaxTotalSkill,
			c_ItemsPrice,
			c_RTOPayments;

		private bool c_YoungOnly,
			c_RecurRent, c_KeepItems,
			c_LeaveItems,
			c_RentToOwn,
			c_Free,
			c_ForcePrivate,
			c_ForcePublic, c_NoBanning;

		private string c_Skill;
		private double c_SkillReq;
		private List<DecoreItemInfo> c_DecoreItemInfos;
		private List<Item> c_PreviewItems;
		private Timer c_RentTimer, c_PreviewTimer;
		private DateTime c_RentTime;
		private TimeSpan c_RentByTime, c_OriginalRentTime;
		private Intu c_Murderers;

		public Point3D BanLoc
		{
			get => c_BanLoc;
			set
			{
				c_BanLoc = value;
				InvalidateProperties();
				if (Owned)
				{
					House.Region.GoLocation = value;
				}
			}
		}

		public Point3D SignLoc
		{
			get => c_SignLoc;
			set
			{
				c_SignLoc = value;
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
			get => c_Locks;
			set
			{
				c_Locks = value;
				InvalidateProperties();
				if (Owned)
				{
					House.MaxLockDowns = value;
				}
			}
		}

		public int Secures
		{
			get => c_Secures;
			set
			{
				c_Secures = value;
				InvalidateProperties();
				if (Owned)
				{
					House.MaxSecures = value;
				}
			}
		}

		public int Price
		{
			get => c_Price;
			set
			{
				c_Price = value;
				InvalidateProperties();
			}
		}

		public int MinZ
		{
			get => c_MinZ;
			set
			{
				if (value > c_MaxZ)
				{
					c_MaxZ = value + 1;
				}

				c_MinZ = value;
				if (Owned)
				{
					VersionCommand.UpdateRegion(this);
				}
			}
		}

		public int MaxZ
		{
			get => c_MaxZ;
			set
			{
				if (value < c_MinZ)
				{
					value = c_MinZ;
				}

				c_MaxZ = value;
				if (Owned)
				{
					VersionCommand.UpdateRegion(this);
				}
			}
		}

		public int MinTotalSkill
		{
			get => c_MinTotalSkill;
			set
			{
				if (value > c_MaxTotalSkill)
				{
					value = c_MaxTotalSkill;
				}

				c_MinTotalSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public int MaxTotalSkill
		{
			get => c_MaxTotalSkill;
			set
			{
				if (value < c_MinTotalSkill)
				{
					value = c_MinTotalSkill;
				}

				c_MaxTotalSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool YoungOnly
		{
			get => c_YoungOnly;
			set
			{
				c_YoungOnly = value;

				if (c_YoungOnly)
				{
					c_Murderers = Intu.Neither;
				}

				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public TimeSpan RentByTime
		{
			get => c_RentByTime;
			set
			{
				c_RentByTime = value;
				c_OriginalRentTime = value;

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
			get => c_RecurRent;
			set
			{
				c_RecurRent = value;

				if (!value)
				{
					c_RentToOwn = value;
				}

				InvalidateProperties();
			}
		}

		public bool KeepItems
		{
			get => c_KeepItems;
			set
			{
				c_LeaveItems = false;
				c_KeepItems = value;
				InvalidateProperties();
			}
		}

		public bool Free
		{
			get => c_Free;
			set
			{
				c_Free = value;
				c_Price = 1;
				InvalidateProperties();
			}
		}

		public Intu Murderers
		{
			get => c_Murderers;
			set
			{
				c_Murderers = value;

				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool ForcePrivate
		{
			get => c_ForcePrivate;
			set
			{
				c_ForcePrivate = value;

				if (value)
				{
					c_ForcePublic = false;

					if (House != null)
					{
						House.Public = false;
					}
				}
			}
		}

		public bool ForcePublic
		{
			get => c_ForcePublic;
			set
			{
				c_ForcePublic = value;

				if (value)
				{
					c_ForcePrivate = false;

					if (House != null)
					{
						House.Public = true;
					}
				}
			}
		}

		public bool NoBanning
		{
			get => c_NoBanning;
			set
			{
				c_NoBanning = value;

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
			get => c_Skill;
			set
			{
				c_Skill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public double SkillReq
		{
			get => c_SkillReq;
			set
			{
				c_SkillReq = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool LeaveItems
		{
			get => c_LeaveItems;
			set
			{
				c_LeaveItems = value;
				InvalidateProperties();
			}
		}

		public bool RentToOwn
		{
			get => c_RentToOwn;
			set
			{
				c_RentToOwn = value;
				InvalidateProperties();
			}
		}

		public bool Relock { get; set; }

		public bool NoHouseTrade { get; set; }

		public int ItemsPrice
		{
			get => c_ItemsPrice;
			set
			{
				c_ItemsPrice = value;
				InvalidateProperties();
			}
		}

		public TownHouse House { get; set; }

		protected Timer DemolishTimer { get; private set; }

		protected DateTime DemolishTime { get; private set; }

		public bool Owned => House != null && !House.Deleted;

		public int Floors => (c_MaxZ - c_MinZ) / 20 + 1;

		public bool BlocksReady => Blocks.Count != 0;

		public bool FloorsReady => (BlocksReady && MinZ != short.MinValue);

		public bool SignReady => (FloorsReady && SignLoc != Point3D.Zero);

		public bool BanReady => (SignReady && BanLoc != Point3D.Zero);

		public bool LocSecReady => (BanReady && Locks != 0 && Secures != 0);

		public bool ItemsReady => LocSecReady;

		public bool LengthReady => ItemsReady;

		public bool PriceReady => (LengthReady && Price != 0);

		public string PriceType
		{
			get
			{
				if (c_RentByTime == TimeSpan.Zero)
				{
					return "Sale";
				}
				if (c_RentByTime == TimeSpan.FromDays(1))
				{
					return "Daily";
				}
				if (c_RentByTime == TimeSpan.FromDays(7))
				{
					return "Weekly";
				}
				return c_RentByTime == TimeSpan.FromDays(30) ? "Monthly" : "Sale";
			}
		}

		public string PriceTypeShort
		{
			get
			{
				if (c_RentByTime == TimeSpan.Zero)
				{
					return "Sale";
				}
				if (c_RentByTime == TimeSpan.FromDays(1))
				{
					return "Day";
				}
				if (c_RentByTime == TimeSpan.FromDays(7))
				{
					return "Week";
				}
				return c_RentByTime == TimeSpan.FromDays(30) ? "Month" : "Sale";
			}
		}

		[Constructable]
		public TownHouseSign() : base(0xC0B)
		{
			Name = "This building is for sale or rent!";
			Movable = false;

			c_BanLoc = Point3D.Zero;
			c_SignLoc = Point3D.Zero;
			c_Skill = "";
			Blocks = new List<Rectangle2D>();
			c_DecoreItemInfos = new List<DecoreItemInfo>();
			c_PreviewItems = new List<Item>();
			DemolishTime = DateTime.UtcNow;
			c_RentTime = DateTime.UtcNow;
			c_RentByTime = TimeSpan.Zero;
			c_RecurRent = true;

			c_MinZ = short.MinValue;
			c_MaxZ = short.MaxValue;

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

			Point2D point = Point2D.Zero;
			ArrayList blocks = new();

			foreach (Rectangle2D rect in Blocks)
			{
				for (int x = rect.Start.X; x < rect.End.X; ++x)
				{
					for (int y = rect.Start.Y; y < rect.End.Y; ++y)
					{
						point = new Point2D(x, y);
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

			foreach (Item item in from Point2D p in blocks
								  let avgz = Map.GetAverageZ(p.X, p.Y)
								  select new Item(0x1766)
								  {
									  Name = "Area Preview",
									  Movable = false,
									  Location = new Point3D(p.X, p.Y, (avgz <= m.Z ? m.Z + 2 : avgz + 2)),
									  Map = Map
								  })
			{
				c_PreviewItems.Add(item);
			}

			c_PreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ShowSignPreview()
		{
			ClearPreview();

			Item sign = new(0xBD2) { Name = "Sign Preview", Movable = false, Location = SignLoc, Map = Map };

			c_PreviewItems.Add(sign);

			sign = new Item(0xB98) { Name = "Sign Preview", Movable = false, Location = SignLoc, Map = Map };

			c_PreviewItems.Add(sign);

			c_PreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ShowBanPreview()
		{
			ClearPreview();

			Item ban = new(0x17EE) { Name = "Ban Loc Preview", Movable = false, Location = BanLoc, Map = Map };

			c_PreviewItems.Add(ban);

			c_PreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ShowFloorsPreview(Mobile m)
		{
			ClearPreview();

			Item item = new(0x7BD)
			{
				Name = "Bottom Floor Preview",
				Movable = false,
				Location = m.Location,
				Z = c_MinZ,
				Map = Map
			};

			c_PreviewItems.Add(item);

			item = new Item(0x7BD)
			{
				Name = "Top Floor Preview",
				Movable = false,
				Location = m.Location,
				Z = c_MaxZ,
				Map = Map
			};

			c_PreviewItems.Add(item);

			c_PreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ClearPreview()
		{
			foreach (Item item in new ArrayList(c_PreviewItems))
			{
				c_PreviewItems.Remove(item);
				item.Delete();
			}

			if (c_PreviewTimer != null)
			{
				c_PreviewTimer.Stop();
			}

			c_PreviewTimer = null;
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

				int price = c_Price + (sellitems ? c_ItemsPrice : 0);

				if (c_Free)
				{
					price = 0;
				}
				var currency = Currency != null && Currency != typeof(Gold);
				//if (m.AccessLevel == AccessLevel.Player && !Banker.Withdraw(m, price))
				//{
				//    m.SendMessage("You cannot afford this house.");
				//    return;
				//}
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

				House = new TownHouse(m, this, c_Locks, c_Secures);

				House.Components.Resize(maxX - minX, maxY - minY);
				House.Components.Add(0x520, House.Components.Width - 1, House.Components.Height - 1, -5);

				House.Location = new Point3D(minX, minY, Map.GetAverageZ(minX, minY));
				House.Map = Map;
				House.Region.GoLocation = c_BanLoc;
				House.Sign.Location = c_SignLoc;
				House.Hanger = new Item(0xB98) { Location = c_SignLoc, Map = Map, Movable = false };

				if (c_ForcePublic)
				{
					House.Public = true;
				}

				House.Price = (RentByTime == TimeSpan.FromDays(0) ? c_Price : 1);

				VersionCommand.UpdateRegion(this);

				if (House.Price == 0)
				{
					House.Price = 1;
				}

				if (c_RentByTime != TimeSpan.Zero)
				{
					BeginRentTimer(c_RentByTime);
				}

				c_RTOPayments = 1;

				HideOtherSigns();

				c_DecoreItemInfos = new List<DecoreItemInfo>();

				ConvertItems(sellitems);
			}
			catch (Exception e)
			{
				Errors.Report(
					string.Format("An error occurred during home purchasing.  More information available on the console."));
				Console.WriteLine(e.Message);
				Console.WriteLine(e.Source);
				Console.WriteLine(e.StackTrace);
			}
		}

		private void HideOtherSigns()
		{
			foreach (Item item in House.Sign.GetItemsInRange(0).Where(item => item is not HouseSign).Where(item => item != null && (item.ItemID == 0xB95
																													|| item.ItemID == 0xB96
																													|| item.ItemID == 0xC43
																													|| item.ItemID == 0xC44
																													|| (item.ItemID > 0xBA3 && item.ItemID < 0xC0E))))
			{
				item.Visible = false;
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

			foreach (Item item in items.Where(item => item is not HouseSign && item is not BaseMulti && item is not BaseAddon && item is not AddonComponent && item != House.Hanger && item.Visible && !item.IsLockedDown && !item.IsSecure && !item.Movable && !c_PreviewItems.Contains(item)))
			{
				if (item is BaseDoor door)
				{
					ConvertDoor(door);
				}
				else if (!c_LeaveItems)
				{
					c_DecoreItemInfos.Add(new DecoreItemInfo(item.GetType().ToString(), item.Name, item.ItemID, item.Hue,
						item.Location, item.Map));

					if (!c_KeepItems || !keep)
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

			GenericHouseDoor newdoor = new(0, door.ClosedID, door.OpenedSound, door.ClosedSound)
			{
				Offset = door.Offset,
				ClosedID = door.ClosedID,
				OpenedID = door.OpenedID,
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

				BaseDoor newdoor = new StrongWoodDoor((DoorFacing)0)
				{
					ItemID = door.ItemID,
					ClosedID = door.ClosedID,
					OpenedID = door.OpenedID,
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
			foreach (DecoreItemInfo info in c_DecoreItemInfos)
			{
				Item item;
				if (info.TypeString.ToLower().IndexOf("static", StringComparison.Ordinal) != -1)
				{
					item = new Static(info.ItemID);
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

				item.ItemID = info.ItemID;
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

			if (c_RentToOwn)
			{
				c_RentByTime = c_OriginalRentTime;
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
			if (c_MaxZ - c_MinZ < 100)
			{
				floors = 1 + Math.Abs((c_MaxZ - c_MinZ) / 20);
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
			else if (c_RentByTime != TimeSpan.Zero)
			{
				BeginRentTimer(c_RentByTime);
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

		protected void ClearRentTimer()
		{
			if (c_RentTimer != null)
			{
				c_RentTimer.Stop();
				c_RentTimer = null;
			}

			c_RentTime = DateTime.UtcNow;
		}
		/*
		private void BeginRentTimer()
		{
			BeginRentTimer(TimeSpan.FromDays(1));
		}
		*/
		private void BeginRentTimer(TimeSpan time)
		{
			if (!Owned)
			{
				return;
			}

			c_RentTimer = Timer.DelayCall(time, RentDue);
			c_RentTime = DateTime.UtcNow + time;
		}

		public void CheckRentTimer()
		{
			if (c_RentTimer == null || !Owned)
			{
				return;
			}

			House.Owner.SendMessage("This rent cycle ends in {0} days, {1}:{2}:{3}.", (c_RentTime - DateTime.UtcNow).Days,
				(c_RentTime - DateTime.UtcNow).Hours, (c_RentTime - DateTime.UtcNow).Minutes,
				(c_RentTime - DateTime.UtcNow).Seconds);
		}

		private void RentDue()
		{
			if (!Owned || House.Owner == null)
			{
				return;
			}

			if (!c_RecurRent)
			{
				House.Owner.SendMessage(
					"Your town house rental contract has expired, and the bank has once again taken possession.");
				PackUpHouse();
				return;
			}

			if (!c_Free && House.Owner.AccessLevel == AccessLevel.Player && !Banker.Withdraw(House.Owner, c_Price))
			{
				House.Owner.SendMessage("Since you can not afford the rent, the bank has reclaimed your town house.");
				PackUpHouse();
				return;
			}

			if (!c_Free)
			{
				House.Owner.SendMessage("The bank has withdrawn {0} gold rent for your town house.", c_Price);
			}

			OnRentPaid();

			if (c_RentToOwn)
			{
				c_RTOPayments++;

				bool complete = false;

				if (c_RentByTime == TimeSpan.FromDays(1) && c_RTOPayments >= 60)
				{
					complete = true;
					House.Price = c_Price * 60;
				}

				if (c_RentByTime == TimeSpan.FromDays(7) && c_RTOPayments >= 9)
				{
					complete = true;
					House.Price = c_Price * 9;
				}

				if (c_RentByTime == TimeSpan.FromDays(30) && c_RTOPayments >= 2)
				{
					complete = true;
					House.Price = c_Price * 2;
				}

				if (complete)
				{
					House.Owner.SendMessage("You now own your rental home.");
					c_RentByTime = TimeSpan.FromDays(0);
					return;
				}
			}

			BeginRentTimer(c_RentByTime);
		}

		protected virtual void OnRentPaid()
		{
		}

		public void NextPriceType()
		{
			if (c_RentByTime == TimeSpan.Zero)
			{
				RentByTime = TimeSpan.FromDays(1);
			}
			else if (c_RentByTime == TimeSpan.FromDays(1))
			{
				RentByTime = TimeSpan.FromDays(7);
			}
			else if (c_RentByTime == TimeSpan.FromDays(7))
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
			if (c_RentByTime == TimeSpan.Zero)
			{
				RentByTime = TimeSpan.FromDays(30);
			}
			else if (c_RentByTime == TimeSpan.FromDays(30))
			{
				RentByTime = TimeSpan.FromDays(7);
			}
			else if (c_RentByTime == TimeSpan.FromDays(7))
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
			if (c_Skill != "")
			{
				try
				{
					SkillName index = (SkillName)Enum.Parse(typeof(SkillName), c_Skill, true);
					if (m.Skills[index].Value < c_SkillReq)
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}

			if (c_MinTotalSkill != 0 && m.SkillsTotal / 10 < c_MinTotalSkill)
			{
				return false;
			}

			if (c_MaxTotalSkill != 0 && m.SkillsTotal / 10 > c_MaxTotalSkill)
			{
				return false;
			}

			if (c_YoungOnly && m.Player && !((PlayerMobile)m).Young)
			{
				return false;
			}

			if (c_Murderers == Intu.Yes && m.Kills < 5)
			{
				return false;
			}

			return c_Murderers != Intu.No || m.Kills < 5;
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
			else if (CanBuyHouse(m) && !BaseHouse.HasReachedHouseLimit(m))
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

			if (c_Free)
			{
				list.Add(1060658, "Price\tFree");
			}
			else if (c_RentByTime == TimeSpan.Zero)
			{
				list.Add(1060658, "Price\t{0}{1}", c_Price, c_KeepItems ? " (+" + c_ItemsPrice + " for the items)" : "");
			}
			else if (c_RecurRent)
			{
				list.Add(1060658, "{0}\t{1}\r{2}", PriceType + (c_RentToOwn ? " Rent-to-Own" : " Recurring"), c_Price,
					c_KeepItems ? " (+" + c_ItemsPrice + " for the items)" : "");
			}
			else
			{
				list.Add(1060658, "One {0}\t{1}{2}", PriceTypeShort, c_Price,
					c_KeepItems ? " (+" + c_ItemsPrice + " for the items)" : "");
			}

			list.Add(1060659, "Lockdowns\t{0}", c_Locks);
			list.Add(1060660, "Secures\t{0}", c_Secures);

			if (c_SkillReq != 0.0)
			{
				list.Add(1060661, "Requires\t{0}", c_SkillReq + " in " + c_Skill);
			}
			if (c_MinTotalSkill != 0)
			{
				list.Add(1060662, "Requires more than\t{0} total skills", c_MinTotalSkill);
			}
			if (c_MaxTotalSkill != 0)
			{
				list.Add(1060663, "Requires less than\t{0} total skills", c_MaxTotalSkill);
			}

			if (c_YoungOnly)
			{
				list.Add(1063483, "Must be\tYoung");
			}
			else
			{
				switch (c_Murderers)
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

			writer.Write(c_ForcePrivate);
			writer.Write(c_ForcePublic);
			writer.Write(NoHouseTrade);

			// Version 12

			writer.Write(c_Free);

			// Version 11

			writer.Write((int)c_Murderers);

			// Version 10

			writer.Write(c_LeaveItems);

			// Version 9
			writer.Write(c_RentToOwn);
			writer.Write(c_OriginalRentTime);
			writer.Write(c_RTOPayments);

			// Version 7
			//writer.WriteItemList( c_PreviewItems, true );
			writer.Write(c_PreviewItems.Count);
			foreach (Item item in c_PreviewItems)
			{
				writer.Write(item);
			}

			// Version 6
			writer.Write(c_ItemsPrice);
			writer.Write(c_KeepItems);

			// Version 5
			writer.Write(c_DecoreItemInfos.Count);
			foreach (DecoreItemInfo info in c_DecoreItemInfos)
			{
				info.Save(writer);
			}

			writer.Write(Relock);

			// Version 4
			writer.Write(c_RecurRent);
			writer.Write(c_RentByTime);
			writer.Write(c_RentTime);
			writer.Write(DemolishTime);
			writer.Write(c_YoungOnly);
			writer.Write(c_MinTotalSkill);
			writer.Write(c_MaxTotalSkill);

			// Version 3
			writer.Write(c_MinZ);
			writer.Write(c_MaxZ);

			// Version 2
			writer.Write(House);

			// Version 1
			writer.Write(c_Price);
			writer.Write(c_Locks);
			writer.Write(c_Secures);
			writer.Write(c_BanLoc);
			writer.Write(c_SignLoc);
			writer.Write(c_Skill);
			writer.Write(c_SkillReq);
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
				c_ForcePrivate = reader.ReadBool();
				c_ForcePublic = reader.ReadBool();
				NoHouseTrade = reader.ReadBool();
			}

			if (version >= 12)
			{
				c_Free = reader.ReadBool();
			}

			if (version >= 11)
			{
				c_Murderers = (Intu)reader.ReadInt();
			}

			if (version >= 10)
			{
				c_LeaveItems = reader.ReadBool();
			}

			if (version >= 9)
			{
				c_RentToOwn = reader.ReadBool();
				c_OriginalRentTime = reader.ReadTimeSpan();
				c_RTOPayments = reader.ReadInt();
			}

			c_PreviewItems = new List<Item>();
			if (version >= 7)
			{
				int previewcount = reader.ReadInt();
				for (int i = 0; i < previewcount; ++i)
				{
					Item item = reader.ReadItem();
					c_PreviewItems.Add(item);
				}
			}

			if (version >= 6)
			{
				c_ItemsPrice = reader.ReadInt();
				c_KeepItems = reader.ReadBool();
			}

			c_DecoreItemInfos = new List<DecoreItemInfo>();
			if (version >= 5)
			{
				int decorecount = reader.ReadInt();
				DecoreItemInfo info;
				for (int i = 0; i < decorecount; ++i)
				{
					info = new DecoreItemInfo();
					info.Load(reader);
					c_DecoreItemInfos.Add(info);
				}

				Relock = reader.ReadBool();
			}

			if (version >= 4)
			{
				c_RecurRent = reader.ReadBool();
				c_RentByTime = reader.ReadTimeSpan();
				c_RentTime = reader.ReadDateTime();
				DemolishTime = reader.ReadDateTime();
				c_YoungOnly = reader.ReadBool();
				c_MinTotalSkill = reader.ReadInt();
				c_MaxTotalSkill = reader.ReadInt();
			}

			if (version >= 3)
			{
				c_MinZ = reader.ReadInt();
				c_MaxZ = reader.ReadInt();
			}

			if (version >= 2)
			{
				House = (TownHouse)reader.ReadItem();
			}

			c_Price = reader.ReadInt();
			c_Locks = reader.ReadInt();
			c_Secures = reader.ReadInt();
			c_BanLoc = reader.ReadPoint3D();
			c_SignLoc = reader.ReadPoint3D();
			c_Skill = reader.ReadString();
			c_SkillReq = reader.ReadDouble();

			Blocks = new List<Rectangle2D>();
			int count = reader.ReadInt();
			for (int i = 0; i < count; ++i)
			{
				Blocks.Add(reader.ReadRect2D());
			}

			if (c_RentTime > DateTime.UtcNow)
			{
				BeginRentTimer(c_RentTime - DateTime.UtcNow);
			}

			Timer.DelayCall(TimeSpan.Zero, StartTimers);

			ClearPreview();

			AllSigns.Add(this);
		}
	}
}
