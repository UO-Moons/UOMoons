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
	public class Item : IEntity, IHued, IComparable<Item>, ISerializable, ISpawnable
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
			if (other == null || other is IEntity)
				return CompareTo((IEntity)other);

			throw new ArgumentException("IEntity in Items");
		}

		#region Standard fields
		private Point3D m_Location;
		private int m_ItemID;
		private int m_Hue;
		private int m_Amount;
		private Layer m_Layer;
		private IEntity m_Parent; // Mobile, Item, or null=World
		private Map m_Map;
		private LootType m_LootType;
		private Direction m_Direction;
		private ItemRank m_ItemRank;
		#endregion

		private ItemDelta m_DeltaFlags;
		private ImplFlag m_Flags;

		#region Packet caches
		private Packet m_WorldPacket;
		private Packet m_WorldPacketSA;
		private Packet m_WorldPacketHS;
		private Packet m_RemovePacket;
		private Packet m_OPLPacket;
		private ObjectPropertyList m_PropertyList;
		#endregion

		public int TempFlags
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				if (info != null)
					return info.m_TempFlags;

				return 0;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.m_TempFlags = value;

				if (info.m_TempFlags == 0)
					VerifyCompactInfo();
			}
		}

		public int SavedFlags
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				if (info != null)
					return info.m_SavedFlags;

				return 0;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.m_SavedFlags = value;

				if (info.m_SavedFlags == 0)
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

				return info?.m_HeldBy;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.m_HeldBy = value;

				if (info.m_HeldBy == null)
					VerifyCompactInfo();
			}
		}
		/// <summary>
		/// The is the gridlocation for Enahanced Client.
		/// </summary>
		private byte m_GridLocation = 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public byte GridLocation
		{
			get => m_GridLocation;
			set
			{
				if (Parent is Container container)
				{
					if (value < 0 || value > 0x7C || !container.IsFreePosition(value))
					{
						m_GridLocation = container.GetNewPosition(0);
					}
					else
					{
						m_GridLocation = value;
					}
				}
				else
				{
					m_GridLocation = value;
				}
			}
		}

		[Flags]
		private enum ImplFlag : byte
		{
			None = 0x00,
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
			public string m_Name;
			public List<Item> m_Items;
			public BounceInfo m_Bounce;
			public Mobile m_HeldBy;
			public Mobile m_BlessedFor;
			public ISpawner m_Spawner;
			public int m_TempFlags;
			public int m_SavedFlags;
			public double m_Weight = -1;
		}

		private CompactInfo m_CompactInfo;

		public ExpandFlag GetExpandFlags()
		{
			CompactInfo info = LookupCompactInfo();

			ExpandFlag flags = 0;

			if (info != null)
			{
				if (info.m_BlessedFor != null)
					flags |= ExpandFlag.Blessed;

				if (info.m_Bounce != null)
					flags |= ExpandFlag.Bounce;

				if (info.m_HeldBy != null)
					flags |= ExpandFlag.Holder;

				if (info.m_Items != null)
					flags |= ExpandFlag.Items;

				if (info.m_Name != null)
					flags |= ExpandFlag.Name;

				if (info.m_Spawner != null)
					flags |= ExpandFlag.Spawner;

				if (info.m_SavedFlags != 0)
					flags |= ExpandFlag.SaveFlag;

				if (info.m_TempFlags != 0)
					flags |= ExpandFlag.TempFlag;

				if (info.m_Weight != -1)
					flags |= ExpandFlag.Weight;
			}

			return flags;
		}

		private CompactInfo LookupCompactInfo()
		{
			return m_CompactInfo;
		}

		private CompactInfo AcquireCompactInfo()
		{
			if (m_CompactInfo == null)
				m_CompactInfo = new CompactInfo();

			return m_CompactInfo;
		}

		private void ReleaseCompactInfo()
		{
			m_CompactInfo = null;
		}

		private void VerifyCompactInfo()
		{
			CompactInfo info = m_CompactInfo;

			if (info == null)
				return;

			bool isValid = (info.m_Name != null)
							|| (info.m_Items != null)
							|| (info.m_Bounce != null)
							|| (info.m_HeldBy != null)
							|| (info.m_BlessedFor != null)
							|| (info.m_Spawner != null)
							|| (info.m_TempFlags != 0)
							|| (info.m_SavedFlags != 0)
							|| (info.m_Weight != -1);

			if (!isValid)
				ReleaseCompactInfo();
		}

		public List<Item> LookupItems()
		{
			if (this is Container)
				return (this as Container).m_Items;

			CompactInfo info = LookupCompactInfo();

			return info?.m_Items;
		}

		public List<Item> AcquireItems()
		{
			if (this is Container)
			{
				Container cont = this as Container;

				if (cont.m_Items == null)
					cont.m_Items = new List<Item>();

				return cont.m_Items;
			}

			CompactInfo info = AcquireCompactInfo();

			if (info.m_Items == null)
				info.m_Items = new List<Item>();

			return info.m_Items;
		}

		#region Mondain's Legacy
		public static Bitmap GetBitmap(int itemID)
		{
			try
			{
				return ArtData.GetStatic(itemID);
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
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}

		private bool GetFlag(ImplFlag flag)
		{
			return (m_Flags & flag) != 0;
		}

		public BounceInfo GetBounce()
		{
			CompactInfo info = LookupCompactInfo();

			if (info != null)
			{
				return info.m_Bounce;
			}

			return null;
		}

		public void RecordBounce(Mobile from, Item parentstack = null)
		{
			CompactInfo info = AcquireCompactInfo();

			info.m_Bounce = new BounceInfo(from, this)
			{
				m_ParentStack = parentstack
			};
		}

		public void ClearBounce()
		{
			CompactInfo info = LookupCompactInfo();

			if (info != null)
			{
				BounceInfo bounce = info.m_Bounce;

				if (bounce != null)
				{
					info.m_Bounce = null;

					if (bounce.m_Parent is Item parentitem)
					{
						if (!parentitem.Deleted)
						{
							parentitem.OnItemBounceCleared(this);
						}
					}
					else if (bounce.m_Parent is Mobile parentmobile)
					{
						if (!parentmobile.Deleted)
						{
							parentmobile.OnItemBounceCleared(this);
						}
					}

					VerifyCompactInfo();
				}
			}
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

		private static readonly Regex m_PluralRegEx = new(@"([^%]+)%([^%/ ]+)(/([^% ]+))*%*([^%]*)", RegexOptions.Compiled | RegexOptions.Singleline);

		public virtual void AppendClickName(StringBuilder sb)
		{
			if (Name == null || Name.Length <= 0)
			{
				bool plural = Amount != 1;

				// bread loa%ves/f%, black pearl%s%, log%s, etc
				Match match = m_PluralRegEx.Match(ItemData.Name);
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
					if (plural && ItemID == 0x0EED)// gold coinS dont ever get the s (unless we put it there <--)
						sb.Append('s');
				}
			}
			else
			{
				sb.Append(Name);
				if (Amount != 1 && ItemID != 0x2006)
					sb.Append('s');
			}
		}

		public virtual void InsertNamePrefix(StringBuilder sb)
		{
			//while ( sb.Length > 0 && sb[0] == ' ' )
			//	sb.Remove( 0, 1 );

			if (Name != null && Name.Length > 0)
				return;

			if (Amount == 1 && sb.Length > 0 && char.IsLetter(sb[0]) && ((ItemData.Flags & TileFlag.ArticleAn) != 0 || (ItemData.Flags & TileFlag.ArticleA) != 0))
			{
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
		}

		public virtual void InstertHtml(StringBuilder sb)
		{
			if (Parent != null)
			{
				string prefix = $"<BIG><BASEFONT COLOR={GetItemRankColor()}>";
				sb.Insert(0, prefix); //big
				sb.Append("</BIG><BASEFONT COLOR=#FFFFFF>"); //big
			}

		}

		public bool AppendLootType(StringBuilder sb)
		{
			if (DisplayLootType && (Name == null || Name.Length <= 0))
			{
				switch (LootType)
				{
					case LootType.Blessed:
						sb.Append("Blessed");
						return true;
					case LootType.Cursed:
						sb.Append("Cursed");
						return true;
				}
			}

			return false;
		}

		public virtual string BuildSingleClick()
		{
			StringBuilder sb = new();

			if (Amount != 1 && ItemID != 0x2006)
				sb.AppendFormat("{0} ", Amount);

			if (AppendLootType(sb))
				sb.Append(' ');
			AppendClickName(sb);
			InsertNamePrefix(sb);
			InstertHtml(sb);

			return sb.ToString();
		}


		public string GetItemRankColor()
		{
			string color = "#FFFFFF";//Default: White

			switch (ItemRank)
			{
				case ItemRank.NotSet: { color = "#FFFFFF"; } break; //White
				case ItemRank.LowQuality: { color = "#9D9D9D"; } break; //Grey
				case ItemRank.Regular: { color = "#FFFFFF"; } break;    //White
				case ItemRank.Crafted: { color = "#0070FF"; } break;    //Blue
				case ItemRank.Resource: { color = "#0070FF"; } break;   //Blue
				case ItemRank.Magic: { color = "#1EFF00"; } break;  //Green
				case ItemRank.Rare: { color = "#A335EE"; } break;   //Purple
				case ItemRank.Unique: { color = "#FF8000"; } break; //Orange
				case ItemRank.Serverbirth: { color = "#E6CC80"; } break;    //Golden
			}
			return color;
		}

		public static string ItemRankColor(ItemRank rank)
		{
			string color = "#FFFFFF";//Default: White

			switch (rank)
			{
				case ItemRank.NotSet: { color = "#FFFFFF"; } break; //White
				case ItemRank.LowQuality: { color = "#9D9D9D"; } break; //Grey
				case ItemRank.Regular: { color = "#FFFFFF"; } break;    //White
				case ItemRank.Crafted: { color = "#0070FF"; } break;    //Blue
				case ItemRank.Resource: { color = "#0070FF"; } break;   //Blue
				case ItemRank.Magic: { color = "#1EFF00"; } break;  //Green
				case ItemRank.Rare: { color = "#A335EE"; } break;   //Purple
				case ItemRank.Unique: { color = "#FF8000"; } break; //Orange
				case ItemRank.Serverbirth: { color = "#E6CC80"; } break;    //Golden
			}
			return color;
		}

		/// <summary>
		/// Overridable. Adds the name of this item to the given <see cref="ObjectPropertyList" />. This method should be overriden if the item requires a complex naming format.
		/// </summary>
		public virtual void AddNameProperty(ObjectPropertyList list)
		{
			if (Core.AOS)
			{
				string name = Name;

				if (name == null)
				{
					if (m_Amount <= 1)
						list.Add(LabelNumber);
					else
						list.Add(1050039, "{0}\t#{1}", m_Amount, LabelNumber); // ~1_NUMBER~ ~2_ITEMNAME~
				}
				else
				{
					if (m_Amount <= 1)
						list.Add(name);
					else
						list.Add(1050039, "{0}\t{1}", m_Amount, Name); // ~1_NUMBER~ ~2_ITEMNAME~
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
			if (m_LootType == LootType.Blessed)
				list.Add(1038021); // blessed
			else if (m_LootType == LootType.Cursed)
				list.Add(1049643); // cursed
			else if (Insured)
				list.Add(1061682); // <b>insured</b>
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

				if (!Movable && !(IsLockedDown || IsSecure) && ItemData.Weight == 255)
				{
					return false;
				}

				return true;
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
		/// Overrideable, used to add crafted by, excpetional, etc properties to items
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
			if (DisplayWeight && Weight > 0)
			{
				int weight = PileWeight + TotalWeight;

				if (weight == 1)
				{
					list.Add(1072788, weight.ToString()); //Weight: ~1_WEIGHT~ stone
				}
				else
				{
					list.Add(1072789, weight.ToString()); //Weight: ~1_WEIGHT~ stones
				}
			}
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
			if (Sockets != null)
			{
				foreach (var socket in Sockets)
				{
					socket.GetProperties(list);
				}
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

			if (Spawner != null)
			{
				Spawner.GetSpawnProperties(this, list);
			}

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
			if (m_Parent is Item item1)
			{
				item1.GetChildProperties(list, item);
			}
			else if (m_Parent is Mobile mobile)
			{
				mobile.GetChildProperties(list, item);
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
			if (m_Parent is Item item1)
			{
				item1.GetChildNameProperties(list, item);
			}
			else if (m_Parent is Mobile mobile)
			{
				mobile.GetChildNameProperties(list, item);
			}
		}

		public virtual bool IsChildVisibleTo(Mobile m, Item child)
		{
			return true;
		}

		public void Bounce(Mobile from)
		{
			if (m_Parent is Item item)
			{
				item.RemoveItem(this);
			}
			else if (m_Parent is Mobile mobile)
			{
				mobile.RemoveItem(this);
			}

			m_Parent = null;

			BounceInfo bounce = GetBounce();

			if (bounce != null)
			{
				var stack = bounce.m_ParentStack;

				if (stack is Item s)
				{
					if (!s.Deleted)
					{
						if (s.IsAccessibleTo(from))
						{
							s.StackWith(from, this);
						}
					}
				}

				var parent = bounce.m_Parent;

				if (parent is Item item1 && !item1.Deleted)
				{
					Item p = item1;
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
				}
				else if (parent is Mobile mobile && !mobile.Deleted)
				{
					if (!mobile.EquipItem(this))
					{
						MoveToWorld(bounce.m_WorldLoc, bounce.m_Map);
					}
				}
				else
				{
					MoveToWorld(bounce.m_WorldLoc, bounce.m_Map);
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
			return m_Layer == layer;
		}

		public bool IsEquipped(Mobile m)
		{
			if (m == null)
				return false;

			Item tocheck = m.FindItemOnLayer(m_Layer);
			if (tocheck == this)
				return true;

			return false;
		}

		public virtual bool CanEquip(Mobile m)
		{
			return m_Layer != Layer.Invalid && m.FindItemOnLayer(m_Layer) == null && CheckEquip(m);
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

			if (m.AccessLevel < AccessLevel.GameMaster && BlessedFor != null && BlessedFor != m)
			{
				m.SendLocalizedMessage(1153882); // You do not own that.

				return false;
			}

			return true;
		}

		public virtual void GetChildContextMenuEntries(Mobile from, List<ContextMenuEntry> list, Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.GetChildContextMenuEntries(from, list, item);
			else if (m_Parent is Mobile mob)
				mob.GetChildContextMenuEntries(from, list, item);
		}

		public virtual void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (m_Parent is Item parentItem)
				parentItem.GetChildContextMenuEntries(from, list, this);
			else if (m_Parent is Mobile mob)
				mob.GetChildContextMenuEntries(from, list, this);
		}

		public virtual bool VerifyMove(Mobile from)
		{
			return Movable;
		}

		public virtual DeathMoveResult OnParentDeath(Mobile parent)
		{
			if (!Movable)
				return DeathMoveResult.RemainEquiped;
			else if (parent.KeepsItemsOnDeath)
				return DeathMoveResult.MoveToBackpack;
			else if (CheckBlessed(parent))
				return DeathMoveResult.MoveToBackpack;
			else if (CheckNewbied() && !parent.Murderer)
				return DeathMoveResult.MoveToBackpack;
			else if (parent.Player && Nontransferable)
				return DeathMoveResult.MoveToBackpack;
			else
				return DeathMoveResult.MoveToCorpse;
		}

		public virtual DeathMoveResult OnInventoryDeath(Mobile parent)
		{
			if (!Movable)
				return DeathMoveResult.MoveToBackpack;
			else if (parent.KeepsItemsOnDeath)
				return DeathMoveResult.MoveToBackpack;
			else if (CheckBlessed(parent))
				return DeathMoveResult.MoveToBackpack;
			else if (CheckNewbied() && !parent.Murderer)
				return DeathMoveResult.MoveToBackpack;
			else if (parent.Player && Nontransferable)
				return DeathMoveResult.MoveToBackpack;
			else
				return DeathMoveResult.MoveToCorpse;
		}

		/// <summary>
		/// Moves the Item to <paramref name="location" />. The Item does not change maps.
		/// </summary>
		public virtual void MoveToWorld(Point3D location)
		{
			MoveToWorld(location, m_Map);
		}

		public void LabelTo(Mobile to, int number)
		{
			_ = to.Send(new MessageLocalized(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, number, "", ""));
		}
		
		public void LabelTo(Mobile to, int hue, int number)
		{
			_ = to.Send(new MessageLocalized(Serial, m_ItemID, MessageType.Label, hue, 3, number, "", ""));
		}

		public void LabelTo(Mobile to, int number, string args)
		{
			_ = to.Send(new MessageLocalized(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, number, "", args));
		}

		public void LabelTo(Mobile to, int hue, int number, string args)
		{
			_ = to.Send(new MessageLocalized(Serial, m_ItemID, MessageType.Label, hue, 3, number, "", args));
		}

		public void LabelTo(Mobile to, string text)
		{
			_ = to.Send(new UnicodeMessage(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, "ENU", "", text));
		}

		public void LabelTo(Mobile to, string format, params object[] args)
		{
			LabelTo(to, string.Format(format, args));
		}

		public void LabelToAffix(Mobile to, int number, AffixType type, string affix)
		{
			_ = to.Send(new MessageLocalizedAffix(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, number, "", type, affix, ""));
		}

		public void LabelToAffix(Mobile to, int hue, int number, AffixType type, string affix)
		{
			_ = to.Send(new MessageLocalizedAffix(Serial, m_ItemID, MessageType.Label, hue, 3, number, "", type, affix, ""));
		}

		public void LabelToAffix(Mobile to, int number, AffixType type, string affix, string args)
		{
			_ = to.Send(new MessageLocalizedAffix(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, number, "", type, affix, args));
		}

		public void LabelToAffix(Mobile to, int hue, int number, AffixType type, string affix, string args)
		{
			_ = to.Send(new MessageLocalizedAffix(Serial, m_ItemID, MessageType.Label, hue, 3, number, "", type, affix, args));
		}

		public virtual void LabelLootTypeTo(Mobile to)
		{
			if (m_LootType == LootType.Blessed)
				LabelTo(to, 1041362); // (blessed)
			else if (m_LootType == LootType.Cursed)
				LabelTo(to, "(cursed)");
		}

		public bool AtWorldPoint(int x, int y)
		{
			return m_Parent == null && m_Location.m_X == x && m_Location.m_Y == y;
		}

		public bool AtPoint(int x, int y)
		{
			return m_Location.m_X == x && m_Location.m_Y == y;
		}

		/// <summary>
		/// Moves the Item to a given <paramref name="location" /> and <paramref name="map" />.
		/// </summary>
		public void MoveToWorld(Point3D location, Map map)
		{
			if (Deleted)
				return;

			Point3D oldLocation = GetWorldLocation();
			Point3D oldRealLocation = m_Location;

			SetLastMoved();

			if (Parent is Mobile mobile)
				mobile.RemoveItem(this);
			else if (Parent is Item item)
				item.RemoveItem(this);

			if (m_Map != map)
			{
				Map old = m_Map;

				if (m_Map != null)
				{
					m_Map.OnLeave(this);

					if (oldLocation.m_X != 0)
					{
						IPooledEnumerable<NetState> eable = m_Map.GetClientsInRange(oldLocation, GetMaxUpdateRange());

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

				m_Location = location;
				OnLocationChange(oldRealLocation);

				ReleaseWorldPackets();

				List<Item> items = LookupItems();

				if (items != null)
				{
					for (int i = 0; i < items.Count; ++i)
						items[i].Map = map;
				}

				m_Map = map;

				if (m_Map != null)
					m_Map.OnEnter(this);

				OnMapChange();

				if (m_Map != null)
				{
					IPooledEnumerable<NetState> eable = m_Map.GetClientsInRange(m_Location, GetMaxUpdateRange());

					foreach (NetState state in eable)
					{
						Mobile m = state.Mobile;

						if (m.CanSee(this) && m.InRange(m_Location, GetUpdateRange(m)))
							SendInfoTo(state);
					}

					eable.Free();
				}

				RemDelta(ItemDelta.Update);

				if (old == null || old == Map.Internal)
					InvalidateProperties();
			}
			else if (m_Map != null)
			{
				IPooledEnumerable<NetState> eable;

				if (oldLocation.m_X != 0)
				{
					eable = m_Map.GetClientsInRange(oldLocation, GetMaxUpdateRange());

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

				Point3D oldInternalLocation = m_Location;

				m_Location = location;
				OnLocationChange(oldRealLocation);

				ReleaseWorldPackets();

				eable = m_Map.GetClientsInRange(m_Location, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (m.CanSee(this) && m.InRange(m_Location, GetUpdateRange(m)))
						SendInfoTo(state);
				}

				eable.Free();

				m_Map.OnMove(oldInternalLocation, this);

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
			get
			{
				return m_ItemRank;
			}
			set
			{
				if (m_ItemRank != value)
				{
					m_ItemRank = value;

				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public LootType LootType
		{
			get => m_LootType;
			set
			{
				if (m_LootType != value)
				{
					m_LootType = value;

					if (DisplayLootType)
						InvalidateProperties();
				}
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

		public virtual bool StackIgnoreItemID => false;
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
				if (m_LootType != dropped.m_LootType)
				{
					m_LootType = LootType.Regular;
				}

				Amount += dropped.Amount;
				dropped.Delete();

				if (playSound && from != null)
				{
					int soundID = GetDropSound();

					if (soundID == -1)
					{
						soundID = 0x42;
					}

					from.SendSound(soundID, GetWorldLocation());
				}

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

			if ((!item.StackIgnoreItemID || !StackIgnoreItemID) && item.ItemID != ItemID)
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
			else if (Sockets != null && item.Sockets != null)
			{
				if (Sockets.Any(s => !item.HasSocket(s.GetType())))
				{
					return false;
				}

				if (item.Sockets.Any(s => !HasSocket(s.GetType())))
				{
					return false;
				}
			}

			return true;
		}

		public virtual bool OnDragDrop(Mobile from, Item dropped)
		{
			if (Parent is Container container)
				return container.OnStackAttempt(from, this, dropped);

			return StackWith(from, dropped);
		}

		public Rectangle2D GetGraphicBounds()
		{
			int itemID = m_ItemID;
			bool doubled = m_Amount > 1;

			if (itemID >= 0xEEA && itemID <= 0xEF2) // Are we coins?
			{
				int coinBase = (itemID - 0xEEA) / 3;
				coinBase *= 3;
				coinBase += 0xEEA;

				doubled = false;

				if (m_Amount <= 1)
				{
					// A single coin
					itemID = coinBase;
				}
				else if (m_Amount <= 5)
				{
					// A stack of coins
					itemID = coinBase + 1;
				}
				else // m_Amount > 5
				{
					// A pile of coins
					itemID = coinBase + 2;
				}
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
				if (m_RemovePacket == null)
				{
					lock (_rpl)
					{
						if (m_RemovePacket == null)
						{
							m_RemovePacket = new RemoveItem(this);
							m_RemovePacket.SetStatic();
						}
					}
				}

				return m_RemovePacket;
			}
		}

		private readonly object _opll = new();
		public Packet OPLPacket
		{
			get
			{
				if (m_OPLPacket == null)
				{
					lock (_opll)
					{
						if (m_OPLPacket == null)
						{
							m_OPLPacket = new OPLInfo(PropertyList);
							m_OPLPacket.SetStatic();
						}
					}
				}

				return m_OPLPacket;
			}
		}

		public ObjectPropertyList PropertyList
		{
			get
			{
				if (m_PropertyList == null)
				{
					m_PropertyList = new ObjectPropertyList(this);

					GetProperties(m_PropertyList);
					AppendChildProperties(m_PropertyList);

					m_PropertyList.Terminate();
					m_PropertyList.SetStatic();
				}

				return m_PropertyList;
			}
		}

		public virtual void AppendChildProperties(ObjectPropertyList list)
		{
			if (m_Parent is Item parentItem)
				parentItem.GetChildProperties(list, this);
			else if (m_Parent is Mobile parentMob)
				parentMob.GetChildProperties(list, this);
		}

		public virtual void AppendChildNameProperties(ObjectPropertyList list)
		{
			if (m_Parent is Item parentItem)
				parentItem.GetChildNameProperties(list, this);
			else if (m_Parent is Mobile parentMob)
				parentMob.GetChildNameProperties(list, this);
		}

		public void ClearProperties()
		{
			Packet.Release(ref m_PropertyList);
			Packet.Release(ref m_OPLPacket);
		}

		public void InvalidateProperties()
		{
			if (!ObjectPropertyList.Enabled)
				return;

			if (m_Map != null && m_Map != Map.Internal && !World.Loading)
			{
				ObjectPropertyList oldList = m_PropertyList;
				m_PropertyList = null;
				ObjectPropertyList newList = PropertyList;

				if (oldList == null || oldList.Hash != newList.Hash)
				{
					Packet.Release(ref m_OPLPacket);
					Delta(ItemDelta.Properties);
				}
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

				if (m_WorldPacket == null)
				{
					lock (_wpl)
					{
						if (m_WorldPacket == null)
						{
							m_WorldPacket = new WorldItem(this);
							m_WorldPacket.SetStatic();
						}
					}
				}

				return m_WorldPacket;
			}
		}

		public Packet WorldPacketSA
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

				if (m_WorldPacketSA == null)
				{
					lock (_wplsa)
					{
						if (m_WorldPacketSA == null)
						{
							m_WorldPacketSA = new WorldItemSA(this);
							m_WorldPacketSA.SetStatic();
						}
					}
				}

				return m_WorldPacketSA;
			}
		}

		public Packet WorldPacketHS
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

				if (m_WorldPacketHS == null)
				{
					lock (_wplhs)
					{
						if (m_WorldPacketHS == null)
						{
							m_WorldPacketHS = new WorldItemHS(this);
							m_WorldPacketHS.SetStatic();
						}
					}
				}

				return m_WorldPacketHS;
			}
		}

		public void ReleaseWorldPackets()
		{
			Packet.Release(ref m_WorldPacket);
			Packet.Release(ref m_WorldPacketSA);
			Packet.Release(ref m_WorldPacketHS);
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

					if (m_Map != null)
					{
						Point3D worldLoc = GetWorldLocation();

						IPooledEnumerable<NetState> eable = m_Map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

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
				if (GetFlag(ImplFlag.Movable) != value)
				{
					SetFlag(ImplFlag.Movable, value);
					ReleaseWorldPackets();
					Delta(ItemDelta.Update);
				}
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
			get => m_Map;
			set
			{
				if (m_Map != value)
				{
					Map old = m_Map;

					if (m_Map != null && m_Parent == null)
					{
						m_Map.OnLeave(this);
						SendRemovePacket();
					}

					List<Item> items = LookupItems();

					if (items != null)
					{
						for (int i = 0; i < items.Count; ++i)
							items[i].Map = value;
					}

					m_Map = value;

					if (m_Map != null && m_Parent == null)
						m_Map.OnEnter(this);

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
			ItemID = 0x00000010,
			Hue = 0x00000020,
			Amount = 0x00000040,
			Layer = 0x00000080,
			Name = 0x00000100,
			Parent = 0x00000200,
			Items = 0x00000400,
			WeightNot1or0 = 0x00000800,
			Map = 0x00001000,
			Visible = 0x00002000,
			Movable = 0x00004000,
			Stackable = 0x00008000,
			WeightIs0 = 0x00010000,
			LocationSByteZ = 0x00020000,
			LocationShortXY = 0x00040000,
			LocationByteXY = 0x00080000,
			ImplFlags = 0x00100000,
			InsuredFor = 0x00200000,
			BlessedFor = 0x00400000,
			HeldBy = 0x00800000,
			IntWeight = 0x01000000,
			SavedFlags = 0x02000000,
			NullWeight = 0x04000000,
			ItemRank = 0x06000000
		}

		int ISerializable.TypeReference => m_TypeRef;

		int ISerializable.SerialIdentity => Serial;

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			SaveFlag flags = SaveFlag.None;

			int x = m_Location.m_X, y = m_Location.m_Y, z = m_Location.m_Z;

			if (x != 0 || y != 0 || z != 0)
			{
				if (x >= short.MinValue && x <= short.MaxValue && y >= short.MinValue && y <= short.MaxValue && z >= sbyte.MinValue && z <= sbyte.MaxValue)
				{
					if (x != 0 || y != 0)
					{
						if (x >= byte.MinValue && x <= byte.MaxValue && y >= byte.MinValue && y <= byte.MaxValue)
							flags |= SaveFlag.LocationByteXY;
						else
							flags |= SaveFlag.LocationShortXY;
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

			if (m_Direction != Direction.North)
				flags |= SaveFlag.Direction;
			if (m_ItemRank != ItemRank.NotSet)
				flags |= SaveFlag.ItemRank;
			if (info != null && info.m_Bounce != null)
				flags |= SaveFlag.Bounce;
			if (m_LootType != LootType.Regular)
				flags |= SaveFlag.LootType;
			if (m_ItemID != 0)
				flags |= SaveFlag.ItemID;
			if (m_Hue != 0)
				flags |= SaveFlag.Hue;
			if (m_Amount != 1)
				flags |= SaveFlag.Amount;
			if (m_Layer != Layer.Invalid)
				flags |= SaveFlag.Layer;
			if (info != null && info.m_Name != null)
				flags |= SaveFlag.Name;
			if (m_Parent != null)
				flags |= SaveFlag.Parent;
			if (items != null && items.Count > 0)
				flags |= SaveFlag.Items;
			if (m_Map != Map.Internal)
				flags |= SaveFlag.Map;
			//if ( m_InsuredFor != null && !m_InsuredFor.Deleted )
			//flags |= SaveFlag.InsuredFor;
			if (info != null && info.m_BlessedFor != null && !info.m_BlessedFor.Deleted)
				flags |= SaveFlag.BlessedFor;
			if (info != null && info.m_HeldBy != null && !info.m_HeldBy.Deleted)
				flags |= SaveFlag.HeldBy;
			if (info != null && info.m_SavedFlags != 0)
				flags |= SaveFlag.SavedFlags;

			if (info == null || info.m_Weight == -1)
			{
				flags |= SaveFlag.NullWeight;
			}
			else
			{
				if (info.m_Weight == 0.0)
				{
					flags |= SaveFlag.WeightIs0;
				}
				else if (info.m_Weight != 1.0)
				{
					if (info.m_Weight == (int)info.m_Weight)
						flags |= SaveFlag.IntWeight;
					else
						flags |= SaveFlag.WeightNot1or0;
				}
			}

			ImplFlag implFlags = (m_Flags & (ImplFlag.Visible | ImplFlag.Movable | ImplFlag.Stackable | ImplFlag.Insured | ImplFlag.PayedInsurance | ImplFlag.QuestItem));

			if (implFlags != (ImplFlag.Visible | ImplFlag.Movable))
				flags |= SaveFlag.ImplFlags;

			writer.Write((int)flags);

			/* begin last moved time optimization */
			long ticks = LastMoved.Ticks;
			long now = DateTime.UtcNow.Ticks;

			TimeSpan d;

			try { d = new TimeSpan(ticks - now); }
			catch { if (ticks < now) d = TimeSpan.MaxValue; else d = TimeSpan.MaxValue; }

			double minutes = -d.TotalMinutes;

			if (minutes < int.MinValue)
				minutes = int.MinValue;
			else if (minutes > int.MaxValue)
				minutes = int.MaxValue;

			writer.WriteEncodedInt((int)minutes);
			/* end */

			if (flags.HasFlag(SaveFlag.Direction))
				writer.Write((byte)m_Direction);

			if (flags.HasFlag(SaveFlag.ItemRank))
				writer.Write((byte)m_ItemRank);

			if (flags.HasFlag(SaveFlag.Bounce))
				BounceInfo.Serialize(info.m_Bounce, writer);

			if (flags.HasFlag(SaveFlag.LootType))
				writer.Write((byte)m_LootType);

			if (flags.HasFlag(SaveFlag.LocationFull))
			{
				writer.WriteEncodedInt(x);
				writer.WriteEncodedInt(y);
				writer.WriteEncodedInt(z);
			}
			else
			{
				if (flags.HasFlag(SaveFlag.LocationByteXY))
				{
					writer.Write((byte)x);
					writer.Write((byte)y);
				}
				else if (flags.HasFlag(SaveFlag.LocationShortXY))
				{
					writer.Write((short)x);
					writer.Write((short)y);
				}

				if (flags.HasFlag(SaveFlag.LocationSByteZ))
					writer.Write((sbyte)z);
			}

			if (flags.HasFlag(SaveFlag.ItemID))
				writer.WriteEncodedInt(m_ItemID);

			if (flags.HasFlag(SaveFlag.Hue))
				writer.WriteEncodedInt(m_Hue);

			if (flags.HasFlag(SaveFlag.Amount))
				writer.WriteEncodedInt(m_Amount);

			if (flags.HasFlag(SaveFlag.Layer))
				writer.Write((byte)m_Layer);

			if (flags.HasFlag(SaveFlag.Name))
				writer.Write(info.m_Name);

			if (flags.HasFlag(SaveFlag.Parent))
			{
				if (m_Parent != null && !m_Parent.Deleted)
					writer.Write(m_Parent.Serial);
				else
					writer.Write(Serial.MinusOne);
			}

			if (flags.HasFlag(SaveFlag.Items))
				writer.Write(items, false);

			if (flags.HasFlag(SaveFlag.IntWeight))
				writer.WriteEncodedInt((int)info.m_Weight);
			else if (flags.HasFlag(SaveFlag.WeightNot1or0))
				writer.Write(info.m_Weight);

			if (flags.HasFlag(SaveFlag.Map))
				writer.Write(m_Map);

			if (flags.HasFlag(SaveFlag.ImplFlags))
				writer.WriteEncodedInt((int)implFlags);

			if (flags.HasFlag(SaveFlag.InsuredFor))
				writer.Write((Mobile)null);

			if (flags.HasFlag(SaveFlag.BlessedFor))
				writer.Write(info.m_BlessedFor);

			if (flags.HasFlag(SaveFlag.HeldBy))
				writer.Write(info.m_HeldBy);

			if (flags.HasFlag(SaveFlag.SavedFlags))
				writer.WriteEncodedInt(info.m_SavedFlags);
		}

		public IPooledEnumerable<IEntity> GetObjectsInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Server.Map.NullEnumerable<IEntity>.Instance;

			if (m_Parent == null)
				return map.GetObjectsInRange(m_Location, range);

			return map.GetObjectsInRange(GetWorldLocation(), range);
		}

		public IPooledEnumerable<Item> GetItemsInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Server.Map.NullEnumerable<Item>.Instance;

			if (m_Parent == null)
				return map.GetItemsInRange(m_Location, range);

			return map.GetItemsInRange(GetWorldLocation(), range);
		}

		public IPooledEnumerable<Mobile> GetMobilesInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Server.Map.NullEnumerable<Mobile>.Instance;

			if (m_Parent == null)
				return map.GetMobilesInRange(m_Location, range);

			return map.GetMobilesInRange(GetWorldLocation(), range);
		}

		public IPooledEnumerable<NetState> GetClientsInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Server.Map.NullEnumerable<NetState>.Instance;

			if (m_Parent == null)
				return map.GetClientsInRange(m_Location, range);

			return map.GetClientsInRange(GetWorldLocation(), range);
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

			return ((info.m_TempFlags & flag) != 0);
		}

		public void SetTempFlag(int flag, bool value)
		{
			CompactInfo info = AcquireCompactInfo();

			if (value)
				info.m_TempFlags |= flag;
			else
				info.m_TempFlags &= ~flag;

			if (info.m_TempFlags == 0)
				VerifyCompactInfo();
		}

		public bool GetSavedFlag(int flag)
		{
			CompactInfo info = LookupCompactInfo();

			if (info == null)
				return false;

			return ((info.m_SavedFlags & flag) != 0);
		}

		public void SetSavedFlag(int flag, bool value)
		{
			CompactInfo info = AcquireCompactInfo();

			if (value)
				info.m_SavedFlags |= flag;
			else
				info.m_SavedFlags &= ~flag;

			if (info.m_SavedFlags == 0)
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
							m_Direction = (Direction)reader.ReadByte();

						if (flags.HasFlag(SaveFlag.ItemRank))
							m_ItemRank = (ItemRank)reader.ReadByte();

						if (flags.HasFlag(SaveFlag.Bounce))
							AcquireCompactInfo().m_Bounce = BounceInfo.Deserialize(reader);

						if (flags.HasFlag(SaveFlag.LootType))
							m_LootType = (LootType)reader.ReadByte();

						int x = 0, y = 0, z = 0;

						if (flags.HasFlag(SaveFlag.LocationFull))
						{
							x = reader.ReadEncodedInt();
							y = reader.ReadEncodedInt();
							z = reader.ReadEncodedInt();
						}
						else
						{
							if (flags.HasFlag(SaveFlag.LocationByteXY))
							{
								x = reader.ReadByte();
								y = reader.ReadByte();
							}
							else if (flags.HasFlag(SaveFlag.LocationShortXY))
							{
								x = reader.ReadShort();
								y = reader.ReadShort();
							}

							if (flags.HasFlag(SaveFlag.LocationSByteZ))
								z = reader.ReadSByte();
						}

						m_Location = new Point3D(x, y, z);

						if (flags.HasFlag(SaveFlag.ItemID))
							m_ItemID = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Hue))
							m_Hue = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Amount))
							m_Amount = reader.ReadEncodedInt();
						else
							m_Amount = 1;

						if (flags.HasFlag(SaveFlag.Layer))
							m_Layer = (Layer)reader.ReadByte();

						if (flags.HasFlag(SaveFlag.Name))
						{
							string name = reader.ReadString();

							if (name != DefaultName)
								AcquireCompactInfo().m_Name = name;
						}

						if (flags.HasFlag(SaveFlag.Parent))
						{
							Serial parent = reader.ReadInt();

							if (parent.IsMobile)
								m_Parent = World.FindMobile(parent);
							else if (parent.IsItem)
								m_Parent = World.FindItem(parent);
							else
								m_Parent = null;

							if (m_Parent == null && (parent.IsMobile || parent.IsItem))
								Delete();
						}

						if (flags.HasFlag(SaveFlag.Items))
						{
							List<Item> items = reader.ReadStrongItemList();

							if (this is Container)
								(this as Container).m_Items = items;
							else
								AcquireCompactInfo().m_Items = items;
						}

						if (!flags.HasFlag(SaveFlag.NullWeight))
						{
							double weight;

							if (flags.HasFlag(SaveFlag.IntWeight))
								weight = reader.ReadEncodedInt();
							else if (flags.HasFlag(SaveFlag.WeightNot1or0))
								weight = reader.ReadDouble();
							else if (flags.HasFlag(SaveFlag.WeightIs0))
								weight = 0.0;
							else
								weight = 1.0;

							if (weight != DefaultWeight)
								AcquireCompactInfo().m_Weight = weight;
						}

						if (flags.HasFlag(SaveFlag.Map))
							m_Map = reader.ReadMap();
						else
							m_Map = Map.Internal;

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
							m_Flags = (ImplFlag)reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.InsuredFor))
							/*m_InsuredFor = */
							_ = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.BlessedFor))
							AcquireCompactInfo().m_BlessedFor = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.HeldBy))
							AcquireCompactInfo().m_HeldBy = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.SavedFlags))
							AcquireCompactInfo().m_SavedFlags = reader.ReadEncodedInt();

						if (m_Map != null && m_Parent == null)
							m_Map.OnEnter(this);

						break;
					}
			}

			if (HeldBy != null)
				_ = Timer.DelayCall(TimeSpan.Zero, new TimerCallback(FixHolding_Sandbox));

			//if ( version < 9 )
			VerifyCompactInfo();
		}

		private void FixHolding_Sandbox()
		{
			Mobile heldBy = HeldBy;

			if (heldBy != null)
			{
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
				state.Send(OPLPacket);
			}
		}

		protected virtual Packet GetWorldPacketFor(NetState state)
		{
			if (state.HighSeas)
				return WorldPacketHS;
			else if (state.StygianAbyss)
				return WorldPacketSA;
			else
				return WorldPacket;
		}

		public virtual bool IsVirtualItem => false;

		public virtual int GetTotal(TotalType type)
		{
			return 0;
		}

		public virtual void UpdateTotal(Item sender, TotalType type, int delta)
		{
			if (!IsVirtualItem)
			{
				if (m_Parent is Item)
					(m_Parent as Item).UpdateTotal(sender, type, delta);
				else if (m_Parent is Mobile)
					(m_Parent as Mobile).UpdateTotal(sender, type, delta);
				else if (HeldBy != null)
					HeldBy.UpdateTotal(sender, type, delta);
			}
		}

		public virtual void UpdateTotals()
		{
		}

		public virtual int LabelNumber
		{
			get
			{
				if (m_ItemID < 0x4000)
					return 1020000 + m_ItemID;
				else
					return 1078872 + m_ItemID;
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
				if (m_ItemID < 0 || m_ItemID > TileData.MaxItemValue || this is BaseMulti)
					return 0;

				int weight = TileData.ItemTable[m_ItemID].Weight;

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

				if (info != null && info.m_Weight != -1)
					return info.m_Weight;

				return DefaultWeight;
			}
			set
			{
				if (Weight != value)
				{
					CompactInfo info = AcquireCompactInfo();

					int oldPileWeight = PileWeight;

					info.m_Weight = value;

					if (info.m_Weight == -1)
						VerifyCompactInfo();

					int newPileWeight = PileWeight;

					UpdateTotal(this, TotalType.Weight, newPileWeight - oldPileWeight);

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int PileWeight => (int)Math.Ceiling(Weight * Amount);

		public virtual int HuedItemID => m_ItemID;

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public virtual int Hue
		{
			get => m_Hue;
			set
			{
				if (m_Hue != value)
				{
					m_Hue = value;
					ReleaseWorldPackets();

					Delta(ItemDelta.Update);
				}
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
			get => m_Layer;
			set
			{
				if (m_Layer != value)
				{
					m_Layer = value;

					Delta(ItemDelta.EquipOnly);
				}
			}
		}

		public List<Item> Items
		{
			get
			{
				List<Item> items = LookupItems();

				if (items == null)
					items = EmptyItems;

				return items;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public IEntity RootParent
		{
			get
			{
				IEntity p = m_Parent;
				while (p is Item item)
				{
					if (item.m_Parent == null)
					{
						break;
					}
					else
					{
						p = item.m_Parent;
					}
				}

				return p;
			}
		}

		public bool ParentsContain<T>() where T : Item
		{
			IEntity p = m_Parent;

			while (p is Item item)
			{
				if (p is T)
					return true;

				if (item.m_Parent == null)
				{
					break;
				}
				else
				{
					p = item.m_Parent;
				}
			}

			return false;
		}

		public virtual void AddItem(Item item)
		{
			if (item == null || item.Deleted || item.m_Parent == this)
			{
				return;
			}
			else if (item == this)
			{
				Console.WriteLine("Warning: Adding item to itself: [0x{0:X} {1}].AddItem( [0x{2:X} {3}] )", Serial.Value, GetType().Name, item.Serial.Value, item.GetType().Name);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return;
			}
			else if (IsChildOf(item))
			{
				Console.WriteLine("Warning: Adding parent item to child: [0x{0:X} {1}].AddItem( [0x{2:X} {3}] )", Serial.Value, GetType().Name, item.Serial.Value, item.GetType().Name);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return;
			}
			else if (item.m_Parent is Mobile parentMob)
			{
				parentMob.RemoveItem(item);
			}
			else if (item.m_Parent is Item parentItem)
			{
				parentItem.RemoveItem(item);
			}
			else
			{
				item.SendRemovePacket();
			}

			item.Parent = this;
			item.Map = m_Map;

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

		private static readonly List<Item> m_DeltaQueue = new();

		public void Delta(ItemDelta flags)
		{
			if (m_Map == null || m_Map == Map.Internal)
				return;

			m_DeltaFlags |= flags;

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
					catch { }
				}
				else
				{
					m_DeltaQueue.Add(this);
				}
			}

			Core.Set();
		}

		public void RemDelta(ItemDelta flags)
		{
			m_DeltaFlags &= ~flags;

			if (GetFlag(ImplFlag.InQueue) && m_DeltaFlags == ItemDelta.None)
			{
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
					catch { }
				}
				else
				{
					_ = m_DeltaQueue.Remove(this);
				}
			}
		}

		public bool NoMoveHS { get; set; }

		public void ProcessDelta()
		{
			ItemDelta flags = m_DeltaFlags;

			SetFlag(ImplFlag.InQueue, false);
			m_DeltaFlags = ItemDelta.None;

			Map map = m_Map;

			if (map != null && !Deleted)
			{
				bool sendOPLUpdate = ObjectPropertyList.Enabled && (flags & ItemDelta.Properties) != 0;

				if (m_Parent is Container contParent && !contParent.IsPublicContainer)
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
										ns.Send(OPLPacket);
								}
							}
						}

						SecureTradeContainer stc = GetSecureTradeCont();

						if (stc != null)
						{
							SecureTrade st = stc.Trade;

							if (st != null)
							{
								Mobile test = st.From.Mobile;

								if (test != null && test != rootParent)
									tradeRecip = test;

								test = st.To.Mobile;

								if (test != null && test != rootParent)
									tradeRecip = test;

								if (tradeRecip != null)
								{
									NetState ns = tradeRecip.NetState;

									if (ns != null)
									{
										if (tradeRecip.CanSee(this) && tradeRecip.InRange(worldLoc, GetUpdateRange(tradeRecip)))
										{
											if (ns.ContainerGridLines)
												ns.Send(new ContainerContentUpdate6017(this));
											else
												ns.Send(new ContainerContentUpdate(this));

											if (ObjectPropertyList.Enabled)
												ns.Send(OPLPacket);
										}
									}
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

										if (ns != null)
										{
											if (mob.CanSee(this))
											{
												if (ns.ContainerGridLines)
													ns.Send(new ContainerContentUpdate6017(this));
												else
													ns.Send(new ContainerContentUpdate(this));

												if (ObjectPropertyList.Enabled)
													ns.Send(OPLPacket);
											}
										}
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

						if (m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
						{
							if (m_Parent == null)
							{
								SendInfoTo(state, ObjectPropertyList.Enabled);
							}
							else
							{
								if (p == null)
								{
									if (m_Parent is Item)
									{
										if (state.ContainerGridLines)
											state.Send(new ContainerContentUpdate6017(this));
										else
											state.Send(new ContainerContentUpdate(this));
									}
									else if (m_Parent is Mobile)
									{
										p = new EquipUpdate(this);
										p.Acquire();

										state.Send(p);
									}
								}
								else
								{
									state.Send(p);
								}

								if (ObjectPropertyList.Enabled)
								{
									state.Send(OPLPacket);
								}
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
					if (m_Parent is Mobile)
					{
						Packet p = null;
						Point3D worldLoc = GetWorldLocation();

						IPooledEnumerable<NetState> eable = map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

						foreach (NetState state in eable)
						{
							Mobile m = state.Mobile;

							if (m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
							{
								//if ( sendOPLUpdate )
								//	state.Send( RemovePacket );

								if (p == null)
									p = Packet.Acquire(new EquipUpdate(this));

								state.Send(p);

								if (ObjectPropertyList.Enabled)
									state.Send(OPLPacket);
							}
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
							state.Send(OPLPacket);
					}

					eable.Free();
				}
			}
		}

		private static bool _processing = false;

		public static void ProcessDeltaQueue()
		{
			_processing = true;

			if (m_DeltaQueue.Count >= 512)
			{
				_ = Parallel.ForEach(m_DeltaQueue, i => i.ProcessDelta());
			}
			else
			{
				for (int i = 0; i < m_DeltaQueue.Count; i++) m_DeltaQueue[i].ProcessDelta();
			}

			m_DeltaQueue.Clear();

			_processing = false;
		}

		public virtual void OnDelete()
		{
			if (Spawner != null)
			{
				Spawner.Remove(this);
				Spawner = null;
			}
		}

		public virtual void OnParentDeleted(IEntity parent)
		{
			Delete();
		}

		public virtual void FreeCache()
		{
			ReleaseWorldPackets();
			Packet.Release(ref m_RemovePacket);
			Packet.Release(ref m_OPLPacket);
			Packet.Release(ref m_PropertyList);
		}

		public virtual void Delete()
		{
			if (Deleted)
				return;
			else if (!World.OnDelete(this))
				return;

			OnDelete();

			List<Item> items = LookupItems();

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

			if (Parent is Mobile parentMob)
				parentMob.RemoveItem(this);
			else if (Parent is Item parentItem)
				parentItem.RemoveItem(this);

			ClearBounce();

			if (m_Map != null)
			{
				if (m_Parent == null)
					m_Map.OnLeave(this);
				m_Map = null;
			}

			World.RemoveItem(this);

			OnAfterDelete();

			FreeCache();
		}

		public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			if (m_Map != null)
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable<NetState> eable = m_Map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
					{
						if (p == null)
						{
							if (ascii)
								p = new AsciiMessage(Serial, m_ItemID, type, hue, 3, Name, text);
							else
								p = new UnicodeMessage(Serial, m_ItemID, type, hue, 3, "ENU", Name, text);

							p.Acquire();
						}

						state.Send(p);
					}
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
			if (m_Map != null)
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable<NetState> eable = m_Map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
					{
						if (p == null)
							p = Packet.Acquire(new MessageLocalized(Serial, m_ItemID, type, hue, 3, number, Name, args));

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, NetState state, string args = "")
		{
			if (Map != null && state != null)
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				Mobile m = state.Mobile;

				if (m != null && m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
				{
					if (p == null)
						p = Packet.Acquire(new MessageLocalized(Serial, m_ItemID, type, hue, 3, number, Name, args));

					state.Send(p);
				}

				Packet.Release(p);
			}
		}

		public void PrivateOverheadMessage(MessageType type, int hue, bool ascii, string text, NetState state)
		{
			if (Map != null && state != null)
			{
				Point3D worldLoc = GetWorldLocation();
				Mobile m = state.Mobile;

				Packet asciip = null;
				Packet p = null;

				if (m != null && m.CanSee(this) && m.InRange(worldLoc, GetUpdateRange(m)))
				{
					if (ascii)
					{
						if (asciip == null)
							asciip = Packet.Acquire(new AsciiMessage(Serial, m_ItemID, type, hue, 3, Name, text));

						state.Send(asciip);
					}
					else
					{
						if (p == null)
							p = Packet.Acquire(new UnicodeMessage(Serial, m_ItemID, type, hue, 3, m.Language, Name, text));

						state.Send(p);
					}
				}

				Packet.Release(asciip);
				Packet.Release(p);
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

		public bool InLOS(Point3D target)
		{
			if (Deleted || Map == null || Parent != null)
				return false;

			return Map.LineOfSight(this, target);
		}

		public virtual void OnAfterDelete()
		{
			EventSink.InvokeOnItemDeleted(this);
		}

		public virtual void RemoveItem(Item item)
		{
			List<Item> items = LookupItems();

			if (items != null && items.Contains(item))
			{
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

				if (info != null)
					return info.m_Spawner;

				return null;

			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.m_Spawner = value;

				if (info.m_Spawner == null)
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

		[CommandProperty(AccessLevel.Counselor)]
		public Serial Serial { get; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Developer)]
		public IEntity ParentEntity => Parent;

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Developer)]
		public IEntity RootParentEntity => RootParent;

		#region Location Location Location!

		public virtual void OnLocationChange(Point3D oldLocation)
		{
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual Point3D Location
		{
			get => m_Location;
			set
			{
				Point3D oldLocation = m_Location;

				if (oldLocation != value)
				{
					if (m_Map != null)
					{
						if (m_Parent == null)
						{
							IPooledEnumerable<NetState> eable;

							if (m_Location.m_X != 0)
							{
								eable = m_Map.GetClientsInRange(oldLocation, GetMaxUpdateRange());

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

							Point3D oldLoc = m_Location;
							m_Location = value;
							ReleaseWorldPackets();

							SetLastMoved();

							eable = m_Map.GetClientsInRange(m_Location, GetMaxUpdateRange());

							foreach (NetState state in eable)
							{
								Mobile m = state.Mobile;

								if (m.CanSee(this) && m.InRange(m_Location, GetUpdateRange(m)) && (!state.HighSeas || !NoMoveHS || (m_DeltaFlags & ItemDelta.Update) != 0 || !m.InRange(oldLoc, GetUpdateRange(m))))
									SendInfoTo(state);
							}

							eable.Free();

							RemDelta(ItemDelta.Update);
						}
						else if (m_Parent is Item)
						{
							m_Location = value;
							ReleaseWorldPackets();

							Delta(ItemDelta.Update);
						}
						else
						{
							m_Location = value;
							ReleaseWorldPackets();
						}

						if (m_Parent == null)
							m_Map.OnMove(oldLocation, this);
					}
					else
					{
						m_Location = value;
						ReleaseWorldPackets();
					}

					OnLocationChange(oldLocation);
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int X
		{
			get => m_Location.m_X;
			set => Location = new Point3D(value, m_Location.m_Y, m_Location.m_Z);
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Y
		{
			get => m_Location.m_Y;
			set => Location = new Point3D(m_Location.m_X, value, m_Location.m_Z);
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Z
		{
			get => m_Location.m_Z;
			set => Location = new Point3D(m_Location.m_X, m_Location.m_Y, value);
		}
		#endregion

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ItemID
		{
			get => m_ItemID;
			set
			{
				if (m_ItemID != value)
				{
					int oldPileWeight = PileWeight;

					m_ItemID = value;
					ReleaseWorldPackets();

					int newPileWeight = PileWeight;

					UpdateTotal(this, TotalType.Weight, newPileWeight - oldPileWeight);

					InvalidateProperties();
					Delta(ItemDelta.Update);
				}
			}
		}

		public virtual string DefaultName => null;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Name
		{
			get
			{
				CompactInfo info = LookupCompactInfo();

				if (info != null && info.m_Name != null)
					return info.m_Name;

				return DefaultName;
			}
			set
			{
				if (value == null || value != DefaultName)
				{
					CompactInfo info = AcquireCompactInfo();

					info.m_Name = value;

					if (info.m_Name == null)
						VerifyCompactInfo();

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Developer)]
		public IEntity Parent
		{
			get => m_Parent;
			set
			{
				if (m_Parent == value)
					return;

				IEntity oldParent = m_Parent;

				m_Parent = value;

				if (m_Map != null)
				{
					if (oldParent != null && m_Parent == null)
						m_Map.OnEnter(this);
					else if (m_Parent != null)
						m_Map.OnLeave(this);
				}

				if (!World.Loading)
				{
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
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public LightType Light
		{
			get => (LightType)m_Direction;
			set
			{
				if ((LightType)m_Direction != value)
				{
					m_Direction = (Direction)value;
					ReleaseWorldPackets();

					Delta(ItemDelta.Update);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Direction
		{
			get => m_Direction;
			set
			{
				if (m_Direction != value)
				{
					m_Direction = value;
					ReleaseWorldPackets();

					Delta(ItemDelta.Update);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Amount
		{
			get => m_Amount;
			set
			{
				int oldValue = m_Amount;

				if (oldValue != value)
				{
					int oldPileWeight = PileWeight;

					m_Amount = value;
					ReleaseWorldPackets();

					int newPileWeight = PileWeight;

					UpdateTotal(this, TotalType.Weight, newPileWeight - oldPileWeight);

					OnAmountChange(oldValue);

					Delta(ItemDelta.Update);

					if (oldValue > 1 || value > 1)
						InvalidateProperties();

					if (!Stackable && m_Amount > 1)
						Console.WriteLine("Warning: 0x{0:X}: Amount changed for non-stackable item '{2}'. ({1})", Serial.Value, m_Amount, GetType().Name);
				}
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
			else if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(target.Location, 2))
				return false;
			else if (!from.CanSee(target) || !from.InLOS(target))
				return false;
			else if (!from.OnDroppedItemToMobile(this, target))
				return false;
			else if (!OnDroppedToMobile(from, target))
				return false;
			else if (!target.OnDragDrop(from, this))
				return false;
			else
				return true;
		}

		public virtual bool OnDroppedInto(Mobile from, Container target, Point3D p)
		{
			if (!from.OnDroppedItemInto(this, target, p))
			{
				return false;
			}
			else if (Nontransferable && from.Player && target != from.Backpack)
			{
				HandleInvalidTransfer(from);
				return false;
			}

			return target.OnDragDropInto(from, this, p);
		}

		public virtual bool OnDroppedOnto(Mobile from, Item target)
		{
			if (Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null)
				return false;
			else if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(target.GetWorldLocation(), 2))
				return false;
			else if (!from.CanSee(target) || !from.InLOS(target))
				return false;
			else if (!target.IsAccessibleTo(from))
				return false;
			else if (!from.OnDroppedItemOnto(this, target))
				return false;
			else if (Nontransferable && from.Player && target != from.Backpack)
			{
				HandleInvalidTransfer(from);
				return false;
			}
			else
				return target.OnDragDrop(from, this);
		}

		public virtual bool DropToItem(Mobile from, Item target, Point3D p)
		{
			if (Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null)
				return false;

			object root = target.RootParent;

			if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(target.GetWorldLocation(), 2))
				return false;
			else if (!from.CanSee(target) || !from.InLOS(target))
				return false;
			else if (!target.IsAccessibleTo(from))
				return false;
			else if (root is Mobile mobile && !mobile.CheckNonlocalDrop(from, this, target))
				return false;
			else if (!from.OnDroppedItemToItem(this, target, p))
				return false;
			else if (target is Container container && p.m_X != -1 && p.m_Y != -1)
				return OnDroppedInto(from, container, p);
			else
				return OnDroppedOnto(from, target);
		}

		public virtual bool OnDroppedToWorld(Mobile from, Point3D p)
		{
			if (Nontransferable && from.Player)
			{
				HandleInvalidTransfer(from);
				return false;
			}

			return true;
		}

		public virtual int GetLiftSound(Mobile from)
		{
			return 0x57;
		}

		private static int m_OpenSlots;

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
			TileFlag landFlags = TileData.LandTable[landTile.ID & TileData.MaxLandValue].Flags;

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
				ItemData id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

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
				if (item is BaseMulti || item.ItemID > TileData.MaxItemValue)
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

			m_OpenSlots = (1 << 20) - 1;

			int surfaceZ = z;

			for (int i = 0; i < tiles.Length; ++i)
			{
				StaticTile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

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

				m_OpenSlots &= ~(((1 << bitCount) - 1) << zStart);
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

				m_OpenSlots &= ~(((1 << bitCount) - 1) << zStart);
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

				okay = ((m_OpenSlots >> i) & match) == match;

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

			if (landAvg > z && (z + height) > landZ)
				return false;
			else if ((landFlags & TileFlag.Impassable) != 0 && landAvg > surfaceZ && (z + height) > landZ)
				return false;

			for (int i = 0; i < tiles.Length; ++i)
			{
				StaticTile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

				int checkZ = tile.Z;
				int checkTop = checkZ + id.CalcHeight;

				if (checkTop > z && (z + height) > checkZ)
					return false;
				else if ((id.Surface || id.Impassable) && checkTop > surfaceZ && (z + height) > checkZ)
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
			else if (!from.OnDroppedItemToWorld(this, p))
				return false;
			else if (!OnDroppedToWorld(from, p))
				return false;

			int soundID = GetDropSound();

			MoveToWorld(p, from.Map);

			from.SendSound(soundID == -1 ? 0x42 : soundID, GetWorldLocation());

			return true;
		}

		public void SendRemovePacket()
		{
			if (!Deleted && m_Map != null)
			{
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable<NetState> eable = m_Map.GetClientsInRange(worldLoc, GetMaxUpdateRange());

				foreach (NetState state in eable)
				{
					Mobile m = state.Mobile;

					if (m.InRange(worldLoc, GetUpdateRange(m)))
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

			if (root == null)
				return m_Location;
			else
				return root.Location;

			//return root == null ? m_Location : new Point3D( (IPoint3D) root );
		}

		public virtual bool BlocksFit => false;

		public Point3D GetSurfaceTop()
		{
			IEntity root = RootParentEntity;

			if (root == null)
				return new Point3D(m_Location.m_X, m_Location.m_Y, m_Location.m_Z + (ItemData.Surface ? ItemData.CalcHeight : 0));
			else
				return root.Location;
		}

		public Point3D GetWorldTop()
		{
			IEntity root = RootParentEntity;

			if (root == null)
				return new Point3D(m_Location.m_X, m_Location.m_Y, m_Location.m_Z + ItemData.CalcHeight);
			else
				return root.Location;
		}

		public void SendLocalizedMessageTo(Mobile to, int number)
		{
			if (Deleted || !to.CanSee(this))
				return;

			_ = to.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, DisplayColor, 3, number, "", ""));
		}

		public void SendLocalizedMessageTo(Mobile to, int number, string args)
		{
			if (Deleted || !to.CanSee(this))
				return;

			_ = to.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, DisplayColor, 3, number, "", args));
		}

		public void SendLocalizedMessageTo(Mobile to, int number, AffixType affixType, string affix, string args)
		{
			if (Deleted || !to.CanSee(this))
				return;

			_ = to.Send(new MessageLocalizedAffix(Serial, ItemID, MessageType.Regular, DisplayColor, 3, number, "", affixType, affix, args));
		}

		public void SendLocalizedMessage(int number, string args)
		{
			if (Deleted || Map == null)
			{
				return;
			}

			IPooledEnumerable eable = Map.GetClientsInRange(Location, Map.GlobalMaxUpdateRange);
			Packet p = Packet.Acquire(new MessageLocalized(Serial, m_ItemID, MessageType.Regular, DisplayColor, 3, number, Name, args));

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
			Packet p = Packet.Acquire(new MessageLocalizedAffix(Serial, m_ItemID, type, DisplayColor, 3, number, "", affixType, affix, args));

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

				p = item.m_Parent;
			}

			return null;
		}

		public virtual void OnItemAdded(Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnSubItemAdded(item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnSubItemAdded(item);
		}

		public virtual void OnItemRemoved(Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnSubItemRemoved(item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnSubItemRemoved(item);
		}

		public virtual void OnSubItemAdded(Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnSubItemAdded(item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnSubItemAdded(item);
		}

		public virtual void OnSubItemRemoved(Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnSubItemRemoved(item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnSubItemRemoved(item);
		}

		public virtual void OnItemBounceCleared(Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnSubItemBounceCleared(item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnSubItemBounceCleared(item);
		}

		public virtual void OnSubItemBounceCleared(Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnSubItemBounceCleared(item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnSubItemBounceCleared(item);
		}

		public virtual bool CheckTarget(Mobile from, Targeting.Target targ, object targeted)
		{
			if (m_Parent is Item parentItem)
				return parentItem.CheckTarget(from, targ, targeted);
			else if (m_Parent is Mobile parentMob)
				return parentMob.CheckTarget(from, targ, targeted);

			return true;
		}

		public virtual void OnStatsQuery(Mobile m)
		{
			if (m == null || m.Deleted || m.Map != Map || m.NetState == null)
			{
				return;
			}

			if (Utility.InUpdateRange(m, this) && m.CanSee(this))
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
			return m != null && m.AccessLevel >= AccessLevel.GameMaster;
		}

		public virtual bool IsAccessibleTo(Mobile check)
		{
			if (m_Parent is Item parentItem)
				return parentItem.IsAccessibleTo(check);

			Region reg = Region.Find(GetWorldLocation(), m_Map);

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
			IEntity p = m_Parent;

			if ((p == null || o == null) && !allowNull)
				return false;

			if (p == o)
				return true;

			while (p is Item item)
			{
				if (item.m_Parent == null)
				{
					break;
				}
				else
				{
					p = item.m_Parent;

					if (p == o)
						return true;
				}
			}

			return false;
		}

		public ItemData ItemData => TileData.ItemTable[m_ItemID & TileData.MaxItemValue];

		public virtual void OnItemUsed(Mobile from, Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnItemUsed(from, item);
			else if (m_Parent is Mobile parentMob)
				parentMob.OnItemUsed(from, item);
		}

		public bool CheckItemUse(Mobile from)
		{
			return CheckItemUse(from, this);
		}

		public virtual bool CheckItemUse(Mobile from, Item item)
		{
			if (m_Parent is Item parentItem)
				return parentItem.CheckItemUse(from, item);
			else if (m_Parent is Mobile parentMob)
				return parentMob.CheckItemUse(from, item);
			else
				return true;
		}

		public virtual void OnItemLifted(Mobile from, Item item)
		{
			if (m_Parent is Item parentItem)
				parentItem.OnItemLifted(from, item);
			else if (m_Parent is Mobile parentMobile)
				parentMobile.OnItemLifted(from, item);
		}

		public bool CheckLift(Mobile from)
		{
			LRReason reject = LRReason.Inspecific;

			return CheckLift(from, this, ref reject);
		}

		public virtual bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			if (m_Parent is Item parentItem)
				return parentItem.CheckLift(from, item, ref reject);
			else if (m_Parent is Mobile parentMobile)
				return parentMobile.CheckLift(from, item, ref reject);
			else
				return true;
		}

		public virtual bool CanTarget => true;
		public virtual bool DisplayLootType => true;

		public virtual void OnSingleClickContained(Mobile from, Item item)
		{
			if (m_Parent is Item parent)
				parent.OnSingleClickContained(from, item);
		}

		public virtual void OnAosSingleClick(Mobile from)
		{
			ObjectPropertyList opl = PropertyList;

			if (opl.Header > 0)
				_ = from.Send(new MessageLocalized(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, opl.Header, Name, opl.HeaderArgs));
		}

		public virtual void OnSingleClick(Mobile from)
		{
			if (Deleted || !from.CanSee(this))
				return;

			if (DisplayLootType)
				LabelLootTypeTo(from);

			NetState ns = from.NetState;

			if (ns != null)
			{
				if (Name == null)
				{
					if (m_Amount <= 1)
						ns.Send(new MessageLocalized(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, LabelNumber, "", ""));
					else
						ns.Send(new MessageLocalizedAffix(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, LabelNumber, "", AffixType.Append, $" : {m_Amount}", ""));
				}
				else
				{
					ns.Send(new UnicodeMessage(Serial, m_ItemID, MessageType.Label, DisplayColor, 3, "ENU", "", Name + (m_Amount > 1 ? " : " + m_Amount : "")));
				}
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
			IEntity thisParent = m_Parent;
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
			if (m_Parent is Container parent)
			{
				parent.AddItem(newItem);
				newItem.Location = m_Location;
			}
			else
			{
				newItem.MoveToWorld(GetWorldLocation(), m_Map);
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

				if (info != null)
					return info.m_BlessedFor;

				return null;
			}
			set
			{
				CompactInfo info = AcquireCompactInfo();

				info.m_BlessedFor = value;

				if (info.m_BlessedFor == null)
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
			if (m_LootType == LootType.Blessed || (Mobile.InsuranceEnabled && Insured))
				return true;

			return (m != null && m == BlessedFor);
		}

		public virtual bool CheckNewbied()
		{
			return (m_LootType == LootType.Newbied);
		}

		public virtual bool IsStandardLoot()
		{
			if (Mobile.InsuranceEnabled && Insured)
				return false;

			if (BlessedFor != null)
				return false;

			return (m_LootType == LootType.Regular);
		}

		public override string ToString()
		{
			return string.Format("0x{0:X} \"{1}\"", Serial.Value, GetType().Name);
		}

		internal int m_TypeRef;

		public Item()
		{
			Serial = Serial.NewItem;

			Visible = true;
			Movable = true;
			Amount = 1;
			m_Map = Map.Internal;

			SetLastMoved();

			World.AddItem(this);

			Type ourType = GetType();
			m_TypeRef = World.m_ItemTypes.IndexOf(ourType);

			if (m_TypeRef == -1)
			{
				World.m_ItemTypes.Add(ourType);
				m_TypeRef = World.m_ItemTypes.Count - 1;
			}

			_ = Timer.DelayCall(EventSink.InvokeOnItemCreated, this);
		}

		[Constructable]
		public Item(int itemID) : this()
		{
			m_ItemID = itemID;
		}

		public Item(Serial serial)
		{
			Serial = serial;

			Type ourType = GetType();
			m_TypeRef = World.m_ItemTypes.IndexOf(ourType);

			if (m_TypeRef == -1)
			{
				World.m_ItemTypes.Add(ourType);
				m_TypeRef = World.m_ItemTypes.Count - 1;
			}
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
			if (Sockets == null)
			{
				Sockets = new List<ItemSocket>();
			}

			Sockets.Add(socket);
			socket.Owner = this;

			InvalidateProperties();
		}

		public bool RemoveSocket<T>()
		{
			var socket = GetSocket(typeof(T));

			if (socket != null)
			{
				RemoveItemSocket(socket);
				return true;
			}

			return false;
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
			if (Sockets == null)
			{
				return null;
			}

			return Sockets.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
		}

		public T GetSocket<T>(Func<T, bool> predicate) where T : ItemSocket
		{
			if (Sockets == null)
			{
				return null;
			}

			return Sockets.FirstOrDefault(s => s.GetType() == typeof(T) && (predicate == null || predicate(s as T))) as T;
		}

		public ItemSocket GetSocket(Type type)
		{
			if (Sockets == null)
			{
				return null;
			}

			return Sockets.FirstOrDefault(s => s.GetType() == type);
		}

		public bool HasSocket<T>()
		{
			if (Sockets == null)
			{
				return false;
			}

			return Sockets.Any(s => s.GetType() == typeof(T));
		}

		public bool HasSocket(Type t)
		{
			if (Sockets == null)
			{
				return false;
			}

			return Sockets.Any(s => s.GetType() == t);
		}
		#endregion
	}
}
