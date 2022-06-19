using Server.Items;
using Server.Mobiles;
using System;

namespace Server;

public class CollectionItem
{
	private readonly int _mX;
	private readonly int _mY;

	public CollectionItem(Type type, int itemId, int tooltip, int hue, double points, bool questitem = false)
	{
		Type = type;
		ItemId = itemId;
		Tooltip = tooltip;
		Hue = hue;
		Points = points;
		QuestItem = questitem;

		Rectangle2D rec;

		try
		{
			rec = ItemBounds.Table[ItemId];
		}
		catch
		{
			rec = new Rectangle2D(0, 0, 0, 0);
		}

		if (rec.X == 0 && rec.Y == 0 && rec.Width == 0 && rec.Height == 0)
		{
			_ = 0;

			Item.Measure(Item.GetBitmap(ItemId), out _mX, out _mY, out int mx, out int my);

			Width = mx - _mX;
			Height = my - _mY;
		}
		else
		{
			_mX = rec.X;
			_mY = rec.Y;
			Width = rec.Width;
			Height = rec.Height;
		}
	}

	public Type Type { get; }  // image info
	public int ItemId { get; }
	public int X => _mX;
	public int Y => _mY;
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
	public CollectionHuedItem(Type type, int itemId, int tooltip, int hue, double points, int[] hues)
		: base(type, itemId, tooltip, hue, points)
	{
		Hues = hues;
	}

	public int[] Hues { get; }
}

public class CollectionTitle : CollectionItem
{
	public CollectionTitle(object title, int tooltip, double points)
		: base(null, 0xFF1, tooltip, 0x0, points)
	{
		Title = title;
	}

	public object Title { get; }

	public override void OnGiveReward(PlayerMobile to, Item item, IComunityCollection collection, int hue)
	{
		if (to.AddRewardTitle(Title))
		{
			switch (Title)
			{
				case int @int:
					to.SendLocalizedMessage(1073625, "#" + @int); // The title "~1_TITLE~" has been bestowed upon you. 
					break;
				case string @string:
					to.SendLocalizedMessage(1073625, @string); // The title "~1_TITLE~" has been bestowed upon you. 
					break;
			}

			to.AddCollectionPoints(collection.CollectionId, (int)Points * -1);
		}
		else
			to.SendLocalizedMessage(1073626); // You already have that title!
	}
}

public class CollectionTreasureMap : CollectionItem
{
	public CollectionTreasureMap(int level, int tooltip, double points)
		: base(typeof(TreasureMap), 0x14EB, tooltip, 0x0, points)
	{
		Level = level;
	}

	public int Level { get; }

	public override bool Validate(PlayerMobile from, Item item)
	{
		return item is TreasureMap map && map.Level == Level;
	}
}

public class CollectionSpellbook : CollectionItem
{
	public CollectionSpellbook(SpellbookType type, int itemId, int tooltip, double points)
		: base(typeof(Spellbook), itemId, tooltip, 0x0, points)
	{
		SpellbookType = type;
	}

	public SpellbookType SpellbookType { get; }

	public override bool Validate(PlayerMobile from, Item item)
	{
		return item is Spellbook spellbook && spellbook.SpellbookType == SpellbookType && spellbook.Content == 0;
	}
}
