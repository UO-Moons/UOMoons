#region References

using Server.Targeting;
using System;

#endregion

namespace Server.Engines.TownHouses
{
	public class TownHouseSetupGump : GumpPlusLight
	{
		public static Rectangle2D FixRect(Rectangle2D rect)
		{
			Point3D pointOne = Point3D.Zero;
			Point3D pointTwo = Point3D.Zero;

			if (rect.Start.X < rect.End.X)
			{
				pointOne.X = rect.Start.X;
				pointTwo.X = rect.End.X;
			}
			else
			{
				pointOne.X = rect.End.X;
				pointTwo.X = rect.Start.X;
			}

			if (rect.Start.Y < rect.End.Y)
			{
				pointOne.Y = rect.Start.Y;
				pointTwo.Y = rect.End.Y;
			}
			else
			{
				pointOne.Y = rect.End.Y;
				pointTwo.Y = rect.Start.Y;
			}

			return new Rectangle2D(pointOne, pointTwo);
		}

		private enum Page
		{
			Welcome,
			Blocks,
			Floors,
			Sign,
			Ban,
			LocSec,
			Items,
			Length,
			Price,
			Skills,
			Other,
			Other2
		}

		private enum TargetType
		{
			BanLoc,
			SignLoc,
			MinZ,
			MaxZ,
			BlockOne,
			BlockTwo
		}

		private readonly TownHouseSign m_CSign;
		private Page m_CPage;
		private bool m_CQuick;

		public TownHouseSetupGump(Mobile m, TownHouseSign sign) : base(m, 50, 50)
		{
			m.CloseGump(typeof(TownHouseSetupGump));

			m_CSign = sign;
		}

		protected override void BuildGump()
		{
			if (m_CSign == null)
			{
				return;
			}

			const int width = 300;
			int y = 0;

			if (m_CQuick)
			{
				QuickPage(width, ref y);
			}
			else
			{
				switch (m_CPage)
				{
					case Page.Welcome:
						WelcomePage(width, ref y);
						break;
					case Page.Blocks:
						BlocksPage(width, ref y);
						break;
					case Page.Floors:
						FloorsPage(width, ref y);
						break;
					case Page.Sign:
						SignPage(width, ref y);
						break;
					case Page.Ban:
						BanPage(width, ref y);
						break;
					case Page.LocSec:
						LocSecPage(width, ref y);
						break;
					case Page.Items:
						ItemsPage(width, ref y);
						break;
					case Page.Length:
						LengthPage(width, ref y);
						break;
					case Page.Price:
						PricePage(width, ref y);
						break;
					case Page.Skills:
						SkillsPage(width, ref y);
						break;
					case Page.Other:
						OtherPage(width, ref y);
						break;
					case Page.Other2:
						OtherPage2(width, ref y);
						break;
				}

				BuildTabs(width, ref y);
			}

			AddBackgroundZero(0, 0, width, y += 30, 0x13BE);

			if (!m_CSign.PriceReady || m_CSign.Owned)
			{
				return;
			}
			AddBackground(width / 2 - 50, y, 100, 30, 0x13BE);
			AddHtml(width / 2 - 50 + 25, y + 5, 100, "Claim Home");
			AddButton(width / 2 - 50 + 5, y + 10, 0x837, 0x838, "Claim", Claim);
		}

