using Server.Engines.Craft;
using Server.Menus.ItemLists;
using Server.Network;
using System;

namespace Server.Items
{
	public interface ITool : IEntity, IUsesRemaining
	{
		CraftSystem CraftSystem { get; }

		bool BreakOnDepletion { get; }

		bool CheckAccessible(Mobile from, ref int num);
	}

	public abstract class BaseTool : BaseItem, ITool, IResource, IQuality
	{
		private int m_UsesRemaining;
		public int m_Hits;
		public int m_MaxHits;

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get { return m_Hits; }
			set
			{
				if (m_Hits == value)
					return;

				if (value > m_MaxHits)
					value = m_MaxHits;

				m_Hits = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get { return m_MaxHits; }
			set { m_MaxHits = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override CraftResource Resource
		{
			get => base.Resource;
			set
			{
				base.Resource = value;
				Hue = CraftResources.GetHue(Resource);
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override ItemQuality Quality
		{
			get => base.Quality;
			set
			{
				UnscaleUses();
				base.Quality = value;
				InvalidateProperties();
				ScaleUses();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RepairMode { get; set; }

		public void ScaleUses()
		{
			m_UsesRemaining = m_UsesRemaining * GetUsesScalar() / 100;
			InvalidateProperties();
		}

		public void UnscaleUses()
		{
			m_UsesRemaining = m_UsesRemaining * 100 / GetUsesScalar();
		}

		public int GetUsesScalar()
		{
			if (Quality == ItemQuality.Exceptional)
			{
				return 200;
			}

			return 100;
		}

		public bool ShowUsesRemaining
		{
			get => true;
			set { }
		}

		public virtual bool BreakOnDepletion => true;

		public abstract CraftSystem CraftSystem { get; }

		public BaseTool(int itemID)
			: this(Utility.RandomMinMax(25, 75), itemID)
		{
		}

		public BaseTool(int uses, int itemID)
			: base(itemID)
		{
			m_UsesRemaining = uses;
			Quality = ItemQuality.Normal;
		}

		public BaseTool(Serial serial)
			: base(serial)
		{
		}

		public override void AddCraftedProperties(ObjectPropertyList list)
		{
			if (Crafter != null)
			{
				list.Add(1050043, Crafter.TitleName); // crafted by ~1_NAME~
			}

			if (Quality == ItemQuality.Exceptional)
			{
				list.Add(1060636); // exceptional
			}
		}

		public override void AddUsesRemainingProperties(ObjectPropertyList list)
		{
			list.Add(1060584, UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public virtual void DisplayDurabilityTo(Mobile m)
		{
			LabelToAffix(m, 1017323, AffixType.Append, ": " + m_UsesRemaining.ToString()); // Durability
		}

		public virtual bool CheckAccessible(Mobile m, ref int num)
		{
			if (RootParent != m)
			{
				num = 1044263;
				return false;
			}

			return true;
		}

		public static bool CheckAccessible(Item tool, Mobile m)
		{
			return CheckAccessible(tool, m, false);
		}

		public static bool CheckAccessible(Item tool, Mobile m, bool message)
		{
			if (tool == null || tool.Deleted)
			{
				return false;
			}

			var num = 0;

			bool res;

			if (tool is ITool)
			{
				res = ((ITool)tool).CheckAccessible(m, ref num);
			}
			else
			{
				res = tool.IsChildOf(m) || tool.Parent == m;
			}

			if (num > 0 && message)
			{
				m.SendLocalizedMessage(num);
			}

			return res;
		}

		public static bool CheckTool(Item tool, Mobile m)
		{
			if (tool == null || tool.Deleted)
			{
				return false;
			}

			Item check = m.FindItemOnLayer(Layer.OneHanded);

			if (check is ITool && check != tool && !(check is AncientSmithyHammer))
			{
				return false;
			}

			check = m.FindItemOnLayer(Layer.TwoHanded);

			if (check is ITool && check != tool && !(check is AncientSmithyHammer))
			{
				return false;
			}

			return true;
		}

		public override void OnSingleClick(Mobile from)
		{
			if (Core.AOS)
			{
				DisplayDurabilityTo(from);
			}
			else
			{
				if (Name != null)
				{
					from.Send(new AsciiMessage(Serial, ItemId, MessageType.Label, 0, 3, "", Name));
				}
				else if (this is SmithHammer)
				{
					from.Send(new AsciiMessage(Serial, ItemId, MessageType.Label, 0, 3, "", "a smith's hammer"));
				}
			}

			base.OnSingleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || Parent == from)
			{
				if (Core.AOS)
				{
					CraftSystem system = CraftSystem;

					if (Core.TOL && RepairMode)
					{
						Repair.Do(from, system, this);
					}
					else
					{
						int num = system.CanCraft(from, this, null);

						if (num > 0 && (num != 1044267 || !Core.SE)) // Blacksmithing shows the gump regardless of proximity of an anvil and forge after SE
						{
							from.SendLocalizedMessage(num);
						}
						else
						{
							from.SendGump(new CraftGump(from, system, this, null));
						}
					}
				}
				else
				{
					if (this is SmithHammer)
					{
						DefBlacksmithy.CheckAnvilAndForge(from, 2, out bool anvil, out bool forge);

						if (anvil && forge)
						{
							BaseTool m_Tool = this;
							string IsFrom = "Main";
							from.SendMenu(new BlacksmithMenu(from, BlacksmithMenu.Main(from), IsFrom, m_Tool));
						}
						else
							from.SendAsciiMessage("You must be near an anvil and a forge to smith items.");
					}
				}
			}
			else
			{
				if (Core.AOS)
				{
					from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				}
				else
					from.SendAsciiMessage("That must be in your pack for you to use it.");
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			writer.Write((int)Resource);
			writer.Write(RepairMode);
			writer.Write((int)Quality);
			writer.Write(m_UsesRemaining);
			writer.Write((int)m_Hits);
			writer.Write((int)m_MaxHits);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Resource = (CraftResource)reader.ReadInt();
						RepairMode = reader.ReadBool();
						Quality = (ItemQuality)reader.ReadInt();
						m_UsesRemaining = reader.ReadInt();
						m_Hits = reader.ReadInt();
						m_MaxHits = reader.ReadInt();
						break;
					}
			}
		}

		#region ICraftable Members
		public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
		{
			PlayerConstructed = true;

			Quality = (ItemQuality)quality;

			if (makersMark)
			{
				Crafter = from;
			}

			return quality;
		}
		#endregion
	}
}
