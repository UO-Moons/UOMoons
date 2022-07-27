using Server.Gumps;
using Server.Network;
using Server.Targeting;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class JewelryBoxGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly JewelryBox _mBox;
	private readonly List<Item> _mList;

	private readonly int _mPage;

	private const int LabelColor = 0x7FFF;

	private bool CheckFilter(Item item)
	{
		JewelryBoxFilter f = _mBox.Filter;

		if (f.IsDefault)
			return true;

		if (f.Ring && item is BaseRing)
		{
			return true;
		}

		if (f.Bracelet && item is BaseBracelet)
		{
			return true;
		}

		if (f.Earrings && item is BaseEarrings)
		{
			return true;
		}

		if (f.Necklace && item is BaseNecklace)
		{
			return true;
		}

		if (f.Talisman && item is BaseTalisman)
		{
			return true;
		}

		return false;
	}

	private static int GetPageCount(int count)
	{
		return (count + 49) / 50;
	}

	private int GetIndexForPage(int page)
	{
		int index = 0;

		while (page-- > 0)
			index += GetCountForIndex(index);

		return index;
	}

	private int GetCountForIndex(int index)
	{
		int slots = 0;
		int count = 0;

		for (var i = index; i >= 0 && i < _mList.Count; ++i)
		{
			var recipe = _mList[i];

			if (CheckFilter(recipe))
			{
				const int add = 1;

				if (slots + add > 50)
					break;

				slots += add;
			}

			++count;
		}

		return count;
	}

	public JewelryBoxGump(Mobile from, JewelryBox box, int page = 0)
		: base(100, 100)
	{
		from.CloseGump(typeof(JewelryBoxGump));

		_mFrom = from;
		_mBox = box;
		_mPage = page;

		_mList = new List<Item>();

		foreach (var item in _mBox.Items.Where(CheckFilter))
		{
			_mList.Add(item);
		}

		int index = GetIndexForPage(page);
		int count = GetCountForIndex(index);
		int pageCount = GetPageCount(_mList.Count);
		int currentpage = pageCount > 0 ? page + 1 : 0;

		for (var i = index; i < index + count && i >= 0 && i < _mList.Count; ++i)
		{
			var item = _mList[i];

			if (!CheckFilter(item))
				continue;
		}

		AddPage(0);

		AddImage(0, 0, 0x9CCA);
		AddHtmlLocalized(40, 2, 500, 20, 1114513, "#1157694", 0x7FF0, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>   

		AddHtmlLocalized(50, 30, 100, 20, 1157695, 0x7FF0, false, false); // Select Filter:

		AddHtmlLocalized(41, 350, 123, 20, 1157698, $"{_mList.Count}@{_mBox.DefaultMaxItems}", 0x7FF0, false, false); // Items: ~1_NUM~ of ~2_MAX~
		AddHtmlLocalized(212, 350, 123, 20, 1153561, $"{currentpage}@{pageCount}", 0x7FF0, false, false); // Page ~1_CUR~ of ~2_MAX~
		AddHtmlLocalized(416, 350, 100, 20, 1153562, 0x7FF0, false, false); // <DIV ALIGN="CENTER">PAGE</DIV>

		JewelryBoxFilter f = box.Filter;

		AddHtmlLocalized(200, 30, 90, 20, 1154607, f.Ring ? 0x421F : LabelColor, false, false); // Ring
		AddButton(160, 30, 0xFA5, 0xFA7, 101, GumpButtonType.Reply, 0);

		AddHtmlLocalized(325, 30, 90, 20, 1079905, f.Bracelet ? 0x421F : LabelColor, false, false); // Bracelet
		AddButton(285, 30, 0xFA5, 0xFA7, 102, GumpButtonType.Reply, 0);

		AddHtmlLocalized(450, 30, 90, 20, 1079903, f.Earrings ? 0x421F : LabelColor, false, false); // Earrings
		AddButton(410, 30, 0xFA5, 0xFA7, 104, GumpButtonType.Reply, 0);

		AddHtmlLocalized(200, 55, 90, 20, 1157697, f.Necklace ? 0x421F : LabelColor, false, false); // Necklace
		AddButton(160, 55, 0xFA5, 0xFA7, 108, GumpButtonType.Reply, 0);

		AddHtmlLocalized(325, 55, 90, 20, 1071023, f.Talisman ? 0x421F : LabelColor, false, false); // Talisman
		AddButton(285, 55, 0xFA5, 0xFA7, 116, GumpButtonType.Reply, 0);

		AddHtmlLocalized(450, 55, 90, 20, 1062229, f.IsDefault ? 0x421F : LabelColor, false, false); // All
		AddButton(410, 55, 0xFA5, 0xFA7, 132, GumpButtonType.Reply, 0);
		AddButton(356, 353, 0x15E3, 0x15E7, 11, GumpButtonType.Reply, 0); // First page
		AddButton(376, 350, 0xFAE, 0xFB0, 1, GumpButtonType.Reply, 0); // Previous page

		AddButton(526, 350, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0); // Next Page         
		AddButton(560, 353, 0x15E1, 0x15E5, 12, GumpButtonType.Reply, 0); // Last page

		AddHtmlLocalized(270, 385, 100, 20, 1157696, LabelColor, false, false); // ADD JEWELRY
		AddButton(225, 385, 0xFAB, 0xFAD, 3, GumpButtonType.Reply, 0);

		int x = 0;

		for (int i = index; i < index + count && i >= 0 && i < _mList.Count; ++i)
		{
			Item item = _mList[i];

			int xoffset = x / 5 * 50;
			int yoffset = i % 5 * 50;

			x++;

			AddEcHandleInput();
			AddButton(50 + xoffset, 90 + yoffset, 0x92F, 0x92F, item.Serial, GumpButtonType.Reply, 0);
			AddItemProperty(item.Serial);
			AddItem(57 + xoffset, 108 + yoffset, item.ItemId, item.Hue);
			AddEcHandleInput();
		}
	}

	private class InternalTarget : Target
	{
		private readonly JewelryBox _mBox;
		private readonly int _mPage;

		public InternalTarget(JewelryBox box, int page)
			: base(-1, false, TargetFlags.None)
		{
			_mBox = box;
			_mPage = page;
		}

		private void TryDrop(Mobile from, Item dropped)
		{
			if (!_mBox.CheckAccessible(from, _mBox))
			{
				from.SendLocalizedMessage(1061637); // You are not allowed to access this.
			}
			else if (!dropped.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1157726); // You must be carrying the item to add it to the jewelry box.
			}
			else if (_mBox.IsAccept(dropped))
			{
				if (_mBox.IsFull)
				{
					from.SendLocalizedMessage(1157723); // The jewelry box is full.
				}
				else
				{
					_mBox.DropItem(dropped);
					from.Target = new InternalTarget(_mBox, _mPage);
				}
			}
			else if (dropped is Container container)
			{
				int count = 0;

				for (int i = container.Items.Count - 1; i >= 0; --i)
				{
					if (i >= container.Items.Count || !_mBox.IsAccept(container.Items[i])) continue;
					if (_mBox.IsFull)
					{
						from.SendLocalizedMessage(1157723); // The jewelry box is full.
						break;
					}

					_mBox.DropItem(container.Items[i]);
					count++;
				}

				if (count > 0)
				{
					from.CloseGump(typeof(JewelryBoxGump));
					from.SendGump(new JewelryBoxGump(from, _mBox, _mPage));
				}
				else
				{
					from.SendLocalizedMessage(1157724); // This is not a ring, bracelet, necklace, earring, or talisman.
					from.SendGump(new JewelryBoxGump(from, _mBox, _mPage));
				}
			}
			else
			{
				from.SendLocalizedMessage(1157724); // This is not a ring, bracelet, necklace, earring, or talisman.
				from.SendGump(new JewelryBoxGump(from, _mBox, _mPage));
			}
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (_mBox is {Deleted: false} && targeted is Item item)
			{
				TryDrop(from, item);
			}
		}

		protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
		{
			if (_mBox == null || _mBox.Deleted) return;
			from.CloseGump(typeof(JewelryBoxGump));
			from.SendGump(new JewelryBoxGump(from, _mBox, _mPage));
			from.SendLocalizedMessage(1157726); // You must be carrying the item to add it to the jewelry box.
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (!_mBox.CheckAccessible(_mFrom, _mBox))
		{
			_mFrom.SendLocalizedMessage(1061637); // You are not allowed to access this.
			return;
		}

		JewelryBoxFilter f = _mBox.Filter;

		int index = info.ButtonID;

		switch (index)
		{
			case 0: { break; }
			case 1: // Previous page
			{
				_mFrom.SendGump(_mPage > 0
					? new JewelryBoxGump(_mFrom, _mBox, _mPage - 1)
					: new JewelryBoxGump(_mFrom, _mBox, _mPage));

				break;
			}
			case 2: // Next Page
			{
				_mFrom.SendGump(GetIndexForPage(_mPage + 1) < _mList.Count
					? new JewelryBoxGump(_mFrom, _mBox, _mPage + 1)
					: new JewelryBoxGump(_mFrom, _mBox, _mPage));

				return;
			}
			case 3: // ADD JEWELRY
			{
				_mFrom.Target = new InternalTarget(_mBox, _mPage);
				_mFrom.SendLocalizedMessage(1157725); // Target rings, bracelets, necklaces, earrings, or talisman in your backpack. You may also target a sub-container to add contents to the the jewelry box. When done, press ESC.
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));
				break;
			}
			case 11: // First page
			{
				_mFrom.SendGump(_mPage > 0
					? new JewelryBoxGump(_mFrom, _mBox, 1)
					: new JewelryBoxGump(_mFrom, _mBox, _mPage));

				break;
			}
			case 12: // Last Page
			{
				int pagecount = GetPageCount(_mList.Count);

				if (_mPage != pagecount && _mPage >= 1)
				{
					_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox, pagecount));
				}
				else
				{
					_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox, _mPage));
				}

				break;
			}
			case 101: // Ring
			{
				f.Ring = true;
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
			case 102: // Bracelet
			{
				f.Bracelet = true;
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
			case 104: // Earrings
			{
				f.Earrings = true;
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
			case 108: // Necklace
			{
				f.Necklace = true;
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
			case 116: // Talisman
			{
				f.Talisman = true;
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
			case 132: // ALL
			{
				f.Clear();
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
			default:
			{
				Item item = _mBox.Items.Find(x => x.Serial == index);
				_mFrom.AddToBackpack(item);
				_mFrom.SendGump(new JewelryBoxGump(_mFrom, _mBox));

				break;
			}
		}
	}
}