		private void BuildTabs(int width, ref int y)
		{
			int x = 20;

			y += 30;

			AddButton(x - 5, y - 3, 0x768, "Quick", Quick);
			AddLabel(x, y - 3, m_CQuick ? 0x34 : 0x47E, "Q");

			AddButton(x += 20, y, m_CPage == Page.Welcome ? 0x939 : 0x93A, "Welcome Page", ChangePage, 0);
			AddButton(x += 20, y, m_CPage == Page.Blocks ? 0x939 : 0x93A, "Blocks Page", ChangePage, 1);

			if (m_CSign.BlocksReady)
			{
				AddButton(x += 20, y, m_CPage == Page.Floors ? 0x939 : 0x93A, "Floors Page", ChangePage, 2);
			}

			if (m_CSign.FloorsReady)
			{
				AddButton(x += 20, y, m_CPage == Page.Sign ? 0x939 : 0x93A, "Sign Page", ChangePage, 3);
			}

			if (m_CSign.SignReady)
			{
				AddButton(x += 20, y, m_CPage == Page.Ban ? 0x939 : 0x93A, "Ban Page", ChangePage, 4);
			}

			if (m_CSign.BanReady)
			{
				AddButton(x += 20, y, m_CPage == Page.LocSec ? 0x939 : 0x93A, "LocSec Page", ChangePage, 5);
			}

			if (m_CSign.LocSecReady)
			{
				AddButton(x += 20, y, m_CPage == Page.Items ? 0x939 : 0x93A, "Items Page", ChangePage, 6);

				if (!m_CSign.Owned)
				{
					AddButton(x += 20, y, m_CPage == Page.Length ? 0x939 : 0x93A, "Length Page", ChangePage, 7);
				}
				else
				{
					x += 20;
				}

				AddButton(x += 20, y, m_CPage == Page.Price ? 0x939 : 0x93A, "Price Page", ChangePage, 8);
			}

			if (!m_CSign.PriceReady)
			{
				return;
			}
			AddButton(x += 20, y, m_CPage == Page.Skills ? 0x939 : 0x93A, "Skills Page", ChangePage, 9);
			AddButton(x += 20, y, m_CPage == Page.Other ? 0x939 : 0x93A, "Other Page", ChangePage, 10);
			AddButton(x + 20, y, m_CPage == Page.Other2 ? 0x939 : 0x93A, "Other Page 2", ChangePage, 11);
		}

		private void QuickPage(int width, ref int y)
		{
			m_CSign.ClearPreview();

			AddHtml(0, y += 10, width, "<CENTER>Quick Setup");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddButton(5, 5, 0x768, "Quick", Quick);
			AddLabel(10, 5, m_CQuick ? 0x34 : 0x47E, "Q");

			AddHtml(0, y += 25, width / 2 - 55, "<DIV ALIGN=RIGHT>Name");
			AddTextField(width / 2 - 15, y, 100, 20, 0x480, 0xBBC, "Name", m_CSign.Name);
			AddButton(width / 2 - 40, y + 3, 0x2716, "Name", Name);

			AddHtml(0, y += 25, width / 2, "<CENTER>Add Area");
			AddButton(width / 4 - 50, y + 3, 0x2716, "Add Area", AddBlock);
			AddButton(width / 4 + 40, y + 3, 0x2716, "Add Area", AddBlock);

			AddHtml(width / 2, y, width / 2, "<CENTER>Clear All");
			AddButton(width / 4 * 3 - 50, y + 3, 0x2716, "ClearAll", ClearAll);
			AddButton(width / 4 * 3 + 40, y + 3, 0x2716, "ClearAll", ClearAll);

			AddHtml(0, y += 25, width, "<CENTER>Base Floor: " + m_CSign.MinZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Base Floor", MinZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Base Floor", MinZSelect);

			AddHtml(0, y += 17, width, "<CENTER>Top Floor: " + m_CSign.MaxZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Top Floor", MaxZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Top Floor", MaxZSelect);

			AddHtml(0, y += 25, width / 2, "<CENTER>Sign Loc");
			AddButton(width / 4 - 50, y + 3, 0x2716, "Sign Loc", SignLocSelect);
			AddButton(width / 4 + 40, y + 3, 0x2716, "Sign Loc", SignLocSelect);

			AddHtml(width / 2, y, width / 2, "<CENTER>Ban Loc");
			AddButton(width / 4 * 3 - 50, y + 3, 0x2716, "Ban Loc", BanLocSelect);
			AddButton(width / 4 * 3 + 40, y + 3, 0x2716, "Ban Loc", BanLocSelect);

			AddHtml(0, y += 25, width, "<CENTER>Suggest Secures");
			AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);
			AddButton(width / 2 + 60, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);

			AddHtml(0, y += 20, width / 2 - 20, "<DIV ALIGN=RIGHT>Secures");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Secures", m_CSign.Secures.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Secures", Secures);

