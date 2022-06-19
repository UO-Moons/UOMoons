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

		private Point3D m_CBanLoc, m_CSignLoc;

		private int m_CLocks,
			m_CSecures,
			m_CPrice,
			m_CMinZ,
			m_CMaxZ,
			m_CMinTotalSkill,
			m_CMaxTotalSkill,
			m_CItemsPrice,
			m_CRtoPayments;

		private bool m_CYoungOnly,
			m_CRecurRent, m_CKeepItems,
			m_CLeaveItems,
			m_CRentToOwn,
			m_CFree,
			m_CForcePrivate,
			m_CForcePublic, m_CNoBanning;

		private string m_CSkill;
		private double m_CSkillReq;
		private List<DecoreItemInfo> m_CDecoreItemInfos;
		private List<Item> m_CPreviewItems;
		private Timer m_CRentTimer, m_CPreviewTimer;
		private TimeSpan m_CRentByTime, m_COriginalRentTime;
		private Intu m_CMurderers;

		public  DateTime CRentTime { get; private set; }

		public Point3D BanLoc
		{
			get => m_CBanLoc;
			set
			{
				m_CBanLoc = value;
				InvalidateProperties();
				if (Owned)
				{
					House.Region.GoLocation = value;
				}
			}
		}

		public Point3D SignLoc
		{
			get => m_CSignLoc;
			set
			{
				m_CSignLoc = value;
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
			get => m_CLocks;
			set
			{
				m_CLocks = value;
				InvalidateProperties();
				if (Owned)
				{
					House.MaxLockDowns = value;
				}
			}
		}

		public int Secures
		{
			get => m_CSecures;
			set
			{
				m_CSecures = value;
				InvalidateProperties();
				if (Owned)
				{
					House.MaxSecures = value;
				}
			}
		}

		public int Price
		{
			get => m_CPrice;
			set
			{
				m_CPrice = value;
				InvalidateProperties();
			}
		}

		public int MinZ
		{
			get => m_CMinZ;
			set
			{
				if (value > m_CMaxZ)
				{
					m_CMaxZ = value + 1;
				}

				m_CMinZ = value;
				if (Owned)
				{
					VersionCommand.UpdateRegion(this);
				}
			}
		}

		public int MaxZ
		{
			get => m_CMaxZ;
			set
			{
				if (value < m_CMinZ)
				{
					value = m_CMinZ;
				}

				m_CMaxZ = value;
				if (Owned)
				{
					VersionCommand.UpdateRegion(this);
				}
			}
		}

		public int MinTotalSkill
		{
			get => m_CMinTotalSkill;
			set
			{
				if (value > m_CMaxTotalSkill)
				{
					value = m_CMaxTotalSkill;
				}

				m_CMinTotalSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public int MaxTotalSkill
		{
			get => m_CMaxTotalSkill;
			set
			{
				if (value < m_CMinTotalSkill)
				{
					value = m_CMinTotalSkill;
				}

				m_CMaxTotalSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool YoungOnly
		{
			get => m_CYoungOnly;
			set
			{
				m_CYoungOnly = value;

				if (m_CYoungOnly)
				{
					m_CMurderers = Intu.Neither;
				}

				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public TimeSpan RentByTime
		{
			get => m_CRentByTime;
			set
			{
				m_CRentByTime = value;
				m_COriginalRentTime = value;

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
			get => m_CRecurRent;
			set
			{
				m_CRecurRent = value;

				if (!value)
				{
					m_CRentToOwn = false;
				}

				InvalidateProperties();
			}
		}

		public bool KeepItems
		{
			get => m_CKeepItems;
			set
			{
				m_CLeaveItems = false;
				m_CKeepItems = value;
				InvalidateProperties();
			}
		}

		public bool Free
		{
			get => m_CFree;
			set
			{
				m_CFree = value;
				m_CPrice = 1;
				InvalidateProperties();
			}
		}

		public Intu Murderers
		{
			get => m_CMurderers;
			set
			{
				m_CMurderers = value;

				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool ForcePrivate
		{
			get => m_CForcePrivate;
			set
			{
				m_CForcePrivate = value;

				if (value)
				{
					m_CForcePublic = false;

					if (House != null)
					{
						House.Public = false;
					}
				}
			}
		}

		public bool ForcePublic
		{
			get => m_CForcePublic;
			set
			{
				m_CForcePublic = value;

				if (value)
				{
					m_CForcePrivate = false;

					if (House != null)
					{
						House.Public = true;
					}
				}
			}
		}

		public bool NoBanning
		{
			get => m_CNoBanning;
			set
			{
				m_CNoBanning = value;

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
			get => m_CSkill;
			set
			{
				m_CSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public double SkillReq
		{
			get => m_CSkillReq;
			set
			{
				m_CSkillReq = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool LeaveItems
		{
			get => m_CLeaveItems;
			set
			{
				m_CLeaveItems = value;
				InvalidateProperties();
			}
		}

		public bool RentToOwn
		{
			get => m_CRentToOwn;
			set
			{
				m_CRentToOwn = value;
				InvalidateProperties();
			}
		}

		public bool Relock { get; set; }

		public bool NoHouseTrade { get; set; }

		public int ItemsPrice
		{
			get => m_CItemsPrice;
			set
			{
				m_CItemsPrice = value;
				InvalidateProperties();
			}
		}

		public TownHouse House { get; set; }
		protected Timer DemolishTimer { get; private set; }
		protected DateTime DemolishTime { get; private set; }
		public bool Owned => House is {Deleted: false};
		public int Floors => (m_CMaxZ - m_CMinZ) / 20 + 1;
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
				if (m_CRentByTime == TimeSpan.Zero)
				{
					return "Sale";
				}
				if (m_CRentByTime == TimeSpan.FromDays(1))
				{
					return "Daily";
				}
				if (m_CRentByTime == TimeSpan.FromDays(7))
				{
					return "Weekly";
				}
				return m_CRentByTime == TimeSpan.FromDays(30) ? "Monthly" : "Sale";
			}
		}

		public string PriceTypeShort
		{
			get
			{
				if (m_CRentByTime == TimeSpan.Zero)
				{
					return "Sale";
				}
				if (m_CRentByTime == TimeSpan.FromDays(1))
				{
					return "Day";
				}
				if (m_CRentByTime == TimeSpan.FromDays(7))
				{
					return "Week";
				}
				return m_CRentByTime == TimeSpan.FromDays(30) ? "Month" : "Sale";
			}
		}

		[Constructable]
		public TownHouseSign() : base(0xC0B)
		{
			Name = "This building is for sale or rent!";
			Movable = false;

			m_CBanLoc = Point3D.Zero;
			m_CSignLoc = Point3D.Zero;
			m_CSkill = "";
			Blocks = new List<Rectangle2D>();
			m_CDecoreItemInfos = new List<DecoreItemInfo>();
			m_CPreviewItems = new List<Item>();
			DemolishTime = DateTime.UtcNow;
			CRentTime = DateTime.UtcNow;
			m_CRentByTime = TimeSpan.Zero;
			m_CRecurRent = true;

			m_CMinZ = short.MinValue;
			m_CMaxZ = short.MaxValue;

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
				m_CPreviewItems.Add(item);
			}

			m_CPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
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

			m_CPreviewItems.Add(sign);

			//sign = new Item(0xB98) { Name = "Sign Preview", Movable = false, Location = SignLoc, Map = Map };

			int hangerId = (int)(northSouth ? SignIDs.SignHangerNS : SignIDs.SignHangerWE);
			sign = new Item(hangerId)
			{
				Name = "Sign Preview",
				Movable = false,
				Location = SignLoc,
				Map = Map
			};

			m_CPreviewItems.Add(sign);

			m_CPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ShowBanPreview()
		{
			ClearPreview();

			Item ban = new(0x17EE) { Name = "Ban Loc Preview", Movable = false, Location = BanLoc, Map = Map };

			m_CPreviewItems.Add(ban);

			m_CPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ShowFloorsPreview(Mobile m)
		{
			ClearPreview();

			Item item = new(0x7BD)
			{
				Name = "Bottom Floor Preview",
				Movable = false,
				Location = m.Location,
				Z = m_CMinZ,
				Map = Map
			};

			m_CPreviewItems.Add(item);

			item = new Item(0x7BD)
			{
				Name = "Top Floor Preview",
				Movable = false,
				Location = m.Location,
				Z = m_CMaxZ,
				Map = Map
			};

			m_CPreviewItems.Add(item);

			m_CPreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), ClearPreview);
		}

		public void ClearPreview()
		{
			foreach (Item item in new ArrayList(m_CPreviewItems))
			{
				m_CPreviewItems.Remove(item);
				item.Delete();
			}

			m_CPreviewTimer?.Stop();

			m_CPreviewTimer = null;
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

				int price = m_CPrice + (sellitems ? m_CItemsPrice : 0);

				if (m_CFree)
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

				House = new TownHouse(m, this, m_CLocks, m_CSecures);
				int signId = (int)(northSouth ? SignIDs.HouseSignNS : SignIDs.HouseSignWE);
				House.ChangeSignType(signId);

				House.Components.Resize(maxX - minX, maxY - minY);
				House.Components.Add(0x520, House.Components.Width - 1, House.Components.Height - 1, -5);

				House.Location = new Point3D(minX, minY, Map.GetAverageZ(minX, minY));
				House.Map = Map;
				House.Region.GoLocation = m_CBanLoc;
				House.Sign.Location = m_CSignLoc;
				int hangerId = (int)(northSouth ? SignIDs.SignHangerNS : SignIDs.SignHangerWE);
				House.Hanger = new Item(hangerId)
				{
					Location = m_CSignLoc,
					Map = Map,
					Movable = false
				};
				//House.Hanger = new Item(0xB98) { Location = m_CSignLoc, Map = Map, Movable = false };

				if (m_CForcePublic)
				{
					House.Public = true;
				}

				House.Price = (RentByTime == TimeSpan.FromDays(0) ? m_CPrice : 1);

				VersionCommand.UpdateRegion(this);

				if (House.Price == 0)
				{
					House.Price = 1;
				}

				if (m_CRentByTime != TimeSpan.Zero)
				{
					BeginRentTimer(m_CRentByTime);
				}

				m_CRtoPayments = 1;

				HideOtherSigns();

				m_CDecoreItemInfos = new List<DecoreItemInfo>();

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

				if (item != null && (item.ItemId is 0xB95 or 0xB96 or 0xC43 or 0xC44 || (item.ItemId > 0xBA3 && item.ItemId < 0xC0E)))
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

			foreach (Item item in items.Where(item => item is not HouseSign && item is not BaseMulti && item is not BaseAddon && item is not AddonComponent && item != House.Hanger && item.Visible && !item.IsLockedDown && !item.IsSecure && !item.Movable && !m_CPreviewItems.Contains(item)))
			{
				if (item is BaseDoor door)
				{
					ConvertDoor(door);
				}
				else if (!m_CLeaveItems)
				{
					m_CDecoreItemInfos.Add(new DecoreItemInfo(item.GetType().ToString(), item.Name, item.ItemId, item.Hue,
						item.Location, item.Map));

					if (!m_CKeepItems || !keep)
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
					ItemId = door.ItemId,
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
			foreach (DecoreItemInfo info in m_CDecoreItemInfos)
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

			if (m_CRentToOwn)
			{
				m_CRentByTime = m_COriginalRentTime;
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
			if (m_CMaxZ - m_CMinZ < 100)
			{
				floors = 1 + Math.Abs((m_CMaxZ - m_CMinZ) / 20);
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
			else if (m_CRentByTime != TimeSpan.Zero)
			{
				BeginRentTimer(m_CRentByTime);
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
			if (m_CRentTimer != null)
			{
				m_CRentTimer.Stop();
				m_CRentTimer = null;
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

			m_CRentTimer = Timer.DelayCall(time, RentDue);
			CRentTime = DateTime.UtcNow + time;
		}

		public void CheckRentTimer()
		{
			if (m_CRentTimer == null || !Owned)
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

			if (!m_CRecurRent)
			{
				House.Owner.SendMessage(
					"Your town house rental contract has expired, and the bank has once again taken possession.");
				PackUpHouse();
				return;
			}

			if (!m_CFree && House.Owner.AccessLevel == AccessLevel.Player && !Banker.Withdraw(House.Owner, m_CPrice))
			{
				House.Owner.SendMessage("Since you can not afford the rent, the bank has reclaimed your town house.");
				PackUpHouse();
				return;
			}

			if (!m_CFree)
			{
				House.Owner.SendMessage("The bank has withdrawn {0} gold rent for your town house.", m_CPrice);
			}

			OnRentPaid();

			if (m_CRentToOwn)
			{
				m_CRtoPayments++;

				bool complete = false;

				if (m_CRentByTime == TimeSpan.FromDays(1) && m_CRtoPayments >= 60)
				{
					complete = true;
					House.Price = m_CPrice * 60;
				}

				if (m_CRentByTime == TimeSpan.FromDays(7) && m_CRtoPayments >= 9)
				{
					complete = true;
					House.Price = m_CPrice * 9;
				}

				if (m_CRentByTime == TimeSpan.FromDays(30) && m_CRtoPayments >= 2)
				{
					complete = true;
					House.Price = m_CPrice * 2;
				}

				if (complete)
				{
					House.Owner.SendMessage("You now own your rental home.");
					m_CRentByTime = TimeSpan.FromDays(0);
					return;
				}
			}

			BeginRentTimer(m_CRentByTime);
		}

		protected virtual void OnRentPaid()
		{
		}

		public void NextPriceType()
		{
			if (m_CRentByTime == TimeSpan.Zero)
			{
				RentByTime = TimeSpan.FromDays(1);
			}
			else if (m_CRentByTime == TimeSpan.FromDays(1))
			{
				RentByTime = TimeSpan.FromDays(7);
			}
			else if (m_CRentByTime == TimeSpan.FromDays(7))
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
			if (m_CRentByTime == TimeSpan.Zero)
			{
				RentByTime = TimeSpan.FromDays(30);
			}
			else if (m_CRentByTime == TimeSpan.FromDays(30))
			{
				RentByTime = TimeSpan.FromDays(7);
			}
			else if (m_CRentByTime == TimeSpan.FromDays(7))
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
			if (m_CSkill != "")
			{
				try
				{
					SkillName index = (SkillName)Enum.Parse(typeof(SkillName), m_CSkill, true);
					if (m.Skills[index].Value < m_CSkillReq)
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}

			if (m_CMinTotalSkill != 0 && m.SkillsTotal / 10 < m_CMinTotalSkill)
			{
				return false;
			}

			if (m_CMaxTotalSkill != 0 && m.SkillsTotal / 10 > m_CMaxTotalSkill)
			{
				return false;
			}

			if (m_CYoungOnly && m.Player && !((PlayerMobile)m).Young)
			{
				return false;
			}

			if (m_CMurderers == Intu.Yes && m.Kills < 5)
			{
				return false;
			}

			return m_CMurderers != Intu.No || m.Kills < 5;
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

			if (m_CFree)
			{
				list.Add(1060658, "Price\tFree");
			}
			else if (m_CRentByTime == TimeSpan.Zero)
			{
				list.Add(1060658, "Price\t{0}{1}", m_CPrice, m_CKeepItems ? " (+" + m_CItemsPrice + " for the items)" : "");
			}
			else if (m_CRecurRent)
			{
				list.Add(1060658, "{0}\t{1}\r{2}", PriceType + (m_CRentToOwn ? " Rent-to-Own" : " Recurring"), m_CPrice,
					m_CKeepItems ? " (+" + m_CItemsPrice + " for the items)" : "");
			}
			else
			{
				list.Add(1060658, "One {0}\t{1}{2}", PriceTypeShort, m_CPrice,
					m_CKeepItems ? " (+" + m_CItemsPrice + " for the items)" : "");
			}

			list.Add(1060659, "Lockdowns\t{0}", m_CLocks);
			list.Add(1060660, "Secures\t{0}", m_CSecures);

			if (m_CSkillReq != 0.0)
			{
				list.Add(1060661, "Requires\t{0}", m_CSkillReq + " in " + m_CSkill);
			}
			if (m_CMinTotalSkill != 0)
			{
				list.Add(1060662, "Requires more than\t{0} total skills", m_CMinTotalSkill);
			}
			if (m_CMaxTotalSkill != 0)
			{
				list.Add(1060663, "Requires less than\t{0} total skills", m_CMaxTotalSkill);
			}

			if (m_CYoungOnly)
			{
				list.Add(1063483, "Must be\tYoung");
			}
			else
			{
				switch (m_CMurderers)
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

			writer.Write(m_CForcePrivate);
			writer.Write(m_CForcePublic);
			writer.Write(NoHouseTrade);

			// Version 12

			writer.Write(m_CFree);

			// Version 11

			writer.Write((int)m_CMurderers);

			// Version 10

			writer.Write(m_CLeaveItems);

			// Version 9
			writer.Write(m_CRentToOwn);
			writer.Write(m_COriginalRentTime);
			writer.Write(m_CRtoPayments);

			// Version 7
			//writer.WriteItemList( c_PreviewItems, true );
			writer.Write(m_CPreviewItems.Count);
			foreach (Item item in m_CPreviewItems)
			{
				writer.Write(item);
			}

			// Version 6
			writer.Write(m_CItemsPrice);
			writer.Write(m_CKeepItems);

			// Version 5
			writer.Write(m_CDecoreItemInfos.Count);
			foreach (DecoreItemInfo info in m_CDecoreItemInfos)
			{
				info.Save(writer);
			}

			writer.Write(Relock);

			// Version 4
			writer.Write(m_CRecurRent);
			writer.Write(m_CRentByTime);
			writer.Write(CRentTime);
			writer.Write(DemolishTime);
			writer.Write(m_CYoungOnly);
			writer.Write(m_CMinTotalSkill);
			writer.Write(m_CMaxTotalSkill);

			// Version 3
			writer.Write(m_CMinZ);
			writer.Write(m_CMaxZ);

			// Version 2
			writer.Write(House);

			// Version 1
			writer.Write(m_CPrice);
			writer.Write(m_CLocks);
			writer.Write(m_CSecures);
			writer.Write(m_CBanLoc);
			writer.Write(m_CSignLoc);
			writer.Write(m_CSkill);
			writer.Write(m_CSkillReq);
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
				m_CForcePrivate = reader.ReadBool();
				m_CForcePublic = reader.ReadBool();
				NoHouseTrade = reader.ReadBool();
			}

			if (version >= 12)
			{
				m_CFree = reader.ReadBool();
			}

			if (version >= 11)
			{
				m_CMurderers = (Intu)reader.ReadInt();
			}

			if (version >= 10)
			{
				m_CLeaveItems = reader.ReadBool();
			}

			if (version >= 9)
			{
				m_CRentToOwn = reader.ReadBool();
				m_COriginalRentTime = reader.ReadTimeSpan();
				m_CRtoPayments = reader.ReadInt();
			}

			m_CPreviewItems = new List<Item>();
			if (version >= 7)
			{
				var previewcount = reader.ReadInt();
				for (var i = 0; i < previewcount; ++i)
				{
					var item = reader.ReadItem();
					m_CPreviewItems.Add(item);
				}
			}

			if (version >= 6)
			{
				m_CItemsPrice = reader.ReadInt();
				m_CKeepItems = reader.ReadBool();
			}

			m_CDecoreItemInfos = new List<DecoreItemInfo>();
			if (version >= 5)
			{
				var decorecount = reader.ReadInt();
				for (var i = 0; i < decorecount; ++i)
				{
					var info = new DecoreItemInfo();
					info.Load(reader);
					m_CDecoreItemInfos.Add(info);
				}

				Relock = reader.ReadBool();
			}

			if (version >= 4)
			{
				m_CRecurRent = reader.ReadBool();
				m_CRentByTime = reader.ReadTimeSpan();
				CRentTime = reader.ReadDateTime();
				DemolishTime = reader.ReadDateTime();
				m_CYoungOnly = reader.ReadBool();
				m_CMinTotalSkill = reader.ReadInt();
				m_CMaxTotalSkill = reader.ReadInt();
			}

			if (version >= 3)
			{
				m_CMinZ = reader.ReadInt();
				m_CMaxZ = reader.ReadInt();
			}

			if (version >= 2)
			{
				House = (TownHouse)reader.ReadItem();
			}

			m_CPrice = reader.ReadInt();
			m_CLocks = reader.ReadInt();
			m_CSecures = reader.ReadInt();
			m_CBanLoc = reader.ReadPoint3D();
			m_CSignLoc = reader.ReadPoint3D();
			m_CSkill = reader.ReadString();
			m_CSkillReq = reader.ReadDouble();

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
}
