#region References

using Server.Gumps;
using Server.Multis;
using System.Collections;
using System.Linq;

#endregion

namespace Server.Engines.TownHouses
{
	public class TownHousesGump : GumpPlusLight
	{
		private enum ListPage
		{
			Town,
			House
		}

		public static void Initialize()
		{
			VersionCommand.AddCommand("TownHouses", AccessLevel.Counselor, OnHouses);
		}

		private static void OnHouses(CommandInfo info)
		{
			_ = new TownHousesGump(info.Mobile);
		}

		private ListPage m_CListPage;
		private int m_CPage;

		public TownHousesGump(Mobile m)
			: base(m, 100, 100)
		{
			m.CloseGump(typeof(TownHousesGump));
		}

		protected override void BuildGump()
		{
			const int width = 400;
			int y = 0;

			AddHtml(0, y += 10, width, "<CENTER>TownHouses Main Menu");
			AddImage(width / 2 - 120, y + 2, 0x39);
			AddImage(width / 2 + 90, y + 2, 0x3B);

			int pp = 10;

			if (m_CPage != 0)
			{
				AddButton(width / 2 - 10, y += 25, 0x25E4, 0x25E5, "Page Down", PageDown);
			}

			ArrayList list = new();
			if (m_CListPage == ListPage.Town)
			{
				list = new ArrayList(TownHouseSign.AllSigns);
			}
			else
			{
				foreach (BaseHouse item in World.Items.Values.OfType<BaseHouse>())
				{
					list.Add(item);
				}
			}

			list.Sort(new InternalSort());

			AddHtml(0, y += 20, width,
				"<CENTER>" + (m_CListPage == ListPage.Town ? "TownHouses" : "Houses") + " Count: " + list.Count);
			AddHtml(0, y += 25, width, "<CENTER>TownHouses / Houses");
			AddButton(width / 2 - 100, y + 3, m_CListPage == ListPage.Town ? 0x939 : 0x2716, "Page", Page, ListPage.Town);
			AddButton(width / 2 + 90, y + 3, m_CListPage == ListPage.House ? 0x939 : 0x2716, "Page", Page, ListPage.House);

			y += 5;

			for (int i = m_CPage * pp; i < (m_CPage + 1) * pp && i < list.Count; ++i)
			{
				if (m_CListPage == ListPage.Town)
				{
					TownHouseSign sign = (TownHouseSign)list[i];

					AddHtml(30, y += 20, width / 2 - 20, "<DIV ALIGN=LEFT>" + sign.Name);
					AddButton(15, y + 3, 0x2716, "TownHouse Menu", TownHouseMenu, sign);

					if (sign.House?.Owner == null)
					{
						continue;
					}
					AddHtml(width / 2, y, width / 2 - 40, "<DIV ALIGN=RIGHT>" + sign.House.Owner.RawName);
					AddButton(width - 30, y + 3, 0x2716, "House Menu", HouseMenu, sign.House);
				}
				else
				{
					var house = (BaseHouse)list[i];

					if (house == null)
						continue;

					AddHtml(30, y += 20, width / 2 - 20, "<DIV ALIGN=LEFT>" + house.Name);
					AddButton(15, y + 3, 0x2716, "Goto", Goto, house);

					if (house.Owner == null)
					{
						continue;
					}

					AddHtml(width / 2, y, width / 2 - 40, "<DIV ALIGN=RIGHT>" + house.Owner.RawName);
					AddButton(width - 30, y + 3, 0x2716, "House Menu", HouseMenu, house);
				}
			}

			if (pp * (m_CPage + 1) < list.Count)
			{
				AddButton(width / 2 - 10, y += 25, 0x25E8, 0x25E9, "Page Up", PageUp);
			}

			if (m_CListPage == ListPage.Town)
			{
				AddHtml(0, y += 35, width, "<CENTER>Add New TownHouse");
				AddButton(width / 2 - 80, y + 3, 0x2716, "New", New);
				AddButton(width / 2 + 70, y + 3, 0x2716, "New", New);
			}

			AddBackgroundZero(0, 0, width, y + 40, 0x13BE);
		}

		private void TownHouseMenu(object obj)
		{
			if (obj is not TownHouseSign sign)
			{
				return;
			}

			NewGump();

			_ = new TownHouseSetupGump(Owner, sign);
		}

		private void Page(object obj)
		{
			m_CListPage = (ListPage)obj;
			NewGump();
		}

		private void Goto(object obj)
		{
			if (obj is not BaseHouse house)
			{
				return;
			}

			Owner.Location = house.BanLocation;
			Owner.Map = house.Map;

			NewGump();
		}

		private void HouseMenu(object obj)
		{
			if (obj is not BaseHouse house)
				return;

			NewGump();

			Owner.SendGump(new HouseGumpAOS(0, Owner, house));
		}

		private void New()
		{
			TownHouseSign sign = new();
			Owner.AddToBackpack(sign);
			Owner.SendMessage(
				"A new sign is now in your backpack.  It will move on it's own during setup, but if you don't complete setup you may want to delete it.");

			NewGump();

			_ = new TownHouseSetupGump(Owner, sign);
		}

		private void PageUp()
		{
			m_CPage++;
			NewGump();
		}

		private void PageDown()
		{
			m_CPage--;
			NewGump();
		}


		private class InternalSort : IComparer
		{
			public int Compare(object x, object y)
			{
				switch (x)
				{
					case null when y == null:
						return 0;
					case TownHouseSign a1:
					{
						var b = (TownHouseSign) y;

						return Insensitive.Compare(a1.Name, b.Name);
					}
					default:
					{
						var a = (BaseHouse) x;
						var b = (BaseHouse) y;

						switch (a)
						{
							case {Owner: null} when b?.Owner != null:
								return -1;
							case {Owner: { }} when b?.Owner == null:
								return 1;
						}

						if (a?.Owner == null)
							return 0;

						if (b?.Owner != null)
							return Insensitive.Compare(a.Owner.RawName, b.Owner.RawName);
						break;
					}
				}

				return 0;
			}
		}
	}
}
