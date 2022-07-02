using Server.ContextMenus;
using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server
{
	public class Item : IHued, IComparable<Item>, ISerializable, ISpawnable
	{
		public static readonly List<Item> EmptyItems = new();
		//Default hue color for item messages
		public static readonly int DefaultDisplayColor = 0x3B2;
		[CommandProperty(AccessLevel.Counselor, true)]
		public virtual int DisplayColor => DefaultDisplayColor;

		public int CompareTo(IEntity other)
		{
			return other == null ? -1 : Serial.CompareTo(other.Serial);
		}

		public int CompareTo(Item other)
		{
			return CompareTo((IEntity)other);
		}

		public int CompareTo(object other)
		{
			if (other is null or IEntity)
				return CompareTo((IEntity)other);

			throw new ArgumentException("Bad Item IEntity");
		}

		#region Standard fields
		private Point3D _mLocation;
		private int _mItemId;
		private int _mHue;
		private int _mAmount;
		private Layer _mLayer;
		private IEntity _mParent; // Mobile, Item, or null=World
		private Map _mMap;
		private LootType _mLootType;
		private Direction _mDirection;
		private ItemRank _mItemRank;
		#endregion

		private ItemDelta _mDeltaFlags;
		private ImplFlag _mFlags;

		#region Packet caches
		private Packet _mWorldPacket;
		private Packet _mWorldPacketSa;
		private Packet _mWorldPacketHs;
		private Packet _mRemovePacket;
		private Packet _mOplPacket;
		private ObjectPropertyList _mPropertyList;
		#endregion

		public int TempFlags
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				return info?.MTempFlags ?? 0;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.MTempFlags = value;

				if (info.MTempFlags == 0)
					VerifyCompactInfo();
			}
		}

		public int SavedFlags
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				return info?.MSavedFlags ?? 0;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.MSavedFlags = value;

				if (info.MSavedFlags == 0)
					VerifyCompactInfo();
			}
		}

		/// <summary>
		/// The <see cref="Mobile" /> who is currently <see cref="Mobile.Holding">holding</see> this item.
		/// </summary>
		public Mobile HeldBy
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				return info?.MHeldBy;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.MHeldBy = value;

				if (info.MHeldBy == null)
					VerifyCompactInfo();
			}
		}
		/// <summary>
		/// The is the gridlocation for Enhanced Client.
		/// </summary>
		private byte _mGridLocation;

		[CommandProperty(AccessLevel.GameMaster)]
		public byte GridLocation
		{
			get => _mGridLocation;
			set
			{
				if (Parent is Container container)
				{
					if (value is < 0 or > 0x7C || !container.IsFreePosition(value))
					{
						_mGridLocation = container.GetNewPosition(0);
					}
					else
					{
						_mGridLocation = value;
					}
				}
				else
				{
					_mGridLocation = value;
				}
			}
		}

		[Flags]
		private enum ImplFlag : byte
		{
			Visible = 0x01,
			Movable = 0x02,
			Deleted = 0x04,
			Stackable = 0x08,
			InQueue = 0x10,
			Insured = 0x20,
			PayedInsurance = 0x40,
			QuestItem = 0x80
		}

		private class CompactInfo
		{
			public string MName;
			public List<Item> MItems;
			public BounceInfo MBounce;
			public Mobile MHeldBy;
			public Mobile MBlessedFor;
			public ISpawner MSpawner;
			public int MTempFlags;
			public int MSavedFlags;
			public double MWeight = -1;
		}

		private CompactInfo _mCompactInfo;

		public ExpandFlag GetExpandFlags()
		{
			CompactInfo info = LookupCompactInfo();

			ExpandFlag flags = 0;

			if (info == null) return flags;
			if (info.MBlessedFor != null)
				flags |= ExpandFlag.Blessed;

			if (info.MBounce != null)
				flags |= ExpandFlag.Bounce;

			if (info.MHeldBy != null)
				flags |= ExpandFlag.Holder;

			if (info.MItems != null)
				flags |= ExpandFlag.Items;

			if (info.MName != null)
				flags |= ExpandFlag.Name;

			if (info.MSpawner != null)
				flags |= ExpandFlag.Spawner;

			if (info.MSavedFlags != 0)
				flags |= ExpandFlag.SaveFlag;

			if (info.MTempFlags != 0)
				flags |= ExpandFlag.TempFlag;

			if (info.MWeight != -1)
				flags |= ExpandFlag.Weight;

			return flags;
		}

		private CompactInfo LookupCompactInfo()
		{
			return _mCompactInfo;
		}

		private CompactInfo AcquireCompactInfo()
		{
			return _mCompactInfo ??= new CompactInfo();
		}

		private void ReleaseCompactInfo()
		{
			_mCompactInfo = null;
		}

		private void VerifyCompactInfo()
		{
			var info = _mCompactInfo;

			if (info == null)
				return;

			var isValid = info.MName != null
			              || info.MItems != null
			              || info.MBounce != null
			              || info.MHeldBy != null
			              || info.MBlessedFor != null
			              || info.MSpawner != null
			              || info.MTempFlags != 0
			              || info.MSavedFlags != 0
			              || info.MWeight != -1;

			if (!isValid)
				ReleaseCompactInfo();
		}

		public List<Item> LookupItems()
		{
			if (this is Container)
				return (this as Container)?.m_Items;

			CompactInfo info = LookupCompactInfo();

			return info?.MItems;
		}

		public List<Item> AcquireItems()
		{
			if (this is Container)
			{
				Container cont = this as Container;

				if (cont is {m_Items: null})
					cont.m_Items = new List<Item>();

				if (cont != null) return cont.m_Items;
			}

			CompactInfo info = AcquireCompactInfo();

			return info.MItems ??= new List<Item>();
		}

		#region Mondain's Legacy
		public static Bitmap GetBitmap(int itemId)
		{
			try
			{
				return ArtData.GetStatic(itemId);
			}
			catch
			{
				if (Core.Debug)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("Art Data: Cannot read client files.");
					Utility.PopColor();
				}
			}

			return null;
		}

		public static void Measure(Bitmap bmp, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			ArtData.Measure(bmp, out xMin, out yMin, out xMax, out yMax);
		}

		public static Rectangle MeasureBound(Bitmap bmp)
		{
			Measure(bmp, out int xMin, out int yMin, out int xMax, out int yMax);
			return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
		}

		public static Size MeasureSize(Bitmap bmp)
		{
			Measure(bmp, out int xMin, out int yMin, out int xMax, out int yMax);
			return new Size(xMax - xMin, yMax - yMin);
		}
		#endregion

		private void SetFlag(ImplFlag flag, bool value)
		{
			if (value)
				_mFlags |= flag;
			else
				_mFlags &= ~flag;
		}

		private bool GetFlag(ImplFlag flag)
		{
			return (_mFlags & flag) != 0;
		}

		public BounceInfo GetBounce()
		{
			CompactInfo info = LookupCompactInfo();

			return info?.MBounce;
		}

		public void RecordBounce(Mobile from, Item parentstack = null)
		{
			CompactInfo info = AcquireCompactInfo();

			info.MBounce = new BounceInfo(from, this)
			{
				m_ParentStack = parentstack
			};
		}

		public void ClearBounce()
		{
			CompactInfo info = LookupCompactInfo();

			BounceInfo bounce = info?.MBounce;

			if (bounce == null) return;
			info.MBounce = null;

			switch (bounce.m_Parent)
			{
				case Item parentitem:
				{
					if (!parentitem.Deleted)
					{
						parentitem.OnItemBounceCleared(this);
					}

					break;
				}
				case Mobile parentmobile:
				{
					if (!parentmobile.Deleted)
					{
						parentmobile.OnItemBounceCleared(this);
					}

					break;
				}
			}

			VerifyCompactInfo();
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a client, <paramref name="from" />, invokes a 'help request' for the Item. Seemingly no longer functional in newer clients.
		/// </summary>
		public virtual void OnHelpRequest(Mobile from)
		{
		}

		/// <summary>
		/// Overridable. Method checked to see if the item can be traded.
		/// </summary>
		/// <returns>True if the trade is allowed, false if not.</returns>
		public virtual bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a trade has completed, either successfully or not.
		/// </summary>
		public virtual void OnSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
		}

		/// <summary>
		/// Overridable. Method checked to see if the elemental resistances of this Item conflict with another Item on the <see cref="Mobile" />.
		/// </summary>
		/// <returns>
		/// <list type="table">
		/// <item>
		/// <term>True</term>
		/// <description>There is a confliction. The elemental resistance bonuses of this Item should not be applied to the <see cref="Mobile" /></description>
		/// </item>
		/// <item>
		/// <term>False</term>
		/// <description>There is no confliction. The bonuses should be applied.</description>
		/// </item>
		/// </list>
		/// </returns>
		public virtual bool CheckPropertyConfliction(Mobile m)
		{
			return false;
		}

		/// <summary>
		/// Overridable. Sends the <see cref="PropertyList">object property list</see> to <paramref name="from" />.
		/// </summary>
		public virtual void SendPropertiesTo(Mobile from)
		{
			_ = from.Send(PropertyList);
		}

		private static readonly Regex MPluralRegEx = new(@"([^%]+)%([^%/ ]+)(/([^% ]+))*%*([^%]*)", RegexOptions.Compiled | RegexOptions.Singleline);

		public virtual void AppendClickName(StringBuilder sb)
		{
			if (Name is not {Length: > 0})
			{
				bool plural = Amount != 1;

				// bread loa%ves/f%, black pearl%s%, log%s, etc
				Match match = MPluralRegEx.Match(ItemData.Name);
				if (match.Success)
				{
					if (match.Groups[1].Value.Length > 0)
						sb.Append(match.Groups[1].Value);

					if (plural)
					{
						if (match.Groups[2].Success && match.Groups[2].Value.Length > 0)
							sb.Append(match.Groups[2].Value);
					}
					else
					{
						if (match.Groups[4].Success && match.Groups[4].Value.Length > 0)
							sb.Append(match.Groups[4].Value);
					}

					if (match.Groups[5].Value.Length > 0)
						sb.Append(match.Groups[5].Value);
				}
				else
				{
					sb.Append(ItemData.Name);
					if (plural && ItemId == 0x0EED)// gold coinS dont ever get the s (unless we put it there <--)
						sb.Append('s');
				}
			}
			else
			{
				sb.Append(Name);
				if (Amount != 1 && ItemId != 0x2006)
					sb.Append('s');
			}
		}

		public virtual void InsertNamePrefix(StringBuilder sb)
		{
			//while ( sb.Length > 0 && sb[0] == ' ' )
			//	sb.Remove( 0, 1 );

			if (Name is {Length: > 0})
				return;

			if (Amount != 1 || sb.Length <= 0 || !char.IsLetter(sb[0]) || ((ItemData.Flags & TileFlag.ArticleAn) == 0 &&
			                                                               (ItemData.Flags & TileFlag.ArticleA) == 0))
				return;
			switch (char.ToUpper(sb[0]))
			{
				case 'A':
				case 'E':
				case 'I':
				case 'O':
				case 'U':
				case 'Y':
					sb.Insert(0, "An ");
					break;
				default:
					sb.Insert(0, "A ");
					break;
			}
		}

		public virtual void InstertHtml(StringBuilder sb)
		{
			if (Parent == null) return;
			string prefix = $"<BIG><BASEFONT COLOR={GetItemRankColor()}>";
			sb.Insert(0, prefix); //big
			sb.Append("</BIG><BASEFONT COLOR=#FFFFFF>"); //big

		}

		public bool AppendLootType(StringBuilder sb)
		{
			if (!DisplayLootType || Name is {Length: > 0}) return false;
			switch (LootType)
			{
				case LootType.Blessed:
					sb.Append("Blessed");
					return true;
				case LootType.Cursed:
					sb.Append("Cursed");
					return true;
				case LootType.Regular:
					break;
				case LootType.Newbied:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return false;
		}

		public virtual string BuildSingleClick()
		{
			StringBuilder sb = new();

			if (Amount != 1 && ItemId != 0x2006)
				sb.Append($"{Amount} ");

			if (AppendLootType(sb))
				sb.Append(' ');
			AppendClickName(sb);
			InsertNamePrefix(sb);
			InstertHtml(sb);

			return sb.ToString();
		}


		public string GetItemRankColor()
		{
			string color = ItemRank switch
			{
				ItemRank.NotSet => "#FFFFFF",
				ItemRank.LowQuality => "#9D9D9D",
				ItemRank.Regular => "#FFFFFF",
				ItemRank.Crafted => "#0070FF",
				ItemRank.Resource => "#0070FF",
				ItemRank.Magic => "#1EFF00",
				ItemRank.Rare => "#A335EE",
				ItemRank.Unique => "#FF8000",
				ItemRank.Serverbirth => "#E6CC80",
				_ => "#FFFFFF"
			};
			return color;
		}

		public static string ItemRankColor(ItemRank rank)
		{
			string color = rank switch
			{
				ItemRank.NotSet => "#FFFFFF",
				ItemRank.LowQuality => "#9D9D9D",
				ItemRank.Regular => "#FFFFFF",
				ItemRank.Crafted => "#0070FF",
				ItemRank.Resource => "#0070FF",
				ItemRank.Magic => "#1EFF00",
				ItemRank.Rare => "#A335EE",
				ItemRank.Unique => "#FF8000",
				ItemRank.Serverbirth => "#E6CC80",
				_ => "#FFFFFF"
			};
			return color;
		}

		/// <summary>
		/// Overridable. Adds the name of this item to the given <see cref="ObjectPropertyList" />. This method should be overriden if the item requires a complex naming format.
		/// </summary>
		public virtual void AddNameProperty(ObjectPropertyList list)
		{
			if (Core.AOS)
			{
				var name = Name;

				if (name == null)
				{
					if (_mAmount <= 1)
						list.Add(LabelNumber);
					else
						list.Add(1050039, "{0}\t#{1}", _mAmount, LabelNumber); // ~1_NUMBER~ ~2_ITEMNAME~
				}
				else
				{
					if (_mAmount <= 1)
						list.Add(name);
					else
						list.Add(1050039, "{0}\t{1}", _mAmount, Name); // ~1_NUMBER~ ~2_ITEMNAME~
				}
			}
			else
			{
				list.Add(BuildSingleClick());
			}
		}

		/// <summary>
		/// Overridable. Adds the loot type of this item to the given <see cref="ObjectPropertyList" />. By default, this will be either 'blessed', 'cursed', or 'insured'.
		/// </summary>
		public virtual void AddLootTypeProperty(ObjectPropertyList list)
		{
			switch (_mLootType)
			{
				case LootType.Blessed:
					list.Add(1038021); // blessed
					break;
				case LootType.Cursed:
					list.Add(1049643); // cursed
					break;
				default:
				{
					if (Insured)
						list.Add(1061682); // <b>insured</b>
					break;
				}
			}
		}

		/// <summary>
		/// Overridable. Adds any elemental resistances of this item to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddResistanceProperties(ObjectPropertyList list)
		{
			int v = PhysicalResistance;

			if (v != 0)
				list.Add(1060448, v.ToString()); // physical resist ~1_val~%

			v = FireResistance;

			if (v != 0)
				list.Add(1060447, v.ToString()); // fire resist ~1_val~%

			v = ColdResistance;

			if (v != 0)
				list.Add(1060445, v.ToString()); // cold resist ~1_val~%

			v = PoisonResistance;

			if (v != 0)
				list.Add(1060449, v.ToString()); // poison resist ~1_val~%

			v = EnergyResistance;

			if (v != 0)
				list.Add(1060446, v.ToString()); // energy resist ~1_val~%
		}

		/// <summary>
		///     Overridable. Determines whether the item will show <see cref="AddWeightProperty" />.
		/// </summary>
		public virtual bool DisplayWeight
		{
			get
			{
				if (!Core.ML)
				{
					return false;
				}

				return Movable || IsLockedDown || IsSecure || ItemData.Weight != 255;
			}
		}

		/// <summary>
		/// Overridable. Adds header properties. By default, this invokes <see cref="AddNameProperty" />, <see cref="AddBlessedForProperty" /> (if applicable), and <see cref="AddLootTypeProperty" /> (if <see cref="DisplayLootType" />).
		/// </summary>
		public virtual void AddNameProperties(ObjectPropertyList list)
		{
			AddNameProperty(list);

			if (IsSecure)
			{
				AddSecureProperty(list);
			}
			else if (IsLockedDown)
			{
				AddLockedDownProperty(list);
			}

			AddCraftedProperties(list);
			AddLootTypeProperty(list);
			AddUsesRemainingProperties(list);
			AddWeightProperty(list);

			AppendChildNameProperties(list);

			//Mobile blessedFor = BlessedFor;

			//if (blessedFor != null && !blessedFor.Deleted)
			//	AddBlessedForProperty(list, blessedFor);

			//if (DisplayLootType)
			//	AddLootTypeProperty(list);

			//if (DisplayWeight)
			//	AddWeightProperty(list);

			if (QuestItem)
			{
				AddQuestItemProperty(list);
			}


			AppendChildNameProperties(list);
		}

		/// <summary>
		/// Overrideable, used to add crafted by, exceptianl, etc properties to items
		/// </summary>
		/// <param name="list"></param>
		public virtual void AddCraftedProperties(ObjectPropertyList list)
		{
		}

		/// <summary>
		/// Overrideable, used for IUsesRemaining UsesRemaining property
		/// </summary>
		/// <param name="list"></param>
		public virtual void AddUsesRemainingProperties(ObjectPropertyList list)
		{
		}

		/// <summary>
		///     Overridable. Displays cliloc 1072788-1072789.
		/// </summary>
		public virtual void AddWeightProperty(ObjectPropertyList list)
		{
			if (!DisplayWeight || !(Weight > 0)) return;
			int weight = PileWeight + TotalWeight;

			list.Add(weight == 1 ? 1072788 : 1072789, weight.ToString());
		}

		/// <summary>
		///     Overridable. Adds the "Quest Item" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddQuestItemProperty(ObjectPropertyList list)
		{
			list.Add(1072351); // Quest Item
		}

		/// <summary>
		///     Overridable. Adds the "Locked Down & Secure" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddSecureProperty(ObjectPropertyList list)
		{
			list.Add(501644); // locked down & secure
		}

		/// <summary>
		///     Overridable. Adds the "Locked Down" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddLockedDownProperty(ObjectPropertyList list)
		{
			list.Add(501643); // locked down
		}

		/// <summary>
		///     Overridable. Adds the "Blessed for ~1_NAME~" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddBlessedForProperty(ObjectPropertyList list, Mobile m)
		{
			list.Add(1062203, "{0}", m.Name); // Blessed for ~1_NAME~
		}

		public virtual void AddItemSocketProperties(ObjectPropertyList list)
		{
			if (Sockets == null) return;
			foreach (var socket in Sockets)
			{
				socket.GetProperties(list);
			}
		}

		public virtual void AddItemPowerProperties(ObjectPropertyList list)
		{
		}

		/// <summary>
		///     Overridable. Fills an <see cref="ObjectPropertyList" /> with everything applicable. By default, this invokes
		///     <see
		///         cref="AddNameProperties" />
		///     , then <see cref="Item.GetChildProperties">Item.GetChildProperties</see> or
		///     <see
		///         cref="Mobile.GetChildProperties">
		///         Mobile.GetChildProperties
		///     </see>
		///     . This method should be overriden to add any custom properties.
		/// </summary>
		public virtual void GetProperties(ObjectPropertyList list)
		{
			AddNameProperties(list);

			AddItemSocketProperties(list);

			Spawner?.GetSpawnProperties(this, list);

			AddItemPowerProperties(list);
		}

		/// <summary>
		///     Overridable. Event invoked when a child (<paramref name="item" />) is building it's <see cref="ObjectPropertyList" />. Recursively calls
		///     <see
		///         cref="Item.GetChildProperties">
		///         Item.GetChildProperties
		///     </see>
		///     or <see cref="Mobile.GetChildProperties">Mobile.GetChildProperties</see>.
		/// </summary>
		public virtual void GetChildProperties(ObjectPropertyList list, Item item)
		{
			switch (_mParent)
			{
				case Item item1:
					item1.GetChildProperties(list, item);
					break;
				case Mobile mobile:
					mobile.GetChildProperties(list, item);
					break;
			}
		}

		/// <summary>
		///     Overridable. Event invoked when a child (<paramref name="item" />) is building it's Name
		///     <see
		///         cref="ObjectPropertyList" />
		///     . Recursively calls <see cref="Item.GetChildProperties">Item.GetChildNameProperties</see> or
		///     <see
		///         cref="Mobile.GetChildProperties">
		///         Mobile.GetChildNameProperties
		///     </see>
		///     .
		/// </summary>
		public virtual void GetChildNameProperties(ObjectPropertyList list, Item item)
		{
			switch (_mParent)
			{
				case Item item1:
					item1.GetChildNameProperties(list, item);
					break;
				case Mobile mobile:
					mobile.GetChildNameProperties(list, item);
					break;
			}
		}

		public virtual bool IsChildVisibleTo(Mobile m, Item child)
		{
			return true;
		}

		public void Bounce(Mobile from)
		{
			switch (_mParent)
			{
				case Item item:
					item.RemoveItem(this);
					break;
				case Mobile mobile:
					mobile.RemoveItem(this);
					break;
			}

			_mParent = null;

			BounceInfo bounce = GetBounce();

			if (bounce != null)
			{
				var stack = bounce.m_ParentStack;

				if (stack is Item {Deleted: false} s)
				{
					if (s.IsAccessibleTo(from))
					{
						s.StackWith(from, this);
					}
				}

				var parent = bounce.m_Parent;

				switch (parent)
				{
					case Item {Deleted: false} item1:
					{
						var p = item1;
						var root = p.RootParent;

						if (p.IsAccessibleTo(from) && (root is not Mobile mobile || mobile.CheckNonlocalDrop(from, this, p)))
						{
							Location = bounce.m_Location;

							p.AddItem(this);
						}
						else
						{
							MoveToWorld(from.Location, from.Map);
						}

						break;
					}
					case Mobile {Deleted: false} mobile:
					{
						if (!mobile.EquipItem(this))
						{
							MoveToWorld(bounce.m_WorldLoc, bounce.m_Map);
						}

						break;
					}
					default:
						MoveToWorld(bounce.m_WorldLoc, bounce.m_Map);
						break;
				}

				ClearBounce();
			}
			else
			{
				MoveToWorld(from.Location, from.Map);
			}
		}

		/// <summary>
		/// Overridable. Method checked to see if this item may be equiped while casting a spell. By default, this returns false. It is overriden on spellbook and spell channeling weapons or shields.
		/// </summary>
		/// <returns>True if it may, false if not.</returns>
		/// <example>
		/// <code>
		///	public override bool AllowEquipedCast( Mobile from )
		///	{
		///		if ( from.Int &gt;= 100 )
		///			return true;
		///
		///		return base.AllowEquipedCast( from );
		/// }</code>
		///
		/// When placed in an Item script, the item may be cast when equiped if the <paramref name="from" /> has 100 or more intelligence. Otherwise, it will drop to their backpack.
		/// </example>
		public virtual bool AllowEquipedCast(Mobile from)
		{
			return false;
		}

		public virtual bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
		{
			return _mLayer == layer;
		}

		public bool IsEquipped(Mobile m)
		{
			if (m == null)
				return false;

			Item tocheck = m.FindItemOnLayer(_mLayer);
			return tocheck == this;
		}

		public virtual bool CanEquip(Mobile m)
		{
			return _mLayer != Layer.Invalid && m.FindItemOnLayer(_mLayer) == null && CheckEquip(m);
		}

		public virtual bool CheckEquip(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			if (this == m.Mount || this == m.Backpack || this == m.FindBankNoCreate())
			{
				return true;
			}

			EventSink.InvokeOnCheckEquipItem(m, this);

			/*
			if (e.Item != this || e.Item.Deleted || e.Block)
			{
				return false;
			}*/

			if (m.IsPlayer() || BlessedFor == null || BlessedFor == m) return true;
			m.SendLocalizedMessage(1153882); // You do not own that.

			return false;

		}

		public virtual void GetChildContextMenuEntries(Mobile from, List<ContextMenuEntry> list, Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.GetChildContextMenuEntries(from, list, item);
					break;
				case Mobile mob:
					mob.GetChildContextMenuEntries(from, list, item);
					break;
			}
		}

		public virtual void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.GetChildContextMenuEntries(from, list, this);
					break;
				case Mobile mob:
					mob.GetChildContextMenuEntries(from, list, this);
					break;
			}
		}

		public virtual bool VerifyMove(Mobile from)
		{
			return Movable;
		}

		public virtual DeathMoveResult OnParentDeath(Mobile parent)
		{
			if (!Movable)
				return DeathMoveResult.RemainEquiped;
			if (parent.KeepsItemsOnDeath)
				return DeathMoveResult.MoveToBackpack;
			if (CheckBlessed(parent))
				return DeathMoveResult.MoveToBackpack;
			if (CheckNewbied() && !parent.Murderer)
				return DeathMoveResult.MoveToBackpack;
			if (parent.Player && Nontransferable)
				return DeathMoveResult.MoveToBackpack;
			return DeathMoveResult.MoveToCorpse;
		}

		public virtual DeathMoveResult OnInventoryDeath(Mobile parent)
		{
			if (!Movable)
				return DeathMoveResult.MoveToBackpack;
			if (parent.KeepsItemsOnDeath)
				return DeathMoveResult.MoveToBackpack;
			if (CheckBlessed(parent))
				return DeathMoveResult.MoveToBackpack;
			if (CheckNewbied() && !parent.Murderer)
				return DeathMoveResult.MoveToBackpack;
			if (parent.Player && Nontransferable)
				return DeathMoveResult.MoveToBackpack;
			return DeathMoveResult.MoveToCorpse;
		}

		/// <summary>
		/// Moves the Item to <paramref name="location" />. The Item does not change maps.
		/// </summary>
		public virtual void MoveToWorld(Point3D location)
		{
			MoveToWorld(location, _mMap);
		}

		public void LabelTo(Mobile to, int number)
		{
			_ = to.Send(new MessageLocalized(m_Serial, _mItemId, MessageType.Label, DisplayColor, 3, number, "", ""));
		}
		
		public void LabelTo(Mobile to, int hue, int number)
		{
			_ = to.Send(new MessageLocalized(m_Serial, _mItemId, MessageType.Label, hue, 3, number, "", ""));
		}

		public void LabelTo(Mobile to, int number, string args)
		{
			_ = to.Send(new MessageLocalized(m_Serial, _mItemId, MessageType.Label, DisplayColor, 3, number, "", args));
		}

		public void LabelTo(Mobile to, int hue, int number, string args)
		{
			_ = to.Send(new MessageLocalized(m_Serial, _mItemId, MessageType.Label, hue, 3, number, "", args));
		}

		public void LabelTo(Mobile to, string text)
		{
			_ = to.Send(new UnicodeMessage(m_Serial, _mItemId, MessageType.Label, DisplayColor, 3, "ENU", "", text));
		}

		public void LabelTo(Mobile to, string format, params object[] args)
		{
			LabelTo(to, string.Format(format, args));
		}

		public void LabelToAffix(Mobile to, int number, AffixType type, string affix)
		{
			_ = to.Send(new MessageLocalizedAffix(m_Serial, _mItemId, MessageType.Label, DisplayColor, 3, number, "", type, affix, ""));
		}

		public void LabelToAffix(Mobile to, int hue, int number, AffixType type, string affix)
		{
			_ = to.Send(new MessageLocalizedAffix(m_Serial, _mItemId, MessageType.Label, hue, 3, number, "", type, affix, ""));
		}

		public void LabelToAffix(Mobile to, int number, AffixType type, string affix, string args)
		{
			_ = to.Send(new MessageLocalizedAffix(m_Serial, _mItemId, MessageType.Label, DisplayColor, 3, number, "", type, affix, args));
		}

		public void LabelToAffix(Mobile to, int hue, int number, AffixType type, string affix, string args)
		{
			_ = to.Send(new MessageLocalizedAffix(m_Serial, _mItemId, MessageType.Label, hue, 3, number, "", type, affix, args));
		}

		public virtual void LabelLootTypeTo(Mobile to)
		{
			switch (_mLootType)
			{
				case LootType.Blessed:
					LabelTo(to, 1041362); // (blessed)
					break;
				case LootType.Cursed:
					LabelTo(to, "(cursed)");
					break;
			}
		}

		public bool AtWorldPoint(int x, int y)
		{
			return _mParent == null && _mLocation.m_X == x && _mLocation.m_Y == y;
		}

		public bool AtPoint(int x, int y)
		{
			return _mLocation.m_X == x && _mLocation.m_Y == y;
		}

		/// <summary>
		/// Moves the Item to a given <paramref name="location" /> and <paramref name="map" />.
		/// </summary>
		public void MoveToWorld(Point3D location, Map map)
		{
			if (Deleted)
				return;

			Point3D oldLocation = GetWorldLocation();
			Point3D oldRealLocation = _mLocation;

			SetLastMoved();

			switch (Parent)
			{
				case Mobile mobile:
					mobile.RemoveItem(this);
					break;
				case Item item:
					item.RemoveItem(this);
					break;
			}

			if (_mMap != map)
			{
				Map old = _mMap;

				if (_mMap != null)
				{
					_mMap.OnLeave(this);

					if (oldLocation.m_X != 0)
					{
						IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(oldLocation, GetMaxUpdateRange());

						foreach (NetState state in eable)
						{
							Mobile m = state.Mobile;

							if (m.InRange(oldLocation, GetUpdateRange(m)))
							{
								state.Send(RemovePacket);
							}
						}

						eable.Free();
					}
				}

				_mLocation = location;
				OnLocationChange(oldRealLocation);

				ReleaseWorldPackets();

				List<Item> items = LookupItems();

				if (items != null)
				{
					for (int i = 0; i < items.Count; ++i)
						items[i].Map = map;
				}

				_mMap = map;

				_mMap?.OnEnter(this);

				OnMapChange();

				if (_mMap != null)
				{
					IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation, GetMaxUpdateRange());

					foreach (NetState state in eable)
					{
						Mobile m = state.Mobile;

						if (m.CanSee(this) && m.InRange(_mLocation, GetUpdateRange(m)))
							SendInfoTo(state);
					}

					eable.Free();
				}

				RemDelta(ItemDelta.Update);

				if (old == null || old == Map.Internal)
					InvalidateProperties();
			}
			else if (_mMap != null)
			{
				IPooledEnumerable<NetState> eable;

				if (oldLocation.m_X != 0)
				{
					eable = _mMap.GetClientsInRange(oldLocation, GetMaxUpdateRange());

					foreach (NetState state in eable)
					{
						Mobile m = state.Mobile;

						if (!m.InRange(location, GetUpdateRange(m)))
						{
							state.Send(RemovePacket);
						}
					}

					eable.Free();
				}

				Point3D oldInternalLocation = _mLocation;

				_mLocation = location;
				OnLocationChange(oldRealLocation);

				ReleaseWorldPackets();

				eable = _mMap.GetClientsInRange(_mLocation, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (m.CanSee(this) && m.InRange(_mLocation, GetUpdateRange(m)))
						SendInfoTo(state);
				}

				eable.Free();

				_mMap.OnMove(oldInternalLocation, this);

				RemDelta(ItemDelta.Update);
			}
			else
			{
				Map = map;
				Location = location;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HonestyItem { get; set; }

		/// <summary>
		/// Has the item been deleted?
		/// </summary>
		public bool Deleted => GetFlag(ImplFlag.Deleted);

		[CommandProperty(AccessLevel.GameMaster)]
		public ItemRank ItemRank
		{
			get => _mItemRank;
			set
			{
				if (_mItemRank != value)
				{
					_mItemRank = value;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public LootType LootType
		{
			get => _mLootType;
			set
			{
				if (_mLootType == value) return;
				_mLootType = value;

				if (DisplayLootType)
					InvalidateProperties();
			}
		}

		/// <summary>
		///		If true the item should be considered an artifact
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool IsArtifact => this is IArtifact artifact && artifact.ArtifactRarity > 0;

		public static TimeSpan DefaultDecayTime { get; set; } = TimeSpan.FromMinutes(Settings.Configuration.Get<int>("Items", "DefaultDecayTime", 60));

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int DecayMultiplier => 1;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool DefaultDecaySetting => true;

		[CommandProperty(AccessLevel.Decorator)]
		public virtual TimeSpan DecayTime => TimeSpan.FromMinutes(DefaultDecayTime.TotalMinutes * DecayMultiplier);

		[CommandProperty(AccessLevel.Decorator)]
		public virtual bool Decays =>
				// TODO: Make item decay an option on the spawner
				DefaultDecaySetting && Movable && Visible && !HonestyItem/* && Spawner == null*/;

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan TimeToDecay => TimeSpan.FromMinutes((DecayTime - (DateTime.UtcNow - LastMoved)).TotalMinutes);

		public virtual bool OnDecay()
		{
			return Decays && Parent == null && Map != Map.Internal && Region.Find(Location, Map).OnDecay(this);
		}

		public void SetLastMoved()
		{
			LastMoved = DateTime.UtcNow;
		}

		public DateTime LastMoved { get; set; }

		public virtual bool StackIgnoreItemId => false;
		public virtual bool StackIgnoreHue => false;
		public virtual bool StackIgnoreName => false;

		public bool StackWith(Mobile from, Item dropped)
		{
			return StackWith(from, dropped, true);
		}

		public virtual bool StackWith(Mobile from, Item dropped, bool playSound)
		{
			if (WillStack(from, dropped))
			{
				if (_mLootType != dropped._mLootType)
				{
					_mLootType = LootType.Regular;
				}

				Amount += dropped.Amount;
				dropped.Delete();

				if (!playSound || from == null) return true;
				int soundId = GetDropSound();

				if (soundId == -1)
				{
					soundId = 0x42;
				}

				from.SendSound(soundId, GetWorldLocation());

				return true;
			}

			return false;
		}

		public virtual bool WillStack(Mobile from, Item item)
		{
			if (item == this || item.GetType() != GetType())
			{
				return false;
			}

			if (!item.Stackable || !Stackable)
			{
				return false;
			}

			if (item.Nontransferable || Nontransferable)
			{
				return false;
			}

			if ((!item.StackIgnoreItemId || !StackIgnoreItemId) && item.ItemId != ItemId)
			{
				return false;
			}

			if ((!item.StackIgnoreHue || !StackIgnoreHue) && item.Hue != Hue)
			{
				return false;
			}

			if ((!item.StackIgnoreName || !StackIgnoreName) && item.Name != Name)
			{
				return false;
			}

			if (item.Amount + Amount > 60000)
			{
				return false;
			}

			if ((Sockets == null && item.Sockets != null) || (Sockets != null && item.Sockets == null))
			{
				return false;
			}

			if (Sockets == null || item.Sockets == null) return true;
			if (Sockets.Any(s => !item.HasSocket(s.GetType())))
			{
				return false;
			}

			return item.Sockets.All(s => HasSocket(s.GetType()));
		}

		public virtual bool OnDragDrop(Mobile from, Item dropped)
		{
			if (Parent is Container container)
				return container.OnStackAttempt(from, this, dropped);

			return StackWith(from, dropped);
		}

		public Rectangle2D GetGraphicBounds()
		{
			int itemID = _mItemId;
			bool doubled = _mAmount > 1;

			if (itemID >= 0xEEA && itemID <= 0xEF2) // Are we coins?
			{
				int coinBase = (itemID - 0xEEA) / 3;
				coinBase *= 3;
				coinBase += 0xEEA;

				doubled = false;

				itemID = _mAmount switch
				{
					<= 1 => coinBase,
					<= 5 => coinBase + 1,
					_ => coinBase + 2
				};
			}

			Rectangle2D bounds = ItemBounds.Table[itemID & 0x3FFF];

			if (doubled)
			{
				bounds.Set(bounds.X, bounds.Y, bounds.Width + 5, bounds.Height + 5);
			}

			return bounds;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Stackable
		{
			get => GetFlag(ImplFlag.Stackable);
			set => SetFlag(ImplFlag.Stackable, value);
		}

		private readonly object _rpl = new();

		public Packet RemovePacket
		{
			get
			{
				if (_mRemovePacket != null)
				{
					lock (_rpl)
					{
						if (_mRemovePacket != null) return _mRemovePacket;
						_mRemovePacket = new RemoveItem(this);
						_mRemovePacket.SetStatic();
					}
				}

				return _mRemovePacket;
			}
		}

		private readonly object _opll = new();
		public Packet OplPacket
		{
			get
			{
				if (_mOplPacket == null)
				{
					lock (_opll)
					{
						if (_mOplPacket != null) return _mOplPacket;
						_mOplPacket = new OplInfo(PropertyList);
						_mOplPacket.SetStatic();
					}
				}

				return _mOplPacket;
			}
		}

		public ObjectPropertyList PropertyList
		{
			get
			{
				if (_mPropertyList != null) return _mPropertyList;
				_mPropertyList = new ObjectPropertyList(this);

				GetProperties(_mPropertyList);
				AppendChildProperties(_mPropertyList);

				_mPropertyList.Terminate();
				_mPropertyList.SetStatic();

				return _mPropertyList;
			}
		}

		public virtual void AppendChildProperties(ObjectPropertyList list)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.GetChildProperties(list, this);
					break;
				case Mobile parentMob:
					parentMob.GetChildProperties(list, this);
					break;
			}
		}

		public virtual void AppendChildNameProperties(ObjectPropertyList list)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.GetChildNameProperties(list, this);
					break;
				case Mobile parentMob:
					parentMob.GetChildNameProperties(list, this);
					break;
			}
		}

		public void ClearProperties()
		{
			Packet.Release(ref _mPropertyList);
			Packet.Release(ref _mOplPacket);
		}

		public void InvalidateProperties()
		{
			if (!ObjectPropertyList.Enabled)
				return;

			if (_mMap != null && _mMap != Map.Internal && !World.Loading)
			{
				ObjectPropertyList oldList = _mPropertyList;
				_mPropertyList = null;
				ObjectPropertyList newList = PropertyList;

				if (oldList != null && oldList.Hash == newList.Hash) return;
				Packet.Release(ref _mOplPacket);
				Delta(ItemDelta.Properties);
			}
			else
			{
				ClearProperties();
			}
		}

		private readonly object _wpl = new();
		private readonly object _wplsa = new();
		private readonly object _wplhs = new();

		public Packet WorldPacket
		{
			get
			{
				// This needs to be invalidated when any of the following changes:
				//  - ItemID
				//  - Amount
				//  - Location
				//  - Hue
				//  - Packet Flags
				//  - Direction

				if (_mWorldPacket == null)
				{
					lock (_wpl)
					{
						if (_mWorldPacket != null) return _mWorldPacket;
						_mWorldPacket = new WorldItem(this);
						_mWorldPacket.SetStatic();
					}
				}

				return _mWorldPacket;
			}
		}

		public Packet WorldPacketSa
		{
			get
			{
				// This needs to be invalidated when any of the following changes:
				//  - ItemID
				//  - Amount
				//  - Location
				//  - Hue
				//  - Packet Flags
				//  - Direction

				if (_mWorldPacketSa == null)
				{
					lock (_wplsa)
					{
						if (_mWorldPacketSa != null) return _mWorldPacketSa;
						_mWorldPacketSa = new WorldItemSA(this);
						_mWorldPacketSa.SetStatic();
					}
				}

				return _mWorldPacketSa;
			}
		}

		public Packet WorldPacketHs
		{
			get
			{
				// This needs to be invalidated when any of the following changes:
				//  - ItemID
				//  - Amount
				//  - Location
				//  - Hue
				//  - Packet Flags
				//  - Direction

				if (_mWorldPacketHs == null)
				{
					lock (_wplhs)
					{
						if (_mWorldPacketHs == null)
						{
							_mWorldPacketHs = new WorldItemHS(this);
							_mWorldPacketHs.SetStatic();
						}
					}
				}

				return _mWorldPacketHs;
			}
		}

		public void ReleaseWorldPackets()
		{
			Packet.Release(ref _mWorldPacket);
			Packet.Release(ref _mWorldPacketSa);
			Packet.Release(ref _mWorldPacketHs);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Visible
		{
			get => GetFlag(ImplFlag.Visible);
			set
			{
				if (GetFlag(ImplFlag.Visible) != value)
				{
					SetFlag(ImplFlag.Visible, value);
					ReleaseWorldPackets();

					if (_mMap != null)
					{
						Point3D worldLoc = GetWorldLocation();

						IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(worldLoc, GetMaxUpdateRange());

						foreach (NetState state in eable)
						{
							Mobile m = state.Mobile;

							if (!m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
							{
								state.Send(RemovePacket);
							}
						}

						eable.Free();
					}

					Delta(ItemDelta.Update);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Movable
		{
			get => GetFlag(ImplFlag.Movable);
			set
			{
				if (GetFlag(ImplFlag.Movable) == value) return;
				SetFlag(ImplFlag.Movable, value);
				ReleaseWorldPackets();
				Delta(ItemDelta.Update);
			}
		}

		public virtual bool ForceShowProperties => false;

		public virtual int GetPacketFlags()
		{
			int flags = 0;

			if (!Visible)
				flags |= 0x80;

			if (Movable || ForceShowProperties)
				flags |= 0x20;

			return flags;
		}

		public virtual void OnEnterLocation(Mobile m)
		{
		}

		public virtual void OnLeaveLocation(Mobile m)
		{
		}

		public virtual bool OnMoveOff(Mobile m)
		{
			return true;
		}

		public virtual bool OnMoveOver(Mobile m)
		{
			return true;
		}

		public virtual bool HandlesOnMovement => false;

		public virtual void OnMovement(Mobile m, Point3D oldLocation)
		{
		}

		public void Internalize()
		{
			MoveToWorld(Point3D.Zero, Map.Internal);
		}

		public virtual void OnMapChange()
		{
		}

		public virtual void OnRemoved(IEntity parent)
		{
		}

		public virtual void OnAdded(IEntity parent)
		{
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Map Map
		{
			get => _mMap;
			set
			{
				if (_mMap != value)
				{
					Map old = _mMap;

					if (_mMap != null && _mParent == null)
					{
						_mMap.OnLeave(this);
						SendRemovePacket();
					}

					List<Item> items = LookupItems();

					if (items != null)
					{
						for (int i = 0; i < items.Count; ++i)
							items[i].Map = value;
					}

					_mMap = value;

					if (_mMap != null && _mParent == null)
						_mMap.OnEnter(this);

					Delta(ItemDelta.Update);

					OnMapChange();

					if (old == null || old == Map.Internal)
						InvalidateProperties();
				}
			}
		}

		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Direction = 0x00000001,
			Bounce = 0x00000002,
			LootType = 0x00000004,
			LocationFull = 0x00000008,
			ItemId = 0x00000010,
			Hue = 0x00000020,
			Amount = 0x00000040,
			Layer = 0x00000080,
			Name = 0x00000100,
			Parent = 0x00000200,
			Items = 0x00000400,
			WeightNot1Or0 = 0x00000800,
			Map = 0x00001000,
			Visible = 0x00002000,
			Movable = 0x00004000,
			Stackable = 0x00008000,
			WeightIs0 = 0x00010000,
			LocationSByteZ = 0x00020000,
			LocationShortXy = 0x00040000,
			LocationByteXy = 0x00080000,
			ImplFlags = 0x00100000,
			InsuredFor = 0x00200000,
			BlessedFor = 0x00400000,
			HeldBy = 0x00800000,
			IntWeight = 0x01000000,
			SavedFlags = 0x02000000,
			NullWeight = 0x04000000,
			ItemRank = 0x06000000
		}

		int ISerializable.TypeReference => MTypeRef;

		int ISerializable.SerialIdentity => m_Serial;

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			SaveFlag flags = SaveFlag.None;

			int x = _mLocation.m_X, y = _mLocation.m_Y, z = _mLocation.m_Z;

			if (x != 0 || y != 0 || z != 0)
			{
				if (x >= short.MinValue && x <= short.MaxValue && y >= short.MinValue && y <= short.MaxValue && z >= sbyte.MinValue && z <= sbyte.MaxValue)
				{
					if (x != 0 || y != 0)
					{
						if (x >= byte.MinValue && x <= byte.MaxValue && y >= byte.MinValue && y <= byte.MaxValue)
							flags |= SaveFlag.LocationByteXy;
						else
							flags |= SaveFlag.LocationShortXy;
					}

					if (z != 0)
						flags |= SaveFlag.LocationSByteZ;
				}
				else
				{
					flags |= SaveFlag.LocationFull;
				}
			}

			CompactInfo info = LookupCompactInfo();
			List<Item> items = LookupItems();

			if (_mDirection != Direction.North)
				flags |= SaveFlag.Direction;
			if (_mItemRank != ItemRank.NotSet)
				flags |= SaveFlag.ItemRank;
			if (info is {MBounce: { }})
				flags |= SaveFlag.Bounce;
			if (_mLootType != LootType.Regular)
				flags |= SaveFlag.LootType;
			if (_mItemId != 0)
				flags |= SaveFlag.ItemId;
			if (_mHue != 0)
				flags |= SaveFlag.Hue;
			if (_mAmount != 1)
				flags |= SaveFlag.Amount;
			if (_mLayer != Layer.Invalid)
				flags |= SaveFlag.Layer;
			if (info is {MName: { }})
				flags |= SaveFlag.Name;
			if (_mParent != null)
				flags |= SaveFlag.Parent;
			if (items is {Count: > 0})
				flags |= SaveFlag.Items;
			if (_mMap != Map.Internal)
				flags |= SaveFlag.Map;
			//if ( m_InsuredFor != null && !m_InsuredFor.Deleted )
			//flags |= SaveFlag.InsuredFor;
			if (info is {MBlessedFor.Deleted: false})
				flags |= SaveFlag.BlessedFor;
			if (info is {MHeldBy.Deleted: false})
				flags |= SaveFlag.HeldBy;
			if (info != null && info.MSavedFlags != 0)
				flags |= SaveFlag.SavedFlags;

			if (info == null || info.MWeight == -1)
			{
				flags |= SaveFlag.NullWeight;
			}
			else
			{
				if (info.MWeight == 0.0)
				{
					flags |= SaveFlag.WeightIs0;
				}
				else if (info.MWeight != 1.0)
				{
					if (info.MWeight == (int)info.MWeight)
						flags |= SaveFlag.IntWeight;
					else
						flags |= SaveFlag.WeightNot1Or0;
				}
			}

			ImplFlag implFlags = (_mFlags & (ImplFlag.Visible | ImplFlag.Movable | ImplFlag.Stackable | ImplFlag.Insured | ImplFlag.PayedInsurance | ImplFlag.QuestItem));

			if (implFlags != (ImplFlag.Visible | ImplFlag.Movable))
				flags |= SaveFlag.ImplFlags;

			writer.Write((int)flags);

			/* begin last moved time optimization */
			long ticks = LastMoved.Ticks;
			long now = DateTime.UtcNow.Ticks;

			TimeSpan d;

			try { d = new TimeSpan(ticks - now); }
			catch
			{
				d = ticks < now ? TimeSpan.MaxValue : TimeSpan.MaxValue;
			}

			double minutes = -d.TotalMinutes;

			minutes = minutes switch
			{
				< int.MinValue => int.MinValue,
				> int.MaxValue => int.MaxValue,
				_ => minutes
			};

			writer.WriteEncodedInt((int)minutes);
			/* end */

			if (flags.HasFlag(SaveFlag.Direction))
				writer.Write((byte)_mDirection);

			if (flags.HasFlag(SaveFlag.ItemRank))
				writer.Write((byte)_mItemRank);

			if (flags.HasFlag(SaveFlag.Bounce))
				if (info != null)
					BounceInfo.Serialize(info.MBounce, writer);

			if (flags.HasFlag(SaveFlag.LootType))
				writer.Write((byte)_mLootType);

			if (flags.HasFlag(SaveFlag.LocationFull))
			{
				writer.WriteEncodedInt(x);
				writer.WriteEncodedInt(y);
				writer.WriteEncodedInt(z);
			}
			else
			{
				if (flags.HasFlag(SaveFlag.LocationByteXy))
				{
					writer.Write((byte)x);
					writer.Write((byte)y);
				}
				else if (flags.HasFlag(SaveFlag.LocationShortXy))
				{
					writer.Write((short)x);
					writer.Write((short)y);
				}

				if (flags.HasFlag(SaveFlag.LocationSByteZ))
					writer.Write((sbyte)z);
			}

			if (flags.HasFlag(SaveFlag.ItemId))
				writer.WriteEncodedInt(_mItemId);

			if (flags.HasFlag(SaveFlag.Hue))
				writer.WriteEncodedInt(_mHue);

			if (flags.HasFlag(SaveFlag.Amount))
				writer.WriteEncodedInt(_mAmount);

			if (flags.HasFlag(SaveFlag.Layer))
				writer.Write((byte)_mLayer);

			if (flags.HasFlag(SaveFlag.Name))
				writer.Write(info.MName);

			if (flags.HasFlag(SaveFlag.Parent))
			{
				if (_mParent is {Deleted: false})
					writer.Write(_mParent.Serial);
				else
					writer.Write(Serial.MinusOne);
			}

			if (flags.HasFlag(SaveFlag.Items))
				writer.Write(items, false);

			if (flags.HasFlag(SaveFlag.IntWeight))
				writer.WriteEncodedInt((int)info.MWeight);
			else if (flags.HasFlag(SaveFlag.WeightNot1Or0))
				writer.Write(info.MWeight);

			if (flags.HasFlag(SaveFlag.Map))
				writer.Write(_mMap);

			if (flags.HasFlag(SaveFlag.ImplFlags))
				writer.WriteEncodedInt((int)implFlags);

			if (flags.HasFlag(SaveFlag.InsuredFor))
				writer.Write((Mobile)null);

			if (flags.HasFlag(SaveFlag.BlessedFor))
				writer.Write(info.MBlessedFor);

			if (flags.HasFlag(SaveFlag.HeldBy))
				writer.Write(info.MHeldBy);

			if (flags.HasFlag(SaveFlag.SavedFlags))
				writer.WriteEncodedInt(info.MSavedFlags);
		}

		public IPooledEnumerable<IEntity> GetObjectsInRange(int range)
		{
			Map map = _mMap;

			if (map == null)
				return Map.NullEnumerable<IEntity>.Instance;

			return map.GetObjectsInRange(_mParent == null ? _mLocation : GetWorldLocation(), range);
		}

		public IPooledEnumerable<Item> GetItemsInRange(int range)
		{
			Map map = _mMap;

			if (map == null)
				return Map.NullEnumerable<Item>.Instance;

			return map.GetItemsInRange(_mParent == null ? _mLocation : GetWorldLocation(), range);
		}

		public IPooledEnumerable<Mobile> GetMobilesInRange(int range)
		{
			Map map = _mMap;

			if (map == null)
				return Map.NullEnumerable<Mobile>.Instance;

			return map.GetMobilesInRange(_mParent == null ? _mLocation : GetWorldLocation(), range);
		}

		public IPooledEnumerable<NetState> GetClientsInRange(int range)
		{
			Map map = _mMap;

			if (map == null)
				return Map.NullEnumerable<NetState>.Instance;

			return map.GetClientsInRange(_mParent == null ? _mLocation : GetWorldLocation(), range);
		}

		public static int LockedDownFlag { get; set; }

		public static int SecureFlag { get; set; }

		public bool IsLockedDown
		{
			get => GetTempFlag(LockedDownFlag);
			set { SetTempFlag(LockedDownFlag, value); InvalidateProperties(); }
		}

		public bool IsSecure
		{
			get => GetTempFlag(SecureFlag);
			set { SetTempFlag(SecureFlag, value); InvalidateProperties(); }
		}

		public bool GetTempFlag(int flag)
		{
			CompactInfo info = LookupCompactInfo();

			if (info == null)
				return false;

			return (info.MTempFlags & flag) != 0;
		}

		public void SetTempFlag(int flag, bool value)
		{
			CompactInfo info = AcquireCompactInfo();

			if (value)
				info.MTempFlags |= flag;
			else
				info.MTempFlags &= ~flag;

			if (info.MTempFlags == 0)
				VerifyCompactInfo();
		}

		public bool GetSavedFlag(int flag)
		{
			CompactInfo info = LookupCompactInfo();

			if (info == null)
				return false;

			return (info.MSavedFlags & flag) != 0;
		}

		public void SetSavedFlag(int flag, bool value)
		{
			CompactInfo info = AcquireCompactInfo();

			if (value)
				info.MSavedFlags |= flag;
			else
				info.MSavedFlags &= ~flag;

			if (info.MSavedFlags == 0)
				VerifyCompactInfo();
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			SetLastMoved();

			switch (version)
			{
				case 0:
					{
						SaveFlag flags = (SaveFlag)reader.ReadInt();

						int minutes = reader.ReadEncodedInt();

						try { LastMoved = DateTime.UtcNow - TimeSpan.FromMinutes(minutes); }
						catch { LastMoved = DateTime.UtcNow; }

						if (flags.HasFlag(SaveFlag.Direction))
							_mDirection = (Direction)reader.ReadByte();

						if (flags.HasFlag(SaveFlag.ItemRank))
							_mItemRank = (ItemRank)reader.ReadByte();

						if (flags.HasFlag(SaveFlag.Bounce))
							AcquireCompactInfo().MBounce = BounceInfo.Deserialize(reader);

						if (flags.HasFlag(SaveFlag.LootType))
							_mLootType = (LootType)reader.ReadByte();

						int x = 0, y = 0, z = 0;

						if (flags.HasFlag(SaveFlag.LocationFull))
						{
							x = reader.ReadEncodedInt();
							y = reader.ReadEncodedInt();
							z = reader.ReadEncodedInt();
						}
						else
						{
							if (flags.HasFlag(SaveFlag.LocationByteXy))
							{
								x = reader.ReadByte();
								y = reader.ReadByte();
							}
							else if (flags.HasFlag(SaveFlag.LocationShortXy))
							{
								x = reader.ReadShort();
								y = reader.ReadShort();
							}

							if (flags.HasFlag(SaveFlag.LocationSByteZ))
								z = reader.ReadSByte();
						}

						_mLocation = new Point3D(x, y, z);

						if (flags.HasFlag(SaveFlag.ItemId))
							_mItemId = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Hue))
							_mHue = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Amount))
							_mAmount = reader.ReadEncodedInt();
						else
							_mAmount = 1;

						if (flags.HasFlag(SaveFlag.Layer))
							_mLayer = (Layer)reader.ReadByte();

						if (flags.HasFlag(SaveFlag.Name))
						{
							string name = reader.ReadString();

							if (name != DefaultName)
								AcquireCompactInfo().MName = name;
						}

						if (flags.HasFlag(SaveFlag.Parent))
						{
							var parent = reader.ReadSerial();

							if (parent.IsMobile)
								_mParent = World.FindMobile(parent);
							else if (parent.IsItem)
								_mParent = World.FindItem(parent);
							else
								_mParent = null;

							if (_mParent == null && (parent.IsMobile || parent.IsItem))
								Delete();
						}

						if (flags.HasFlag(SaveFlag.Items))
						{
							List<Item> items = reader.ReadStrongItemList();

							if (this is Container)
								((Container) this).m_Items = items;
							else
								AcquireCompactInfo().MItems = items;
						}

						if (!flags.HasFlag(SaveFlag.NullWeight))
						{
							double weight;

							if (flags.HasFlag(SaveFlag.IntWeight))
								weight = reader.ReadEncodedInt();
							else if (flags.HasFlag(SaveFlag.WeightNot1Or0))
								weight = reader.ReadDouble();
							else if (flags.HasFlag(SaveFlag.WeightIs0))
								weight = 0.0;
							else
								weight = 1.0;

							if (weight != DefaultWeight)
								AcquireCompactInfo().MWeight = weight;
						}

						if (flags.HasFlag(SaveFlag.Map))
							_mMap = reader.ReadMap();
						else
							_mMap = Map.Internal;

						if (flags.HasFlag(SaveFlag.Visible))
							SetFlag(ImplFlag.Visible, reader.ReadBool());
						else
							SetFlag(ImplFlag.Visible, true);

						if (flags.HasFlag(SaveFlag.Movable))
							SetFlag(ImplFlag.Movable, reader.ReadBool());
						else
							SetFlag(ImplFlag.Movable, true);

						if (flags.HasFlag(SaveFlag.Stackable))
							SetFlag(ImplFlag.Stackable, reader.ReadBool());

						if (flags.HasFlag(SaveFlag.ImplFlags))
							_mFlags = (ImplFlag)reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.InsuredFor))
							/*m_InsuredFor = */
							_ = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.BlessedFor))
							AcquireCompactInfo().MBlessedFor = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.HeldBy))
							AcquireCompactInfo().MHeldBy = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.SavedFlags))
							AcquireCompactInfo().MSavedFlags = reader.ReadEncodedInt();

						if (_mMap != null && _mParent == null)
							_mMap.OnEnter(this);

						break;
					}
			}

			if (HeldBy != null)
				_ = Timer.DelayCall(TimeSpan.Zero, FixHolding_Sandbox);

			//if ( version < 9 )
			VerifyCompactInfo();
		}

		private void FixHolding_Sandbox()
		{
			Mobile heldBy = HeldBy;

			if (heldBy == null) return;
			if (GetBounce() != null)
			{
				Bounce(heldBy);
			}
			else
			{
				heldBy.Holding = null;
				_ = heldBy.AddToBackpack(this);
				ClearBounce();
			}
		}

		public virtual int GetMaxUpdateRange()
		{
			return Map.GlobalUpdateRange;
		}

		public virtual int GetUpdateRange(Mobile m)
		{
			return Map.GlobalUpdateRange;
		}

		public void SendInfoTo(NetState state)
		{
			SendInfoTo(state, ObjectPropertyList.Enabled);
		}

		public virtual void SendInfoTo(NetState state, bool sendOplPacket)
		{
			state.Send(GetWorldPacketFor(state));

			if (sendOplPacket)
			{
				state.Send(OplPacket);
			}
		}

		protected virtual Packet GetWorldPacketFor(NetState state)
		{
			if (state.HighSeas)
				return WorldPacketHs;
			if (state.StygianAbyss)
				return WorldPacketSa;
			return WorldPacket;
		}

		public virtual bool IsVirtualItem => false;

		public virtual int GetTotal(TotalType type)
		{
			return 0;
		}

		public virtual void UpdateTotal(Item sender, TotalType type, int delta)
		{
			if (IsVirtualItem) return;
			switch (_mParent)
			{
				case Item item:
					item.UpdateTotal(sender, type, delta);
					break;
				case Mobile mobile:
					mobile.UpdateTotal(sender, type, delta);
					break;
				default:
					HeldBy?.UpdateTotal(sender, type, delta);
					break;
			}
		}

		public virtual void UpdateTotals()
		{
		}

		public virtual int LabelNumber
		{
			get
			{
				if (_mItemId < 0x4000)
					return 1020000 + _mItemId;
				return 1078872 + _mItemId;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalGold => GetTotal(TotalType.Gold);

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalItems => GetTotal(TotalType.Items);

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalWeight => GetTotal(TotalType.Weight);

		public virtual double DefaultWeight
		{
			get
			{
				if (_mItemId < 0 || _mItemId > TileData.MaxItemValue || this is BaseMulti)
					return 0;

				int weight = TileData.ItemTable[_mItemId].Weight;

				if (weight == 255 || weight == 0)
					weight = 1;

				return weight;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public double Weight
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				if (info != null && info.MWeight != -1)
					return info.MWeight;

				return DefaultWeight;
			}
			set
			{
				if (Weight == value) return;
				CompactInfo info = AcquireCompactInfo();

				int oldPileWeight = PileWeight;

				info.MWeight = value;

				if (info.MWeight == -1)
					VerifyCompactInfo();

				int newPileWeight = PileWeight;

				UpdateTotal(this, TotalType.Weight, newPileWeight - oldPileWeight);

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int PileWeight => (int)Math.Ceiling(Weight * Amount);

		public virtual int HuedItemID => _mItemId;

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public virtual int Hue
		{
			get => _mHue;
			set
			{
				if (_mHue == value) return;
				_mHue = value;
				ReleaseWorldPackets();

				Delta(ItemDelta.Update);
			}
		}

		public virtual bool HiddenQuestItemHue { get; set; }

		public int QuestItemHue => HiddenQuestItemHue ? Hue : 0x04EA;

		public virtual bool Nontransferable => QuestItem;

		public virtual void HandleInvalidTransfer(Mobile from)
		{
			// OSI sends 1074769, bug!
			if (QuestItem)
				from.SendLocalizedMessage(1049343); // You can only drop quest items into the top-most level of your backpack while you still need them for your quest.
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual Layer Layer
		{
			get => _mLayer;
			set
			{
				if (_mLayer == value) return;
				_mLayer = value;

				Delta(ItemDelta.EquipOnly);
			}
		}

		public List<Item> Items
		{
			get
			{
				List<Item> items = LookupItems() ?? EmptyItems;

				return items;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public IEntity RootParent
		{
			get
			{
				IEntity p = _mParent;
				while (p is Item item)
				{
					if (item._mParent == null)
					{
						break;
					}

					p = item._mParent;
				}

				return p;
			}
		}

		public bool ParentsContain<T>() where T : Item
		{
			IEntity p = _mParent;

			while (p is Item item)
			{
				if (p is T)
					return true;

				if (item._mParent == null)
				{
					break;
				}

				p = item._mParent;
			}

			return false;
		}

		public virtual void AddItem(Item item)
		{
			if (item == null || item.Deleted || item._mParent == this)
			{
				return;
			}

			if (item == this)
			{
				Console.WriteLine("Warning: Adding item to itself: [0x{0:X} {1}].AddItem( [0x{2:X} {3}] )", Serial.Value, GetType().Name, item.Serial.Value, item.GetType().Name);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return;
			}

			if (IsChildOf(item))
			{
				Console.WriteLine("Warning: Adding parent item to child: [0x{0:X} {1}].AddItem( [0x{2:X} {3}] )", Serial.Value, GetType().Name, item.Serial.Value, item.GetType().Name);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return;
			}

			switch (item._mParent)
			{
				case Mobile parentMob:
					parentMob.RemoveItem(item);
					break;
				case Item parentItem:
					parentItem.RemoveItem(item);
					break;
				default:
					item.SendRemovePacket();
					break;
			}

			item.Parent = this;
			item.Map = _mMap;

			List<Item> items = AcquireItems();

			items.Add(item);

			if (!item.IsVirtualItem)
			{
				UpdateTotal(item, TotalType.Gold, item.TotalGold);
				UpdateTotal(item, TotalType.Items, item.TotalItems + 1);
				UpdateTotal(item, TotalType.Weight, item.TotalWeight + item.PileWeight);
			}

			item.Delta(ItemDelta.Update);

			item.OnAdded(this);
			OnItemAdded(item);
		}

		private static readonly List<Item> MDeltaQueue = new();

		public void Delta(ItemDelta flags)
		{
			if (_mMap == null || _mMap == Map.Internal)
				return;

			_mDeltaFlags |= flags;

			if (!GetFlag(ImplFlag.InQueue))
			{
				SetFlag(ImplFlag.InQueue, true);

				if (_processing)
				{
					try
					{
						using StreamWriter op = new("delta-recursion.log", true);
						op.WriteLine("# {0}", DateTime.UtcNow);
						op.WriteLine(new System.Diagnostics.StackTrace());
						op.WriteLine();
					}
					catch
					{
						// ignored
					}
				}
				else
				{
					MDeltaQueue.Add(this);
				}
			}

			Core.Set();
		}

		public void RemDelta(ItemDelta flags)
		{
			_mDeltaFlags &= ~flags;

			if (!GetFlag(ImplFlag.InQueue) || _mDeltaFlags != ItemDelta.None) return;
			SetFlag(ImplFlag.InQueue, false);

			if (_processing)
			{
				try
				{
					using StreamWriter op = new("delta-recursion.log", true);
					op.WriteLine("# {0}", DateTime.UtcNow);
					op.WriteLine(new System.Diagnostics.StackTrace());
					op.WriteLine();
				}
				catch
				{
					// ignored
				}
			}
			else
			{
				_ = MDeltaQueue.Remove(this);
			}
		}

		public bool NoMoveHs { get; set; }

		public void ProcessDelta()
		{
			ItemDelta flags = _mDeltaFlags;

			SetFlag(ImplFlag.InQueue, false);
			_mDeltaFlags = ItemDelta.None;

			Map map = _mMap;

			if (map == null || Deleted) return;
			bool sendOPLUpdate = ObjectPropertyList.Enabled && (flags & ItemDelta.Properties) != 0;

			if (_mParent is Container {IsPublicContainer: false} contParent)
			{
				if ((flags & ItemDelta.Update) != 0)
				{
					Point3D worldLoc = GetWorldLocation();

					Mobile rootParent = contParent.RootParent as Mobile;
					Mobile tradeRecip = null;

					if (rootParent != null)
					{
						NetState ns = rootParent.NetState;

						if (ns != null)
						{
							if (rootParent.CanSee(this) && rootParent.InRange(worldLoc, GetUpdateRange(rootParent)))
							{
								if (ns.ContainerGridLines)
									ns.Send(new ContainerContentUpdate6017(this));
								else
									ns.Send(new ContainerContentUpdate(this));

								if (ObjectPropertyList.Enabled)
									ns.Send(OplPacket);
							}
						}
					}

					SecureTradeContainer stc = GetSecureTradeCont();

					SecureTrade st = stc?.Trade;

					if (st != null)
					{
						Mobile test = st.From.Mobile;

						if (test != null && test != rootParent)
							tradeRecip = test;

						test = st.To.Mobile;

						if (test != null && test != rootParent)
							tradeRecip = test;

						NetState ns = tradeRecip?.NetState;

						if (ns != null)
						{
							if (tradeRecip.CanSee(this) && tradeRecip.InRange(worldLoc, GetUpdateRange(tradeRecip)))
							{
								if (ns.ContainerGridLines)
									ns.Send(new ContainerContentUpdate6017(this));
								else
									ns.Send(new ContainerContentUpdate(this));

								if (ObjectPropertyList.Enabled)
									ns.Send(OplPacket);
							}
						}
					}

					List<Mobile> openers = contParent.Openers;

					if (openers != null)
					{
						lock (openers)
						{
							for (int i = 0; i < openers.Count; ++i)
							{
								Mobile mob = openers[i];

								int range = GetUpdateRange(mob);

								if (mob.Map != map || !mob.InRange(worldLoc, range))
								{
									openers.RemoveAt(i--);
								}
								else
								{
									if (mob == rootParent || mob == tradeRecip)
										continue;

									NetState ns = mob.NetState;

									if (ns == null) continue;
									if (!mob.CanSee(this)) continue;
									if (ns.ContainerGridLines)
										ns.Send(new ContainerContentUpdate6017(this));
									else
										ns.Send(new ContainerContentUpdate(this));

									if (ObjectPropertyList.Enabled)
										ns.Send(OplPacket);
								}
							}

							if (openers.Count == 0)
								contParent.Openers = null;
						}
					}
					return;
				}
			}

			if ((flags & ItemDelta.Update) != 0)
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable<NetState> eable = map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (!m.CanSee(this) || !m.InRange(worldLoc, GetUpdateRange(m))) continue;
					if (_mParent == null)
					{
						SendInfoTo(state, ObjectPropertyList.Enabled);
					}
					else
					{
						if (p == null)
						{
							switch (_mParent)
							{
								case Item when state.ContainerGridLines:
									state.Send(new ContainerContentUpdate6017(this));
									break;
								case Item:
									state.Send(new ContainerContentUpdate(this));
									break;
								case Mobile:
									p = new EquipUpdate(this);
									p.Acquire();

									state.Send(p);
									break;
							}
						}
						else
						{
							state.Send(p);
						}

						if (ObjectPropertyList.Enabled)
						{
							state.Send(OplPacket);
						}
					}
				}

				if (p != null)
					Packet.Release(p);

				eable.Free();
				sendOPLUpdate = false;
			}
			else if ((flags & ItemDelta.EquipOnly) != 0)
			{
				if (_mParent is Mobile)
				{
					Packet p = null;
					Point3D worldLoc = GetWorldLocation();

					IPooledEnumerable<NetState> eable = map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

					foreach (NetState state in eable)
					{
						Mobile m = state.Mobile;

						if (!m.CanSee(this) || !m.InRange(worldLoc, GetUpdateRange(m))) continue;
						//if ( sendOPLUpdate )
						//	state.Send( RemovePacket );

						p ??= Packet.Acquire(new EquipUpdate(this));

						state.Send(p);

						if (ObjectPropertyList.Enabled)
							state.Send(OplPacket);
					}

					Packet.Release(p);

					eable.Free();
					sendOPLUpdate = false;
				}
			}

			if (sendOPLUpdate)
			{
				Point3D worldLoc = GetWorldLocation();
				IPooledEnumerable<NetState> eable = map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
						state.Send(OplPacket);
				}

				eable.Free();
			}
		}

		private static bool _processing = false;

		public static void ProcessDeltaQueue()
		{
			_processing = true;

			if (MDeltaQueue.Count >= 512)
			{
				_ = Parallel.ForEach(MDeltaQueue, i => i.ProcessDelta());
			}
			else
			{
				for (int i = 0; i < MDeltaQueue.Count; i++) MDeltaQueue[i].ProcessDelta();
			}

			MDeltaQueue.Clear();

			_processing = false;
		}

		public virtual void OnDelete()
		{
			if (Spawner != null)
			{
				Spawner.Remove(this);
				Spawner = null;
			}

			var region = Region.Find(GetWorldLocation(), Map);

			region?.OnDelete(this);
		}

		public virtual void OnParentDeleted(object parent)
		{
			Delete();
		}

		public virtual void FreeCache()
		{
			ReleaseWorldPackets();
			Packet.Release(ref _mRemovePacket);
			Packet.Release(ref _mOplPacket);
			Packet.Release(ref _mPropertyList);
		}

		public virtual void Delete()
		{
			if (Deleted)
				return;

			else if (!World.OnDelete(this))
				return;

			OnDelete();

			var items = LookupItems();

			if (items != null)
			{
				for (int i = items.Count - 1; i >= 0; --i)
				{
					if (i < items.Count)
						items[i].OnParentDeleted(this);
				}
			}

			SendRemovePacket();

			SetFlag(ImplFlag.Deleted, true);

			switch (Parent)
			{
				case Mobile parentMob:
					parentMob.RemoveItem(this);
					break;
				case Item parentItem:
					parentItem.RemoveItem(this);
					break;
			}

			ClearBounce();

			if (_mMap != null)
			{
				if (_mParent == null)
					_mMap.OnLeave(this);
				_mMap = null;
			}

			World.RemoveItem(this);

			OnAfterDelete();

			FreeCache();
		}

		public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			if (_mMap != null)
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(worldLoc, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (!m.CanSee(this) || !m.InRange(worldLoc, GetUpdateRange(m))) continue;
					if (p == null)
					{
						if (ascii)
							p = new AsciiMessage(m_Serial, _mItemId, type, hue, 3, Name, text);
						else
							p = new UnicodeMessage(m_Serial, _mItemId, type, hue, 3, "ENU", Name, text);

						p.Acquire();
					}

					state.Send(p);
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number)
		{
			PublicOverheadMessage(type, hue, number, "");
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, string args)
		{
			if (_mMap == null) return;
			Packet p = null;
			Point3D worldLoc = GetWorldLocation();

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(worldLoc, GetMaxUpdateRange());

			foreach (NetState state in eable)
			{
				Mobile m = state.Mobile;

				if (!m.CanSee(this) || !m.InRange(worldLoc, GetUpdateRange(m))) continue;
				p ??= Packet.Acquire(new MessageLocalized(m_Serial, _mItemId, type, hue, 3, number, Name, args));

				state.Send(p);
			}

			Packet.Release(p);

			eable.Free();
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, NetState state, string args = "")
		{
			if (Map == null || state == null) return;
			Packet p = null;
			Point3D worldLoc = GetWorldLocation();

			Mobile m = state.Mobile;

			if (m != null && m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
			{
				p ??= Packet.Acquire(new MessageLocalized(m_Serial, _mItemId, type, hue, 3, number, Name, args));

				state.Send(p);
			}

			Packet.Release(p);
		}

		public void PrivateOverheadMessage(MessageType type, int hue, bool ascii, string text, NetState state)
		{
			if (Map == null || state == null) return;
			Point3D worldLoc = GetWorldLocation();
			Mobile m = state.Mobile;

			Packet asciip = null;
			Packet p = null;

			if (m != null && m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
			{
				if (ascii)
				{
					asciip = Packet.Acquire(new AsciiMessage(m_Serial, _mItemId, type, hue, 3, Name, text));

					state.Send(asciip);
				}
				else
				{
					p ??= Packet.Acquire(new UnicodeMessage(m_Serial, _mItemId, type, hue, 3, m.Language, Name, text));

					state.Send(p);
				}
			}

			Packet.Release(asciip);
			Packet.Release(p);
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, NetState state)
		{
			PrivateOverheadMessage(type, hue, number, "", state);
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, string args, NetState state)
		{
			Point3D worldLoc = GetWorldLocation();
			if (Map != null && Map != Map.Internal && state != null)
			{
				var m = state.Mobile;

				if (m != null && m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
				{
					state.Send(new MessageLocalized(m_Serial, _mItemId, type, hue, 3, number, Name, args));
				}
			}
		}

		public Region GetRegion()
		{
			return Region.Find(GetWorldLocation(), Map);
		}

		public double GetDistanceToSqrt(IPoint3D p)
		{
			Point3D loc = GetWorldLocation();

			int xDelta = loc.X - p.X;
			int yDelta = loc.Y - p.Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public bool InRange(IPoint3D p, int range)
		{
			Point3D loc = GetWorldLocation();

			return (p.X >= (loc.X - range))
				&& (p.X <= (loc.X + range))
				&& (p.Y >= (loc.Y - range))
				&& (p.Y <= (loc.Y + range));
		}

		public bool InLos(Point3D target)
		{
			if (Deleted || Map == null || Parent != null)
				return false;

			return Map.LineOfSight(this, target);
		}

		public virtual void OnAfterDelete()
		{
			Sockets?.IterateReverse(socket =>
			{
				socket.Remove();
			});

			EventSink.InvokeOnItemDeleted(this);
		}

		public virtual void RemoveItem(Item item)
		{
			List<Item> items = LookupItems();

			if (items == null || !items.Contains(item)) return;
			item.SendRemovePacket();

			_ = items.Remove(item);

			if (!item.IsVirtualItem)
			{
				UpdateTotal(item, TotalType.Gold, -item.TotalGold);
				UpdateTotal(item, TotalType.Items, -(item.TotalItems + 1));
				UpdateTotal(item, TotalType.Weight, -(item.TotalWeight + item.PileWeight));
			}

			item.Parent = null;

			item.OnRemoved(this);
			OnItemRemoved(item);
		}

		public virtual void OnAfterDuped(Item newItem)
		{
		}

		public virtual bool OnDragLift(Mobile from)
		{
			return true;
		}

		public virtual bool OnEquip(Mobile from)
		{
			return true;
		}

		public ISpawner Spawner
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				return info?.MSpawner;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.MSpawner = value;

				if (info.MSpawner == null)
					VerifyCompactInfo();
			}
		}

		public virtual void OnBeforeSpawn(Point3D location, Map m)
		{
		}

		public virtual void OnAfterSpawn()
		{
		}

		public virtual int PhysicalResistance => 0;
		public virtual int FireResistance => 0;
		public virtual int ColdResistance => 0;
		public virtual int PoisonResistance => 0;
		public virtual int EnergyResistance => 0;

		private Serial m_Serial;
		[CommandProperty(AccessLevel.Counselor)]
		public Serial Serial => m_Serial;

		internal void NewSerial()
		{
			m_Serial = Serial.NewItem;
		}

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Developer)]
		public IEntity ParentEntity => Parent;

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Developer)]
		public IEntity RootParentEntity => RootParent;

		#region Location Location Location!
		public virtual void OnLocationChange(Point3D oldLocation)
		{
			var items = Items;

			if (items == null)
			{
				return;
			}

			var i = items.Count;

			while (--i >= 0)
			{
				if (i >= items.Count)
				{
					continue;
				}

				var o = items[i];

				if (o != null)
				{
					o.OnParentLocationChange(oldLocation);
				}
			}
		}

		public virtual void OnParentLocationChange(Point3D oldLocation)
		{ }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual Point3D Location
		{
			get => _mLocation;
			set
			{
				Point3D oldLocation = _mLocation;

				if (oldLocation == value) return;
				if (_mMap != null)
				{
					switch (_mParent)
					{
						case null:
						{
							IPooledEnumerable<NetState> eable;

							if (_mLocation.m_X != 0)
							{
								eable = _mMap.GetClientsInRange(oldLocation, GetMaxUpdateRange());

								foreach (NetState state in eable)
								{
									Mobile m = state.Mobile;

									if (!m.InRange(value, GetUpdateRange(m)))
									{
										state.Send(RemovePacket);
									}
								}

								eable.Free();
							}

							Point3D oldLoc = _mLocation;
							_mLocation = value;
							ReleaseWorldPackets();

							SetLastMoved();

							eable = _mMap.GetClientsInRange(_mLocation, GetMaxUpdateRange());

							foreach (NetState state in eable)
							{
								Mobile m = state.Mobile;

								if (m.CanSee(this) && m.InRange(_mLocation, GetUpdateRange(m)) && (!state.HighSeas || !NoMoveHs || (_mDeltaFlags & ItemDelta.Update) != 0 || !m.InRange(oldLoc, GetUpdateRange(m))))
									SendInfoTo(state);
							}

							eable.Free();

							RemDelta(ItemDelta.Update);
							break;
						}
						case Item:
							_mLocation = value;
							ReleaseWorldPackets();

							Delta(ItemDelta.Update);
							break;
						default:
							_mLocation = value;
							ReleaseWorldPackets();
							break;
					}

					if (_mParent == null)
						_mMap.OnMove(oldLocation, this);
				}
				else
				{
					_mLocation = value;
					ReleaseWorldPackets();
				}

				OnLocationChange(oldLocation);
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int X
		{
			get => _mLocation.m_X;
			set => Location = new Point3D(value, _mLocation.m_Y, _mLocation.m_Z);
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Y
		{
			get => _mLocation.m_Y;
			set => Location = new Point3D(_mLocation.m_X, value, _mLocation.m_Z);
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Z
		{
			get => _mLocation.m_Z;
			set => Location = new Point3D(_mLocation.m_X, _mLocation.m_Y, value);
		}
		#endregion

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ItemId
		{
			get => _mItemId;
			set
			{
				if (_mItemId == value) return;
				int oldPileWeight = PileWeight;

				_mItemId = value;
				ReleaseWorldPackets();

				int newPileWeight = PileWeight;

				UpdateTotal(this, TotalType.Weight, newPileWeight - oldPileWeight);

				InvalidateProperties();
				Delta(ItemDelta.Update);
			}
		}

		public virtual string DefaultName => null;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Name
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				if (info != null && info.MName != null)
					return info.MName;

				return DefaultName;
			}
			set
			{
				if (value == null || value != DefaultName)
				{
					CompactInfo info = AcquireCompactInfo();

					info.MName = value;

					if (info.MName == null)
						VerifyCompactInfo();

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Developer)]
		public IEntity Parent
		{
			get => _mParent;
			set
			{
				if (_mParent == value)
					return;

				IEntity oldParent = _mParent;

				_mParent = value;

				if (_mMap != null)
				{
					if (oldParent != null && _mParent == null)
						_mMap.OnEnter(this);
					else if (_mParent != null)
						_mMap.OnLeave(this);
				}

				if (World.Loading) return;
				if (oldParent is Item parentItem)
				{
					oldParent = parentItem.RootParent;
				}

				if (RootParent is Mobile root && oldParent != root)
				{
					root.OnItemObtained(this);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public LightType Light
		{
			get => (LightType)_mDirection;
			set
			{
				if ((LightType) _mDirection == value) return;
				_mDirection = (Direction)value;
				ReleaseWorldPackets();

				Delta(ItemDelta.Update);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Direction
		{
			get => _mDirection;
			set
			{
				if (_mDirection == value) return;
				_mDirection = value;
				ReleaseWorldPackets();

				Delta(ItemDelta.Update);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Amount
		{
			get => _mAmount;
			set
			{
				int oldValue = _mAmount;

				if (oldValue == value) return;
				int oldPileWeight = PileWeight;

				_mAmount = value;
				ReleaseWorldPackets();

				int newPileWeight = PileWeight;

				UpdateTotal(this, TotalType.Weight, newPileWeight - oldPileWeight);

				OnAmountChange(oldValue);

				Delta(ItemDelta.Update);

				if (oldValue > 1 || value > 1)
					InvalidateProperties();

				if (!Stackable && _mAmount > 1)
					Console.WriteLine("Warning: 0x{0:X}: Amount changed for non-stackable item '{2}'. ({1})", Serial.Value, _mAmount, GetType().Name);
			}
		}

		protected virtual void OnAmountChange(int oldValue)
		{
		}

		public virtual bool HandlesOnSpeech => false;

		public virtual void OnSpeech(SpeechEventArgs e)
		{
		}

		public virtual bool OnDroppedToMobile(Mobile from, Mobile target)
		{
			if (Nontransferable && from.Player)
			{
				HandleInvalidTransfer(from);
				return false;
			}

			return true;
		}

		public virtual bool DropToMobile(Mobile from, Mobile target, Point3D p)
		{
			if (Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null)
				return false;
			if (from.IsPlayer() && !from.InRange(target.Location, 2))
				return false;
			if (!from.CanSee(target) || !from.InLOS(target))
				return false;
			if (!from.OnDroppedItemToMobile(this, target))
				return false;
			return OnDroppedToMobile(from, target) && target.OnDragDrop(from, this);
		}

		public virtual bool OnDroppedInto(Mobile from, Container target, Point3D p)
		{
			if (!from.OnDroppedItemInto(this, target, p))
			{
				return false;
			}

			if (!Nontransferable || !from.Player || target == from.Backpack)
				return target.OnDragDropInto(from, this, p);
			HandleInvalidTransfer(from);
			return false;

		}

		public virtual bool OnDroppedOnto(Mobile from, Item target)
		{
			if (Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null)
				return false;
			if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(target.GetWorldLocation(), 2))
				return false;
			if (!from.CanSee(target) || !from.InLOS(target))
				return false;
			if (!target.IsAccessibleTo(from))
				return false;
			if (!from.OnDroppedItemOnto(this, target))
				return false;
			if (!Nontransferable || !from.Player || target == from.Backpack) return target.OnDragDrop(from, this);
			HandleInvalidTransfer(from);
			return false;

		}

		public virtual bool DropToItem(Mobile from, Item target, Point3D p)
		{
			if (Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null)
				return false;

			object root = target.RootParent;

			if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(target.GetWorldLocation(), 2))
				return false;
			if (!from.CanSee(target) || !from.InLOS(target))
				return false;
			if (!target.IsAccessibleTo(from))
				return false;
			if (root is Mobile mobile && !mobile.CheckNonlocalDrop(from, this, target))
				return false;
			if (!from.OnDroppedItemToItem(this, target, p))
				return false;
			if (target is Container container && p.m_X != -1 && p.m_Y != -1)
				return OnDroppedInto(from, container, p);
			return OnDroppedOnto(from, target);
		}

		public virtual bool OnDroppedToWorld(Mobile from, Point3D p)
		{
			if (!Nontransferable || !from.Player) return true;
			HandleInvalidTransfer(from);
			return false;

		}

		public virtual int GetLiftSound(Mobile from)
		{
			return 0x57;
		}

		private static int _mOpenSlots;

		public virtual bool DropToWorld(Mobile from, Point3D p)
		{
			if (Deleted || from.Deleted || from.Map == null)
				return false;
			else if (!from.InRange(p, 2))
				return false;

			Map map = from.Map;

			if (map == null)
				return false;

			int x = p.m_X, y = p.m_Y;
			int z = int.MinValue;

			int maxZ = from.Z + 16;

			LandTile landTile = map.Tiles.GetLandTile(x, y);
			TileFlag landFlags = TileData.LandTable[landTile.Id & TileData.MaxLandValue].Flags;

			int landZ = 0, landAvg = 0, landTop = 0;
			map.GetAverageZ(x, y, ref landZ, ref landAvg, ref landTop);

			if (!landTile.Ignored && (landFlags & TileFlag.Impassable) == 0)
			{
				if (landAvg <= maxZ)
					z = landAvg;
			}

			StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, true);

			for (int i = 0; i < tiles.Length; ++i)
			{
				StaticTile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.Id & TileData.MaxItemValue];

				if (!id.Surface)
					continue;

				int top = tile.Z + id.CalcHeight;

				if (top > maxZ || top < z)
					continue;

				z = top;
			}

			List<Item> items = new();

			IPooledEnumerable<Item> eable = map.GetItemsInRange(p, 0);

			foreach (Item item in eable)
			{
				if (item is BaseMulti || item.ItemId > TileData.MaxItemValue)
					continue;

				items.Add(item);

				ItemData id = item.ItemData;

				if (!id.Surface)
					continue;

				int top = item.Z + id.CalcHeight;

				if (top > maxZ || top < z)
					continue;

				z = top;
			}

			eable.Free();

			if (z == int.MinValue)
				return false;

			if (z > maxZ)
				return false;

			_mOpenSlots = (1 << 20) - 1;

			int surfaceZ = z;

			for (int i = 0; i < tiles.Length; ++i)
			{
				StaticTile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.Id & TileData.MaxItemValue];

				int checkZ = tile.Z;
				int checkTop = checkZ + id.CalcHeight;

				if (checkTop == checkZ && !id.Surface)
					++checkTop;

				int zStart = checkZ - z;
				int zEnd = checkTop - z;

				if (zStart >= 20 || zEnd < 0)
					continue;

				if (zStart < 0)
					zStart = 0;

				if (zEnd > 19)
					zEnd = 19;

				int bitCount = zEnd - zStart;

				_mOpenSlots &= ~(((1 << bitCount) - 1) << zStart);
			}

			for (int i = 0; i < items.Count; ++i)
			{
				Item item = items[i];
				ItemData id = item.ItemData;

				int checkZ = item.Z;
				int checkTop = checkZ + id.CalcHeight;

				if (checkTop == checkZ && !id.Surface)
					++checkTop;

				int zStart = checkZ - z;
				int zEnd = checkTop - z;

				if (zStart >= 20 || zEnd < 0)
					continue;

				if (zStart < 0)
					zStart = 0;

				if (zEnd > 19)
					zEnd = 19;

				int bitCount = zEnd - zStart;

				_mOpenSlots &= ~(((1 << bitCount) - 1) << zStart);
			}

			int height = ItemData.Height;

			if (height == 0)
				++height;

			if (height > 30)
				height = 30;

			int match = (1 << height) - 1;
			bool okay = false;

			for (int i = 0; i < 20; ++i)
			{
				if ((i + height) > 20)
					match >>= 1;

				okay = ((_mOpenSlots >> i) & match) == match;

				if (okay)
				{
					z += i;
					break;
				}
			}

			if (!okay)
				return false;

			height = ItemData.Height;

			if (height == 0)
				++height;

			if (landAvg > z && z + height > landZ)
				return false;
			if ((landFlags & TileFlag.Impassable) != 0 && landAvg > surfaceZ && z + height > landZ)
				return false;

			for (int i = 0; i < tiles.Length; ++i)
			{
				StaticTile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.Id & TileData.MaxItemValue];

				int checkZ = tile.Z;
				int checkTop = checkZ + id.CalcHeight;

				if (checkTop > z && z + height > checkZ)
					return false;
				if ((id.Surface || id.Impassable) && checkTop > surfaceZ && z + height > checkZ)
					return false;
			}

			for (int i = 0; i < items.Count; ++i)
			{
				Item item = items[i];
				ItemData id = item.ItemData;

				//int checkZ = item.Z;
				//int checkTop = checkZ + id.CalcHeight;

				if ((item.Z + id.CalcHeight) > z && (z + height) > item.Z)
					return false;
			}

			p = new Point3D(x, y, z);

			if (!from.InLOS(new Point3D(x, y, z + 1)))
				return false;
			if (!from.OnDroppedItemToWorld(this, p))
				return false;
			if (!OnDroppedToWorld(from, p))
				return false;

			int soundId = GetDropSound();

			MoveToWorld(p, from.Map);

			from.SendSound(soundId == -1 ? 0x42 : soundId, GetWorldLocation());

			return true;
		}

		public void SendRemovePacket()
		{
			if (!Deleted && _mMap != null)
			{
				Point3D worldLoc = GetWorldLocation();

				var eable = _mMap.GetClientsInRange(worldLoc, GetMaxUpdateRange() - 4);

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (Utility.InRange(worldLoc, m.Location, GetUpdateRange(m)))
					{
						state.Send(RemovePacket);
					}
				}

				eable.Free();
			}
		}

		public virtual int GetDropSound()
		{
			return -1;
		}

		public Point3D GetWorldLocation()
		{
			IEntity root = RootParentEntity;

			return root?.Location ?? _mLocation;

			//return root == null ? m_Location : new Point3D( (IPoint3D) root );
		}

		public virtual bool BlocksFit => false;

		public Point3D GetSurfaceTop()
		{
			IEntity root = RootParentEntity;

			return root?.Location ?? new Point3D(_mLocation.m_X, _mLocation.m_Y, _mLocation.m_Z + (ItemData.Surface ? ItemData.CalcHeight : 0));
		}

		public Point3D GetWorldTop()
		{
			IEntity root = RootParentEntity;

			return root?.Location ?? new Point3D(_mLocation.m_X, _mLocation.m_Y, _mLocation.m_Z + ItemData.CalcHeight);
		}

		public void SendLocalizedMessageTo(Mobile to, int number)
		{
			if (Deleted || !to.CanSee(this))
				return;

			_ = to.Send(new MessageLocalized(Serial, ItemId, MessageType.Regular, DisplayColor, 3, number, "", ""));
		}

		public void SendLocalizedMessageTo(Mobile to, int number, string args)
		{
			if (Deleted || !to.CanSee(this))
				return;

			_ = to.Send(new MessageLocalized(Serial, ItemId, MessageType.Regular, DisplayColor, 3, number, "", args));
		}

		public void SendLocalizedMessageTo(Mobile to, int number, AffixType affixType, string affix, string args)
		{
			if (Deleted || !to.CanSee(this))
				return;

			_ = to.Send(new MessageLocalizedAffix(Serial, ItemId, MessageType.Regular, DisplayColor, 3, number, "", affixType, affix, args));
		}

		public void SendLocalizedMessage(int number, string args)
		{
			if (Deleted || Map == null)
			{
				return;
			}

			IPooledEnumerable eable = Map.GetClientsInRange(Location, Map.GlobalMaxUpdateRange);
			Packet p = Packet.Acquire(new MessageLocalized(Serial, _mItemId, MessageType.Regular, DisplayColor, 3, number, Name, args));

			foreach (NetState ns in eable)
			{
				ns.Send(p);
			}

			Packet.Release(p);
			eable.Free();
		}

		public void SendLocalizedMessage(MessageType type, int number, AffixType affixType, string affix, string args)
		{
			IPooledEnumerable eable = Map.GetClientsInRange(Location, Map.GlobalMaxUpdateRange);
			Packet p = Packet.Acquire(new MessageLocalizedAffix(Serial, _mItemId, type, DisplayColor, 3, number, "", affixType, affix, args));

			foreach (NetState ns in eable)
			{
				ns.Send(p);
			}

			Packet.Release(p);
			eable.Free();
		}

		#region OnDoubleClick[...]

		public virtual void OnDoubleClick(Mobile from)
		{
		}

		public virtual void OnDoubleClickOutOfRange(Mobile from)
		{
		}

		public virtual void OnDoubleClickCantSee(Mobile from)
		{
		}

		public virtual void OnDoubleClickDead(Mobile from)
		{
			from.LocalOverheadMessage(MessageType.Regular, DisplayColor, 1019048); // I am dead and cannot do that.
		}

		public virtual void OnDoubleClickNotAccessible(Mobile from)
		{
			from.SendLocalizedMessage(500447); // That is not accessible.
		}

		public virtual void OnDoubleClickSecureTrade(Mobile from)
		{
			from.SendLocalizedMessage(500447); // That is not accessible.
		}
		#endregion

		public virtual void OnSnoop(Mobile from)
		{
		}

		public bool InSecureTrade => (GetSecureTradeCont() != null);

		public SecureTradeContainer GetSecureTradeCont()
		{
			object p = this;

			while (p is Item item)
			{
				if (p is SecureTradeContainer container)
					return container;

				p = item._mParent;
			}

			return null;
		}

		public virtual void OnItemAdded(Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnSubItemAdded(item);
					break;
				case Mobile parentMob:
					parentMob.OnSubItemAdded(item);
					break;
			}
		}

		public virtual void OnItemRemoved(Item item)
		{
			if (_mParent is Item parentItem)
				parentItem.OnSubItemRemoved(item);
			else if (_mParent is Mobile parentMob)
				parentMob.OnSubItemRemoved(item);
		}

		public virtual void OnSubItemAdded(Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnSubItemAdded(item);
					break;
				case Mobile parentMob:
					parentMob.OnSubItemAdded(item);
					break;
			}
		}

		public virtual void OnSubItemRemoved(Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnSubItemRemoved(item);
					break;
				case Mobile parentMob:
					parentMob.OnSubItemRemoved(item);
					break;
			}
		}

		public virtual void OnItemBounceCleared(Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnSubItemBounceCleared(item);
					break;
				case Mobile parentMob:
					parentMob.OnSubItemBounceCleared(item);
					break;
			}
		}

		public virtual void OnSubItemBounceCleared(Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnSubItemBounceCleared(item);
					break;
				case Mobile parentMob:
					parentMob.OnSubItemBounceCleared(item);
					break;
			}
		}

		public virtual bool CheckTarget(Mobile from, Targeting.Target targ, object targeted)
		{
			return _mParent switch
			{
				Item parentItem => parentItem.CheckTarget(from, targ, targeted),
				Mobile parentMob => parentMob.CheckTarget(from, targ, targeted),
				_ => true
			};
		}

		public virtual void OnStatsQuery(Mobile m)
		{
			if (m == null || m.Deleted || m.Map != Map || m.NetState == null)
			{
				return;
			}

			if (m.InUpdateRange(this) && m.CanSee(this))
			{
				SendStatusTo(m.NetState);
			}
		}

		public virtual void SendStatusTo(NetState state)
		{
			var p = GetStatusPacketFor(state);

			if (p != null)
			{
				state.Send(p);
			}
		}

		public virtual Packet GetStatusPacketFor(NetState state)
		{
			return this is IDamageable damageable && state != null && state.Mobile != null && state.HighSeas
				? new MobileStatusCompact(CanBeRenamedBy(state.Mobile), damageable)
				: (Packet)null;
		}

		public virtual bool CanBeRenamedBy(Mobile m)
		{
			return m != null && m.IsStaff();
		}

		public virtual bool IsAccessibleTo(Mobile check)
		{
			if (_mParent is Item parentItem)
				return parentItem.IsAccessibleTo(check);

			Region reg = Region.Find(GetWorldLocation(), _mMap);

			return reg.CheckAccessibility(this, check);

			/*SecureTradeContainer cont = GetSecureTradeCont();

			if ( cont != null && !cont.IsChildOf( check ) )
				return false;

			return true;*/
		}

		public bool IsChildOf(IEntity o)
		{
			return IsChildOf(o, false);
		}

		public bool IsChildOf(IEntity o, bool allowNull)
		{
			IEntity p = _mParent;

			if ((p == null || o == null) && !allowNull)
				return false;

			if (p == o)
				return true;

			while (p is Item item)
			{
				if (item._mParent == null)
				{
					break;
				}

				p = item._mParent;

				if (p == o)
					return true;
			}

			return false;
		}

		public ItemData ItemData => TileData.ItemTable[_mItemId & TileData.MaxItemValue];

		public virtual void OnItemUsed(Mobile from, Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnItemUsed(from, item);
					break;
				case Mobile parentMob:
					parentMob.OnItemUsed(from, item);
					break;
			}
		}

		public bool CheckItemUse(Mobile from)
		{
			return CheckItemUse(from, this);
		}

		public virtual bool CheckItemUse(Mobile from, Item item)
		{
			return _mParent switch
			{
				Item parentItem => parentItem.CheckItemUse(from, item),
				Mobile parentMob => parentMob.CheckItemUse(from, item),
				_ => true
			};
		}

		public virtual void OnItemLifted(Mobile from, Item item)
		{
			switch (_mParent)
			{
				case Item parentItem:
					parentItem.OnItemLifted(from, item);
					break;
				case Mobile parentMobile:
					parentMobile.OnItemLifted(from, item);
					break;
			}
		}

		public bool CheckLift(Mobile from)
		{
			LRReason reject = LRReason.Inspecific;

			return CheckLift(from, this, ref reject);
		}

		public virtual bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			return _mParent switch
			{
				Item parentItem => parentItem.CheckLift(from, item, ref reject),
				Mobile parentMobile => parentMobile.CheckLift(from, item, ref reject),
				_ => true
			};
		}

		public virtual bool CanTarget => true;
		public virtual bool DisplayLootType => true;

		public virtual void OnSingleClickContained(Mobile from, Item item)
		{
			if (_mParent is Item parent)
				parent.OnSingleClickContained(from, item);
		}

		public virtual void OnAosSingleClick(Mobile from)
		{
			ObjectPropertyList opl = PropertyList;

			if (opl.Header > 0)
				_ = from.Send(new MessageLocalized(Serial, _mItemId, MessageType.Label, DisplayColor, 3, opl.Header, Name, opl.HeaderArgs));
		}

		public virtual void OnSingleClick(Mobile from)
		{
			if (Deleted || !from.CanSee(this))
				return;

			if (DisplayLootType)
				LabelLootTypeTo(from);

			NetState ns = from.NetState;

			if (ns == null) return;
			if (Name == null)
			{
				if (_mAmount <= 1)
					ns.Send(new MessageLocalized(Serial, _mItemId, MessageType.Label, DisplayColor, 3, LabelNumber, "", ""));
				else
					ns.Send(new MessageLocalizedAffix(Serial, _mItemId, MessageType.Label, DisplayColor, 3, LabelNumber, "", AffixType.Append, $" : {_mAmount}", ""));
			}
			else
			{
				ns.Send(new UnicodeMessage(Serial, _mItemId, MessageType.Label, DisplayColor, 3, "ENU", "", Name + (_mAmount > 1 ? " : " + _mAmount : "")));
			}
		}

		public static bool ScissorCopyLootType { get; set; }

		public virtual void ScissorHelper(Mobile from, Item newItem, int amountPerOldItem)
		{
			ScissorHelper(from, newItem, amountPerOldItem, true);
		}

		public virtual void ScissorHelper(Mobile from, Item newItem, int amountPerOldItem, bool carryHue)
		{
			int amount = Amount;

			if (amount > (60000 / amountPerOldItem)) // let's not go over 60000
				amount = 60000 / amountPerOldItem;

			Amount -= amount;

			int ourHue = Hue;
			Map thisMap = Map;
			IEntity thisParent = _mParent;
			Point3D worldLoc = GetWorldLocation();
			LootType type = LootType;

			if (Amount == 0)
				Delete();

			newItem.Amount = amount * amountPerOldItem;

			if (carryHue)
				newItem.Hue = ourHue;

			if (ScissorCopyLootType)
				newItem.LootType = type;

			if (thisParent is not Container container || !container.TryDropItem(from, newItem, false))
				newItem.MoveToWorld(worldLoc, thisMap);
		}

		public virtual void Consume()
		{
			Consume(1);
		}

		public virtual void Consume(int amount)
		{
			Amount -= amount;

			if (Amount <= 0)
				Delete();
		}

		public virtual void ReplaceWith(Item newItem)
		{
			if (_mParent is Container parent)
			{
				parent.AddItem(newItem);
				newItem.Location = _mLocation;
			}
			else
			{
				newItem.MoveToWorld(GetWorldLocation(), _mMap);
			}

			Delete();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool QuestItem
		{
			get => GetFlag(ImplFlag.QuestItem);
			set
			{
				SetFlag(ImplFlag.QuestItem, value);

				InvalidateProperties();

				ReleaseWorldPackets();

				Delta(ItemDelta.Update);
			}
		}

		public bool Insured
		{
			get => GetFlag(ImplFlag.Insured);
			set { SetFlag(ImplFlag.Insured, value); InvalidateProperties(); }
		}

		public bool PayedInsurance
		{
			get => GetFlag(ImplFlag.PayedInsurance);
			set => SetFlag(ImplFlag.PayedInsurance, value);
		}

		public Mobile BlessedFor
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				return info?.MBlessedFor;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.MBlessedFor = value;

				if (info.MBlessedFor == null)
					VerifyCompactInfo();

				InvalidateProperties();
			}
		}

		public virtual bool CheckBlessed(object obj)
		{
			return CheckBlessed(obj as Mobile);
		}

		public virtual bool CheckBlessed(Mobile m)
		{
			if (_mLootType == LootType.Blessed || (Mobile.InsuranceEnabled && Insured))
				return true;

			return (m != null && m == BlessedFor);
		}

		public virtual bool CheckNewbied()
		{
			return (_mLootType == LootType.Newbied);
		}

		public virtual bool IsStandardLoot()
		{
			if (Mobile.InsuranceEnabled && Insured)
				return false;

			if (BlessedFor != null)
				return false;

			return (_mLootType == LootType.Regular);
		}

		public override string ToString()
		{
			return $"0x{Serial.Value:X} \"{GetType().Name}\"";
		}

		internal int MTypeRef;

		public Item()
		{
			m_Serial = Serial.NewItem;

			Visible = true;
			Movable = true;
			Amount = 1;
			_mMap = Map.Internal;

			SetLastMoved();

			World.AddItem(this);

			Type ourType = GetType();
			MTypeRef = World.m_ItemTypes.IndexOf(ourType);

			if (MTypeRef == -1)
			{
				World.m_ItemTypes.Add(ourType);
				MTypeRef = World.m_ItemTypes.Count - 1;
			}

			Timer.DelayCall(() =>
			{
				if (!Deleted)
				{
					EventSink.InvokeOnItemCreated(this);
				}
			});
		}

		[Constructable]
		public Item(int itemId) : this()
		{
			_mItemId = itemId;
		}

		public Item(Serial serial)
		{
			m_Serial = serial;

			Type ourType = GetType();
			MTypeRef = World.m_ItemTypes.IndexOf(ourType);

			if (MTypeRef != -1) return;
			World.m_ItemTypes.Add(ourType);
			MTypeRef = World.m_ItemTypes.Count - 1;
		}

		public virtual void OnSectorActivate()
		{
		}

		public virtual void OnSectorDeactivate()
		{
		}

		#region Item Sockets
		public List<ItemSocket> Sockets { get; private set; }

		public void AttachSocket(ItemSocket socket)
		{
			Sockets ??= new List<ItemSocket>();

			Sockets.Add(socket);
			socket.Owner = this;

			InvalidateProperties();
		}

		public bool RemoveSocket<T>()
		{
			var socket = GetSocket(typeof(T));

			if (socket == null) return false;
			RemoveItemSocket(socket);
			return true;

		}

		public void RemoveItemSocket(ItemSocket socket)
		{
			if (Sockets == null)
			{
				return;
			}

			Sockets.Remove(socket);
			socket.OnRemoved();

			if (Sockets.Count == 0)
			{
				Sockets = null;
			}

			InvalidateProperties();
		}

		public T GetSocket<T>() where T : ItemSocket
		{
			return Sockets?.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
		}

		public T GetSocket<T>(Func<T, bool> predicate) where T : ItemSocket
		{
			return Sockets?.FirstOrDefault(s => s.GetType() == typeof(T) && (predicate == null || predicate(s as T))) as T;
		}

		public ItemSocket GetSocket(Type type)
		{
			return Sockets?.FirstOrDefault(s => s.GetType() == type);
		}

		public bool HasSocket<T>()
		{
			return Sockets != null && Sockets.Any(s => s.GetType() == typeof(T));
		}

		public bool HasSocket(Type t)
		{
			return Sockets != null && Sockets.Any(s => s.GetType() == t);
		}
		#endregion
	}
}
