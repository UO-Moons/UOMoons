using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

[Flipable(0x9A95, 0x9AA7)]
public abstract class BaseSpecialScrollBook : Container, ISecurable
{
	public const int MaxScrolls = 300;

	private int _capacity;

	[CommandProperty(AccessLevel.GameMaster)]
	public int Capacity
	{
		get => _capacity <= 0 ? MaxScrolls : _capacity;
		set
		{
			_capacity = value;

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public SecureLevel Level { get; set; }

	public override bool DisplaysContent => false;
	public override double DefaultWeight => 1.0;

	public abstract Type ScrollType { get; }

	public abstract int BadDropMessage { get; }
	public abstract int DropMessage { get; }
	public abstract int RemoveMessage { get; }
	public abstract int GumpTitle { get; }

	public BaseSpecialScrollBook(int id)
		: base(id)
	{
		LootType = LootType.Blessed;
	}

	public override int GetTotal(TotalType type)
	{
		return 0;
	}

	public override void OnDoubleClick(Mobile m)
	{
		_ = BaseHouse.FindHouseAt(this);

		if (m is PlayerMobile mobile && mobile.InRange(GetWorldLocation(), 2) /*&& (house == null || house.HasSecureAccess(m, this))*/)
		{
			_ = BaseGump.SendGump(new SpecialScrollBookGump(mobile, this));
		}
		else if (m.AccessLevel > AccessLevel.Player)
		{
			base.OnDoubleClick(m);
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1151797, $"{Items.Count}\t{Capacity}"); // Scrolls in book: ~1_val~/~2_val~
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		SetSecureLevelEntry.AddTo(from, this, list);
	}

	public override bool OnDragDrop(Mobile m, Item dropped)
	{
		if (m.InRange(GetWorldLocation(), 2))
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (dropped.GetType() != ScrollType)
			{
				m.SendLocalizedMessage(BadDropMessage);
			}
			else if (house == null || !IsLockedDown)
			{
				m.SendLocalizedMessage(1151765); // You must lock this book down in a house to add scrolls to it.
			}
			else if (!house.CheckAccessibility(this, m))
			{
				m.SendLocalizedMessage(1155693); // This item is impermissible and can not be added to the book.
			}
			else if (Items.Count < Capacity)
			{
				DropItem(dropped);

				m.SendLocalizedMessage(DropMessage);

				dropped.Movable = false;

				_ = m.CloseGump(typeof(SpecialScrollBookGump));

				return true;
			}
		}

		return false;
	}

	/*
    public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
    {
        return false; // prevent third party program drop needs tested
    }
    */

	public virtual void Construct(Mobile m, SkillName sk, double value)
	{
		SpecialScroll scroll = Items.OfType<SpecialScroll>().FirstOrDefault(s => s.Skill == sk && Math.Abs(s.Value - value) < 120);

		if (scroll != null)
		{
			if (m.Backpack == null || !m.Backpack.TryDropItem(m, scroll, false))
			{
				m.SendLocalizedMessage(502868); // Your backpack is too full.
			}
			else
			{
				BaseHouse house = BaseHouse.FindHouseAt(this);

				if (house != null && house.LockDowns.ContainsKey(scroll))
				{
					_ = house.LockDowns.Remove(scroll);
				}

				if (!scroll.Movable)
				{
					scroll.Movable = true;
				}

				if (scroll.IsLockedDown)
				{
					scroll.IsLockedDown = false;
				}

				m.SendLocalizedMessage(RemoveMessage);
			}
		}
	}

	public BaseSpecialScrollBook(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(3); // version

		writer.Write((int)Level);
		writer.Write(_capacity);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		if (version > 2)
		{
			Level = (SecureLevel)reader.ReadInt();
			_capacity = reader.ReadInt();
		}

		_ = Timer.DelayCall(
			() =>
			{
				foreach (var item in Items.Where(i => i.Movable))
					item.Movable = false;
			});
	}

	public virtual Dictionary<SkillCat, List<SkillName>> SkillInfo => null;
	public virtual Dictionary<int, double> ValueInfo => null;

	public static int GetCategoryLocalization(SkillCat category)
	{
		return category switch
		{
			SkillCat.Miscellaneous => 1078596,
			SkillCat.Combat => 1078592,
			SkillCat.TradeSkills => 1078591,
			SkillCat.Magic => 1078593,
			SkillCat.Wilderness => 1078595,
			SkillCat.Thievery => 1078594,
			SkillCat.Bard => 1078590,
			_ => 0
		};
	}
}