			AddHtml(0, y += 22, width / 2 - 20, "<DIV ALIGN=RIGHT>Lockdowns");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Lockdowns", m_CSign.Locks.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Lockdowns", Lockdowns);

			AddHtml(0, y += 25, width, "<CENTER>Give buyer items in home");
			AddButton(width / 2 - 110, y, m_CSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);
			AddButton(width / 2 + 90, y, m_CSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);

			if (m_CSign.KeepItems)
			{
				AddHtml(0, y += 25, width / 2 - 25, "<DIV ALIGN=RIGHT>At cost");
				AddTextField(width / 2 + 15, y, 70, 20, 0x480, 0xBBC, "ItemsPrice", m_CSign.ItemsPrice.ToString());
				AddButton(width / 2 - 10, y + 5, 0x2716, "ItemsPrice", ItemsPrice);
			}
			else
			{
				AddHtml(0, y += 25, width, "<CENTER>Don't delete items");
				AddButton(width / 2 - 110, y, m_CSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
				AddButton(width / 2 + 90, y, m_CSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
			}

			if (!m_CSign.Owned)
			{
				AddHtml(120, y += 25, 50, m_CSign.PriceType);
				AddButton(170, y + 8, 0x985, 0x985, "LengthUp", PriceUp);
				AddButton(170, y - 2, 0x983, 0x983, "LengthDown", PriceDown);
			}

			if (m_CSign.RentByTime != TimeSpan.Zero)
			{
				AddHtml(0, y += 25, width, "<CENTER>Recurring Rent");
				AddButton(width / 2 - 80, y, m_CSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);
				AddButton(width / 2 + 60, y, m_CSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);

				if (m_CSign.RecurRent)
				{
					AddHtml(0, y += 20, width, "<CENTER>Rent To Own");
					AddButton(width / 2 - 80, y, m_CSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
					AddButton(width / 2 + 60, y, m_CSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
				}
			}

			AddHtml(0, y += 25, width, "<CENTER>Free");
			AddButton(width / 2 - 80, y, m_CSign.Free ? 0xD3 : 0xD2, "Free", Free);
			AddButton(width / 2 + 60, y, m_CSign.Free ? 0xD3 : 0xD2, "Free", Free);

			if (m_CSign.Free)
			{
				return;
			}
			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>" + m_CSign.PriceType + " Price");
			AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", m_CSign.Price.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Price", Price);

			AddHtml(0, y += 25, width, "<CENTER>Suggest Price");
			AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest", SuggestPrice);
			AddButton(width / 2 + 50, y + 3, 0x2716, "Suggest", SuggestPrice);
		}

		private void WelcomePage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Welcome!");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			var helptext = "";

			AddHtml(0, y += 25, width / 2 - 55, "<DIV ALIGN=RIGHT>Name");
			AddTextField(width / 2 - 15, y, 100, 20, 0x480, 0xBBC, "Name", m_CSign.Name);
			AddButton(width / 2 - 40, y + 3, 0x2716, "Name", Name);

			if (m_CSign != null && m_CSign.Map != Map.Internal && m_CSign.RootParent == null)
			{
				AddHtml(0, y += 25, width, "<CENTER>Goto");
				AddButton(width / 2 - 50, y + 3, 0x2716, "Goto", Goto);
				AddButton(width / 2 + 40, y + 3, 0x2716, "Goto", Goto);
			}

			if (m_CSign.Owned)
			{
				helptext = $"  This home is owned by {m_CSign.House.Owner.Name}, so be aware that changing anything " +
				           "through this menu will change the home itself!  You can add more area, change the ownership " +
				           "rules, almost anything!  You cannot, however, change the rental status of the home, way too many " +
				           "ways for things to go ill.  If you change the restrictions and the home owner no longer meets them, " +
				           "they will receive the normal 24 hour demolish warning.";

				AddHtml(10, y += 25, width - 20, 180, helptext, false, false);

				y += 180;
			}
			else
			{
				helptext = string.Format("  Welcome to the TownHouse setup menu!  This menu will guide you through " +
										 "each step in the setup process.  You can set up any area to be a home, and then detail everything from " +
										 "lockdowns and price to whether you want to sell or rent the house.  Let's begin here with the name of " +
										 "this new Town House!");

				AddHtml(10, y += 25, width - 20, 130, helptext, false, false);

				y += 130;
			}

			AddHtml(width - 60, y += 15, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void BlocksPage(int width, ref int y)
		{
			if (m_CSign == null)
			{
				return;
			}

			m_CSign.ShowAreaPreview(Owner);

			AddHtml(0, y += 10, width, "<CENTER>Create the Area");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Add Area");
			AddButton(width / 2 - 50, y + 3, 0x2716, "Add Area", AddBlock);
			AddButton(width / 2 + 40, y + 3, 0x2716, "Add Area", AddBlock);

			AddHtml(0, y += 20, width, "<CENTER>Clear All");
			AddButton(width / 2 - 50, y + 3, 0x2716, "ClearAll", ClearAll);
			AddButton(width / 2 + 40, y + 3, 0x2716, "ClearAll", ClearAll);

			string helptext = string.Format("   Setup begins with defining the area you wish to sell or rent.  " +
											"You can add as many boxes as you wish, and each time the preview will extend to show what " +
											"you've selected so far.  If you feel like starting over, just clear them away!  You must have " +
											"at least one block defined before continuing to the next step.");

			AddHtml(10, y += 35, width - 20, 140, helptext, false, false);

			y += 140;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.BlocksReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void FloorsPage(int width, ref int y)
		{
			m_CSign.ShowFloorsPreview(Owner);

			AddHtml(0, y += 10, width, "<CENTER>Floors");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Base Floor: " + m_CSign.MinZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Base Floor", MinZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Base Floor", MinZSelect);

			AddHtml(0, y += 20, width, "<CENTER>Top Floor: " + m_CSign.MaxZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Top Floor", MaxZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Top Floor", MaxZSelect);

			string helptext = string.Format("   Now you will need to target the floors you wish to sell.  " +
											"If you only want one floor, you can skip targeting the top floor.  Everything within the base " +
											"and highest floor will come with the home, and the more floors, the higher the cost later on.");

			AddHtml(10, y += 35, width - 20, 110, helptext, false, false);

			y += 110;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.FloorsReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void SignPage(int width, ref int y)
		{
			if (m_CSign == null)
			{
				return;
			}

			m_CSign.ShowSignPreview();

			AddHtml(0, y += 10, width, "<CENTER>Sign Location");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Set Location");
			AddButton(width / 2 - 60, y + 3, 0x2716, "Sign Loc", SignLocSelect);
			AddButton(width / 2 + 50, y + 3, 0x2716, "Sign Loc", SignLocSelect);

			string helptext = string.Format("   With this sign, the owner will have the same home owning rights " +
											"as custom or classic homes.  If they use the sign to demolish the home, it will automatically " +
											"return to sale or rent.  The sign players will use to purchase the home will appear in the same " +
											"spot, slightly below the normal house sign.");

			AddHtml(10, y += 35, width - 20, 130, helptext, false, false);

			y += 130;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.SignReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void BanPage(int width, ref int y)
		{
			if (m_CSign == null)
			{
				return;
			}

			m_CSign.ShowBanPreview();

			AddHtml(0, y += 10, width, "<CENTER>Ban Location");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Set Location");
			AddButton(width / 2 - 60, y + 3, 0x2716, "Ban Loc", BanLocSelect);
			AddButton(width / 2 + 50, y + 3, 0x2716, "Ban Loc", BanLocSelect);

			string helptext = string.Format("   The ban location determines where players are sent when ejected or " +
											"banned from a home.  If you never set this, they would appear at the south west corner of the outside " +
											"of the home.");

			AddHtml(10, y += 35, width - 20, 100, helptext, false, false);

			y += 100;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.BanReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void LocSecPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Lockdowns and Secures");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Suggest");
			AddButton(width / 2 - 50, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);
			AddButton(width / 2 + 40, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Secures");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Secures", m_CSign.Secures.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Secures", Secures);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Lockdowns");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Lockdowns", m_CSign.Locks.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Lockdowns", Lockdowns);

			string helptext = string.Format("   With this step you'll set the amount of storage for the home, or let " +
											"the system do so for you using the Suggest button.  In general, players get half the number of lockdowns " +
											"as secure storage.");

			AddHtml(10, y += 35, width - 20, 90, helptext, false, false);

			y += 90;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.LocSecReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void ItemsPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Decoration Items");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Give buyer items in home");
			AddButton(width / 2 - 110, y, m_CSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);
			AddButton(width / 2 + 90, y, m_CSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);

			if (m_CSign.KeepItems)
			{
				AddHtml(0, y += 25, width / 2 - 25, "<DIV ALIGN=RIGHT>At cost");
				AddTextField(width / 2 + 15, y, 70, 20, 0x480, 0xBBC, "ItemsPrice", m_CSign.ItemsPrice.ToString());
				AddButton(width / 2 - 10, y + 5, 0x2716, "ItemsPrice", ItemsPrice);
			}
			else
			{
				AddHtml(0, y += 25, width, "<CENTER>Don't delete items");
				AddButton(width / 2 - 110, y, m_CSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
				AddButton(width / 2 + 90, y, m_CSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
			}

			var helptext =
				string.Format("   By default, the system will delete all items non-static items already " +
							  "in the home at the time of purchase.  These items are commonly referred to as Decoration Items. " +
							  "They do not include home addons, like forges and the like.  They do include containers.  You can " +
							  "allow players to keep these items by saying so here, and you may also charge them to do so!");

			AddHtml(10, y += 35, width - 20, 160, helptext, false, false);

			y += 160;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.ItemsReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + (m_CSign.Owned ? 2 : 1));
		}

		private void LengthPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Buy or Rent");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(120, y += 25, 50, m_CSign.PriceType);
			AddButton(170, y + 8, 0x985, 0x985, "LengthUp", PriceUp);
			AddButton(170, y - 2, 0x983, 0x983, "LengthDown", PriceDown);

			if (m_CSign.RentByTime != TimeSpan.Zero)
			{
				AddHtml(0, y += 25, width, "<CENTER>Recurring Rent");
				AddButton(width / 2 - 80, y, m_CSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);
				AddButton(width / 2 + 60, y, m_CSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);

				if (m_CSign.RecurRent)
				{
					AddHtml(0, y += 20, width, "<CENTER>Rent To Own");
					AddButton(width / 2 - 80, y, m_CSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
					AddButton(width / 2 + 60, y, m_CSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
				}
			}

			var helptext =
				string.Format("   Getting closer to completing the setup!  Now you get to specify whether " +
							  "this is a purchase or rental property.  Simply use the arrows until you have the setting you desire.  For " +
							  "rental property, you can also make the purchase non-recuring, meaning after the time is up the player " +
							  "gets the boot!  With recurring, if they have the money available they can continue to rent.  You can " +
							  "also enable Rent To Own, allowing players to own the property after making two months worth of payments.");

			AddHtml(10, y += 35, width - 20, 160, helptext, false, true);

			y += 160;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.LengthReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void PricePage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Price");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Free");
			AddButton(width / 2 - 80, y, m_CSign.Free ? 0xD3 : 0xD2, "Free", Free);
			AddButton(width / 2 + 60, y, m_CSign.Free ? 0xD3 : 0xD2, "Free", Free);

			if (!m_CSign.Free)
			{
				AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>" + m_CSign.PriceType + " Price");
				AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", m_CSign.Price.ToString());
				AddButton(width / 2 - 5, y + 5, 0x2716, "Price", Price);

				AddHtml(0, y += 20, width, "<CENTER>Suggest");
				AddButton(width / 2 - 50, y + 3, 0x2716, "Suggest", SuggestPrice);
				AddButton(width / 2 + 40, y + 3, 0x2716, "Suggest", SuggestPrice);
			}

			string helptext = string.Format("   Now you get to set the price for the home.  Remember, if this is a " +
											"rental home, the system will charge them this amount for every period!  Luckily the Suggestion " +
											"takes this into account.  If you don't feel like guessing, let the system suggest a price for you.  " +
											"You can also give the home away with the Free option.");

			AddHtml(10, y += 35, width - 20, 130, helptext, false, false);

			y += 130;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - (m_CSign.Owned ? 2 : 1));

			if (!m_CSign.PriceReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void SkillsPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Skill Restictions");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Skill");
			AddTextField(width / 2 + 20, y, 100, 20, 0x480, 0xBBC, "Skill", m_CSign.Skill);
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Amount");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "SkillReq", m_CSign.SkillReq.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Min Total");
			AddTextField(width / 2 + 20, y, 60, 20, 0x480, 0xBBC, "MinTotalSkill", m_CSign.MinTotalSkill.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Max Total");
			AddTextField(width / 2 + 20, y, 60, 20, 0x480, 0xBBC, "MaxTotalSkill", m_CSign.MaxTotalSkill.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			string helptext =
				string.Format("   These settings are all optional.  If you want to restrict who can own " +
							  "this home by their skills, here's the place.  You can specify by the skill name and value, or by " +
							  "player's total skills.");

			AddHtml(10, y += 35, width - 20, 90, helptext, false, false);

			y += 90;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			if (!m_CSign.PriceReady)
			{
				return;
			}
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void OtherPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Other Options");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Young");
			AddButton(width / 2 - 80, y, m_CSign.YoungOnly ? 0xD3 : 0xD2, "Young Only", Young);
			AddButton(width / 2 + 60, y, m_CSign.YoungOnly ? 0xD3 : 0xD2, "Young Only", Young);

			if (!m_CSign.YoungOnly)
			{
				AddHtml(0, y += 25, width, "<CENTER>Innocents");
				AddButton(width / 2 - 80, y, m_CSign.Murderers == Intu.No ? 0xD3 : 0xD2, "No Murderers", Murderers, Intu.No);
				AddButton(width / 2 + 60, y, m_CSign.Murderers == Intu.No ? 0xD3 : 0xD2, "No Murderers", Murderers, Intu.No);
				AddHtml(0, y += 20, width, "<CENTER>Murderers");
				AddButton(width / 2 - 80, y, m_CSign.Murderers == Intu.Yes ? 0xD3 : 0xD2, "Yes Murderers", Murderers,
					Intu.Yes);
				AddButton(width / 2 + 60, y, m_CSign.Murderers == Intu.Yes ? 0xD3 : 0xD2, "Yes Murderers", Murderers,
					Intu.Yes);
				AddHtml(0, y += 20, width, "<CENTER>All");
				AddButton(width / 2 - 80, y, m_CSign.Murderers == Intu.Neither ? 0xD3 : 0xD2, "Neither Murderers",
					Murderers, Intu.Neither);
				AddButton(width / 2 + 60, y, m_CSign.Murderers == Intu.Neither ? 0xD3 : 0xD2, "Neither Murderers",
					Murderers, Intu.Neither);
			}

			AddHtml(0, y += 25, width, "<CENTER>Relock doors on demolish");
			AddButton(width / 2 - 110, y, m_CSign.Relock ? 0xD3 : 0xD2, "Relock", Relock);
			AddButton(width / 2 + 90, y, m_CSign.Relock ? 0xD3 : 0xD2, "Relock", Relock);

			string helptext =
				string.Format("   These options are also optional.  With the young setting, you can restrict " +
							  "who can buy the home to young players only.  Similarly, you can specify whether murderers or innocents are " +
							  " allowed to own the home.  You can also specify whether the doors within the " +
							  "home are locked when the owner demolishes their property.");

			AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

			y += 180;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);

			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)m_CPage + 1);
		}

		private void OtherPage2(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Other Options 2");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Force Public");
			AddButton(width / 2 - 110, y, m_CSign.ForcePublic ? 0xD3 : 0xD2, "Public", ForcePublic);
			AddButton(width / 2 + 90, y, m_CSign.ForcePublic ? 0xD3 : 0xD2, "Public", ForcePublic);

			AddHtml(0, y += 25, width, "<CENTER>Force Private");
			AddButton(width / 2 - 110, y, m_CSign.ForcePrivate ? 0xD3 : 0xD2, "Private", ForcePrivate);
			AddButton(width / 2 + 90, y, m_CSign.ForcePrivate ? 0xD3 : 0xD2, "Private", ForcePrivate);

			//AddHtml(0, y += 25, width, "<CENTER>No Trading");
			//AddButton(width / 2 - 110, y, c_Sign.NoTrade ? 0xD3 : 0xD2, "NoTrade", NoTrade);
			//AddButton(width / 2 + 90, y, c_Sign.NoTrade ? 0xD3 : 0xD2, "NoTrade", NoTrade);

			AddHtml(0, y += 25, width, "<CENTER>No Banning");
			AddButton(width / 2 - 110, y, m_CSign.NoBanning ? 0xD3 : 0xD2, "NoBan", NoBan);
			AddButton(width / 2 + 90, y, m_CSign.NoBanning ? 0xD3 : 0xD2, "NoBan", NoBan);

			var helptext =
				string.Format(
					"   Another page of optional options!  Sometimes houses have features you don't want players using.  " +
					"So here you can force homes to be private or public.  You can also prevent trading of the home.  Lastly, you can remove their ability to ban players.");

			AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

			y += 180;

			AddHtml(30, y += 15, 80, "Previous");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)m_CPage - 1);
		}

		private bool SkillNameExists(string text)
		{
			try
			{
				var index = (SkillName)Enum.Parse(typeof(SkillName), text, true);
				return true;
			}
			catch
			{
				Owner.SendMessage("You provided an invalid skill name.");
				return false;
			}
		}

		private void ChangePage(object obj)
		{
			if (m_CSign == null)
			{
				return;
			}

			if (obj is not int)
			{
				return;
			}

			m_CPage = (Page)(int)obj;

			m_CSign.ClearPreview();

			NewGump();
		}

		private void Name()
		{
			m_CSign.Name = GetTextField("Name");
			Owner.SendMessage("Name set!");
			NewGump();
		}

		private void Goto()
		{
			Owner.Location = m_CSign.Location;
			Owner.Z += 5;
			Owner.Map = m_CSign.Map;

			NewGump();
		}

		private void Quick()
		{
			m_CQuick = !m_CQuick;
			NewGump();
		}

		private void BanLocSelect()
		{
			Owner.SendMessage("Target the ban location.");
			Owner.Target = new InternalTarget(this, m_CSign, TargetType.BanLoc);
		}

		private void SignLocSelect()
		{
			Owner.SendMessage("Target the location for the home sign.");
			Owner.Target = new InternalTarget(this, m_CSign, TargetType.SignLoc);
		}

		private void MinZSelect()
		{
			Owner.SendMessage("Target the base floor.");
			Owner.Target = new InternalTarget(this, m_CSign, TargetType.MinZ);
		}

		private void MaxZSelect()
		{
			Owner.SendMessage("Target the highest floor.");
			Owner.Target = new InternalTarget(this, m_CSign, TargetType.MaxZ);
		}

		private void Young()
		{
			m_CSign.YoungOnly = !m_CSign.YoungOnly;
			NewGump();
		}

		private void Murderers(object obj)
		{
			if (obj is not Intu)
			{
				return;
			}

			m_CSign.Murderers = (Intu)obj;

			NewGump();
		}

		private void Relock()
		{
			m_CSign.Relock = !m_CSign.Relock;
			NewGump();
		}

		private void ForcePrivate()
		{
			m_CSign.ForcePrivate = !m_CSign.ForcePrivate;
			NewGump();
		}

		private void ForcePublic()
		{
			m_CSign.ForcePublic = !m_CSign.ForcePublic;
			NewGump();
		}

		//private void NoTrade()
		//{
		//    c_Sign.NoTrade = !c_Sign.NoTrade;
		//    NewGump();
		//}

		private void NoBan()
		{
			m_CSign.NoBanning = !m_CSign.NoBanning;
			NewGump();
		}

		private void KeepItems()
		{
			m_CSign.KeepItems = !m_CSign.KeepItems;
			NewGump();
		}

		private void LeaveItems()
		{
			m_CSign.LeaveItems = !m_CSign.LeaveItems;
			NewGump();
		}

		private void ItemsPrice()
		{
			m_CSign.ItemsPrice = GetTextFieldInt("ItemsPrice");
			Owner.SendMessage("Item Price set!");
			NewGump();
		}

		private void RecurRent()
		{
			m_CSign.RecurRent = !m_CSign.RecurRent;
			NewGump();
		}

		private void RentToOwn()
		{
			m_CSign.RentToOwn = !m_CSign.RentToOwn;
			NewGump();
		}

		private void Skill()
		{
			if (GetTextField("Skill") != "" && SkillNameExists(GetTextField("Skill")))
			{
				m_CSign.Skill = GetTextField("Skill");
			}
			else
			{
				m_CSign.Skill = "";
			}

			m_CSign.SkillReq = GetTextFieldInt("SkillReq");
			m_CSign.MinTotalSkill = GetTextFieldInt("MinTotalSkill");
			m_CSign.MaxTotalSkill = GetTextFieldInt("MaxTotalSkill");

			Owner.SendMessage("Skill info set!");

			NewGump();
		}

		private void Claim()
		{
			_ = new TownHouseConfirmGump(Owner, m_CSign);
			OnClose();
		}

		private void SuggestLocSec()
		{
			int price = m_CSign.CalcVolume() * General.SuggestionFactor;
			m_CSign.Secures = price / 75;
			m_CSign.Locks = m_CSign.Secures / 2;

			NewGump();
		}

		private void Secures()
		{
			m_CSign.Secures = GetTextFieldInt("Secures");
			Owner.SendMessage("Secures set!");
			NewGump();
		}

		private void Lockdowns()
		{
			m_CSign.Locks = GetTextFieldInt("Lockdowns");
			Owner.SendMessage("Lockdowns set!");
			NewGump();
		}

		private void SuggestPrice()
		{
			m_CSign.Price = m_CSign.CalcVolume() * General.SuggestionFactor;

			if (m_CSign.RentByTime == TimeSpan.FromDays(1))
			{
				m_CSign.Price /= 60;
			}
			if (m_CSign.RentByTime == TimeSpan.FromDays(7))
			{
				m_CSign.Price = (int)(m_CSign.Price / 8.57);
			}
			if (m_CSign.RentByTime == TimeSpan.FromDays(30))
			{
				m_CSign.Price /= 2;
			}

			NewGump();
		}

		private void Price()
		{
			m_CSign.Price = GetTextFieldInt("Price");
			Owner.SendMessage("Price set!");
			NewGump();
		}

		private void Free()
		{
			m_CSign.Free = !m_CSign.Free;
			NewGump();
		}

		private void AddBlock()
		{
			if (m_CSign == null)
			{
				return;
			}

			Owner.SendMessage("Target the north western corner.");
			Owner.Target = new InternalTarget(this, m_CSign, TargetType.BlockOne);
		}

		private void ClearAll()
		{
			if (m_CSign == null)
			{
				return;
			}

			m_CSign.Blocks.Clear();
			m_CSign.ClearPreview();
			m_CSign.UpdateBlocks();

			NewGump();
		}

		private void PriceUp()
		{
			m_CSign.NextPriceType();
			NewGump();
		}

		private void PriceDown()
		{
			m_CSign.PrevPriceType();
			NewGump();
		}

		protected override void OnClose()
		{
			m_CSign.ClearPreview();
		}


		private class InternalTarget : Target
		{
			private readonly TownHouseSetupGump m_CGump;
			private readonly TownHouseSign m_CSign;
			private readonly TargetType m_CType;
			private readonly Point3D m_CBoundOne;

			public InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type)
				: this(gump, sign, type, Point3D.Zero)
			{
			}

			private InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type, Point3D point)
				: base(20, true, TargetFlags.None)
			{
				m_CGump = gump;
				m_CSign = sign;
				m_CType = type;
				m_CBoundOne = point;
			}

			protected override void OnTarget(Mobile m, object o)
			{
				var point = (IPoint3D)o;

				switch (m_CType)
				{
					case TargetType.BanLoc:
						m_CSign.BanLoc = new Point3D(point.X, point.Y, point.Z);
						m_CGump.NewGump();
						break;

					case TargetType.SignLoc:
						m_CSign.SignLoc = new Point3D(point.X, point.Y, point.Z);
						m_CSign.MoveToWorld(m_CSign.SignLoc, m_CSign.Map);
						m_CSign.Z -= 5;
						m_CSign.ShowSignPreview();
						m_CGump.NewGump();
						break;

					case TargetType.MinZ:
						m_CSign.MinZ = point.Z;

						if (m_CSign.MaxZ < m_CSign.MinZ + 19)
						{
							m_CSign.MaxZ = point.Z + 19;
						}

						if (m_CSign.MaxZ == short.MaxValue)
						{
							m_CSign.MaxZ = point.Z + 19;
						}

						m_CGump.NewGump();
						break;

					case TargetType.MaxZ:
						m_CSign.MaxZ = point.Z + 19;

						if (m_CSign.MinZ > m_CSign.MaxZ)
						{
							m_CSign.MinZ = point.Z;
						}

						m_CGump.NewGump();
						break;

					case TargetType.BlockOne:
						m.SendMessage("Now target the south eastern corner.");
						m.Target = new InternalTarget(m_CGump, m_CSign, TargetType.BlockTwo,
							new Point3D(point.X, point.Y, point.Z));
						break;

					case TargetType.BlockTwo:
						m_CSign.Blocks.Add(
							FixRect(new Rectangle2D(m_CBoundOne, new Point3D(point.X + 1, point.Y + 1, point.Z))));
						m_CSign.UpdateBlocks();
						m_CSign.ShowAreaPreview(m);
						m_CGump.NewGump();
						break;
					default:
						throw new ArgumentOutOfRangeException
						{
							HelpLink = null,
							HResult = 0,
							Source = null
						};
				}
			}

			protected override void OnTargetCancel(Mobile m, TargetCancelType cancelType)
			{
				m_CGump.NewGump();
			}
		}
	}
}
