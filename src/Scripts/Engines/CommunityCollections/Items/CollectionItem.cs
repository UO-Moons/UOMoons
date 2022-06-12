using Server.Items;
using Server.Mobiles;
using System;

namespace Server
{
	public class CollectionItem
	{
		private readonly int m_X;
		private readonly int m_Y;

		public CollectionItem(Type type, int itemID, int tooltip, int hue, double points, bool questitem = false)
		{
			Type = type;
			ItemID = itemID;
			Tooltip = tooltip;
			Hue = hue;
			Points = points;
			QuestItem = questitem;

			Rectangle2D rec;

			try
			{
				rec = ItemBounds.Table[ItemID];
			}
			catch
			{
				rec = new Rectangle2D(0, 0, 0, 0);
			}

			if (rec.X == 0 && rec.Y == 0 && rec.Width == 0 && rec.Height == 0)
			{
				_ = 0;

				Item.Measure(Item.GetBitmap(ItemID), out m_X, out m_Y, out int mx, out int my);

				Width = mx - m_X;
				Height = my - m_Y;
			}
			else
			{
				m_X = rec.X;
				m_Y = rec.Y;
				Width = rec.Width;
				Height = rec.Height;
			}
		}

		public Type Type { get; }  // image info
		public int ItemID { get; }
		public int X => m_X;
		public int Y => m_Y;
		public int Width { get; }
		public int Height { get; }
		public int Tooltip { get; }
		public int Hue { get; }
		public double Points { get; }
		public bool QuestItem { get; }

		public virtual bool Validate(PlayerMobile from, Item item)
		{
			return true;
		}

		public virtual bool CanSelect(PlayerMobile from)
		{
			return true;
		}

		public virtual void OnGiveReward(PlayerMobile to, Item item, IComunityCollection collection, int hue)
		{
		}
	}

	public class CollectionHuedItem : CollectionItem
	{
		private readonly int[] m_Hues;

		public CollectionHuedItem(Type type, int itemID, int tooltip, int hue, double points, int[] hues)
			: base(type, itemID, tooltip, hue, points)
		{
			m_Hues = hues;
		}

		public int[] Hues => m_Hues;
	}

	public class CollectionTitle : CollectionItem
	{
		private readonly object m_Title;

		public CollectionTitle(object title, int tooltip, double points)
			: base(null, 0xFF1, tooltip, 0x0, points)
		{
			m_Title = title;
		}

		public object Title => m_Title;

		public override void OnGiveReward(PlayerMobile to, Item item, IComunityCollection collection, int hue)
		{
			if (to.AddRewardTitle(m_Title))
			{
				if (m_Title is int @int)
					to.SendLocalizedMessage(1073625, "#" + @int); // The title "~1_TITLE~" has been bestowed upon you. 
				else if (m_Title is string @string)
					to.SendLocalizedMessage(1073625, @string); // The title "~1_TITLE~" has been bestowed upon you. 

				to.AddCollectionPoints(collection.CollectionID, (int)Points * -1);
			}
			else
				to.SendLocalizedMessage(1073626); // You already have that title!
		}
	}

	public class CollectionTreasureMap : CollectionItem
	{
		private readonly int m_Level;

		public CollectionTreasureMap(int level, int tooltip, double points)
			: base(typeof(TreasureMap), 0x14EB, tooltip, 0x0, points)
		{
			m_Level = level;
		}

		public int Level => m_Level;

		public override bool Validate(PlayerMobile from, Item item)
		{
			if (item is TreasureMap map && map.Level == m_Level)
				return true;

			return false;
		}
	}

	public class CollectionSpellbook : CollectionItem
	{
		private readonly SpellbookType m_Type;

		public CollectionSpellbook(SpellbookType type, int itemID, int tooltip, double points)
			: base(typeof(Spellbook), itemID, tooltip, 0x0, points)
		{
			m_Type = type;
		}

		public SpellbookType SpellbookType => m_Type;

		public override bool Validate(PlayerMobile from, Item item)
		{
			if (item is Spellbook spellbook && spellbook.SpellbookType == m_Type && spellbook.Content == 0)
				return true;

			return false;
		}
	}
}
