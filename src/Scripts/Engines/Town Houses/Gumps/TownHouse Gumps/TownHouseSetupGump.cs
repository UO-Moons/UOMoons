using Server.Targeting;
using System;
using System.Globalization;

namespace Server.Engines.TownHouses;

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

	private readonly TownHouseSign _cSign;
	private Page _cPage;
	private bool _cQuick;

	public TownHouseSetupGump(Mobile m, TownHouseSign sign) : base(m, 50, 50)
	{
		m.CloseGump(typeof(TownHouseSetupGump));

		_cSign = sign;
	}

	protected override void BuildGump()
	{
		if (_cSign == null)
		{
			return;
		}

		const int width = 300;
		int y = 0;

		if (_cQuick)
		{
			QuickPage(width, ref y);
		}
		else
		{
			switch (_cPage)
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

			BuildTabs(ref y);
		}

		AddBackgroundZero(0, 0, width, y += 30, 0x13BE);

		if (!_cSign.PriceReady || _cSign.Owned)
		{
			return;
		}
		AddBackground(width / 2 - 50, y, 100, 30, 0x13BE);
		AddHtml(width / 2 - 50 + 25, y + 5, 100, "Claim Home");
		AddButton(width / 2 - 50 + 5, y + 10, 0x837, 0x838, "Claim", Claim);
	}

	private void BuildTabs(ref int y)
	{
		int x = 20;

		y += 30;

		AddButton(x - 5, y - 3, 0x768, "Quick", Quick);
		AddLabel(x, y - 3, _cQuick ? 0x34 : 0x47E, "Q");

		AddButton(x += 20, y, _cPage == Page.Welcome ? 0x939 : 0x93A, "Welcome Page", ChangePage, 0);
		AddButton(x += 20, y, _cPage == Page.Blocks ? 0x939 : 0x93A, "Blocks Page", ChangePage, 1);

		if (_cSign.BlocksReady)
		{
			AddButton(x += 20, y, _cPage == Page.Floors ? 0x939 : 0x93A, "Floors Page", ChangePage, 2);
		}

		if (_cSign.FloorsReady)
		{
			AddButton(x += 20, y, _cPage == Page.Sign ? 0x939 : 0x93A, "Sign Page", ChangePage, 3);
		}

		if (_cSign.SignReady)
		{
			AddButton(x += 20, y, _cPage == Page.Ban ? 0x939 : 0x93A, "Ban Page", ChangePage, 4);
		}

		if (_cSign.BanReady)
		{
			AddButton(x += 20, y, _cPage == Page.LocSec ? 0x939 : 0x93A, "LocSec Page", ChangePage, 5);
		}

		if (_cSign.LocSecReady)
		{
			AddButton(x += 20, y, _cPage == Page.Items ? 0x939 : 0x93A, "Items Page", ChangePage, 6);

			if (!_cSign.Owned)
			{
				AddButton(x += 20, y, _cPage == Page.Length ? 0x939 : 0x93A, "Length Page", ChangePage, 7);
			}
			else
			{
				x += 20;
			}

			AddButton(x += 20, y, _cPage == Page.Price ? 0x939 : 0x93A, "Price Page", ChangePage, 8);
		}

		if (!_cSign.PriceReady)
		{
			return;
		}
		AddButton(x += 20, y, _cPage == Page.Skills ? 0x939 : 0x93A, "Skills Page", ChangePage, 9);
		AddButton(x += 20, y, _cPage == Page.Other ? 0x939 : 0x93A, "Other Page", ChangePage, 10);
		AddButton(x + 20, y, _cPage == Page.Other2 ? 0x939 : 0x93A, "Other Page 2", ChangePage, 11);
	}

	private void QuickPage(int width, ref int y)
	{
		_cSign.ClearPreview();

		AddHtml(0, y += 10, width, "<CENTER>Quick Setup");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddButton(5, 5, 0x768, "Quick", Quick);
		AddLabel(10, 5, _cQuick ? 0x34 : 0x47E, "Q");

		AddHtml(0, y += 25, width / 2 - 55, "<DIV ALIGN=RIGHT>Name");
		AddTextField(width / 2 - 15, y, 100, 20, 0x480, 0xBBC, "Name", _cSign.Name);
		AddButton(width / 2 - 40, y + 3, 0x2716, "Name", Name);

		AddHtml(0, y += 25, width / 2, "<CENTER>Add Area");
		AddButton(width / 4 - 50, y + 3, 0x2716, "Add Area", AddBlock);
		AddButton(width / 4 + 40, y + 3, 0x2716, "Add Area", AddBlock);

		AddHtml(width / 2, y, width / 2, "<CENTER>Clear All");
		AddButton(width / 4 * 3 - 50, y + 3, 0x2716, "ClearAll", ClearAll);
		AddButton(width / 4 * 3 + 40, y + 3, 0x2716, "ClearAll", ClearAll);

		AddHtml(0, y += 25, width, "<CENTER>Base Floor: " + _cSign.MinZ);
		AddButton(width / 2 - 80, y + 3, 0x2716, "Base Floor", MinZSelect);
		AddButton(width / 2 + 70, y + 3, 0x2716, "Base Floor", MinZSelect);

		AddHtml(0, y += 17, width, "<CENTER>Top Floor: " + _cSign.MaxZ);
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
		AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Secures", _cSign.Secures.ToString());
		AddButton(width / 2 - 5, y + 3, 0x2716, "Secures", Secures);

		AddHtml(0, y += 22, width / 2 - 20, "<DIV ALIGN=RIGHT>Lockdowns");
		AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Lockdowns", _cSign.Locks.ToString());
		AddButton(width / 2 - 5, y + 3, 0x2716, "Lockdowns", Lockdowns);

		AddHtml(0, y += 25, width, "<CENTER>Give buyer items in home");
		AddButton(width / 2 - 110, y, _cSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);
		AddButton(width / 2 + 90, y, _cSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);

		if (_cSign.KeepItems)
		{
			AddHtml(0, y += 25, width / 2 - 25, "<DIV ALIGN=RIGHT>At cost");
			AddTextField(width / 2 + 15, y, 70, 20, 0x480, 0xBBC, "ItemsPrice", _cSign.ItemsPrice.ToString());
			AddButton(width / 2 - 10, y + 5, 0x2716, "ItemsPrice", ItemsPrice);
		}
		else
		{
			AddHtml(0, y += 25, width, "<CENTER>Don't delete items");
			AddButton(width / 2 - 110, y, _cSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
			AddButton(width / 2 + 90, y, _cSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
		}

		if (!_cSign.Owned)
		{
			AddHtml(120, y += 25, 50, _cSign.PriceType);
			AddButton(170, y + 8, 0x985, 0x985, "LengthUp", PriceUp);
			AddButton(170, y - 2, 0x983, 0x983, "LengthDown", PriceDown);
		}

		if (_cSign.RentByTime != TimeSpan.Zero)
		{
			AddHtml(0, y += 25, width, "<CENTER>Recurring Rent");
			AddButton(width / 2 - 80, y, _cSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);
			AddButton(width / 2 + 60, y, _cSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);

			if (_cSign.RecurRent)
			{
				AddHtml(0, y += 20, width, "<CENTER>Rent To Own");
				AddButton(width / 2 - 80, y, _cSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
				AddButton(width / 2 + 60, y, _cSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
			}
		}

		AddHtml(0, y += 25, width, "<CENTER>Free");
		AddButton(width / 2 - 80, y, _cSign.Free ? 0xD3 : 0xD2, "Free", Free);
		AddButton(width / 2 + 60, y, _cSign.Free ? 0xD3 : 0xD2, "Free", Free);

		if (_cSign.Free)
		{
			return;
		}
		AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>" + _cSign.PriceType + " Price");
		AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", _cSign.Price.ToString());
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

		string helptext;

		AddHtml(0, y += 25, width / 2 - 55, "<DIV ALIGN=RIGHT>Name");
		AddTextField(width / 2 - 15, y, 100, 20, 0x480, 0xBBC, "Name", _cSign.Name);
		AddButton(width / 2 - 40, y + 3, 0x2716, "Name", Name);

		if (_cSign != null && _cSign.Map != Map.Internal && _cSign.RootParent == null)
		{
			AddHtml(0, y += 25, width, "<CENTER>Goto");
			AddButton(width / 2 - 50, y + 3, 0x2716, "Goto", Goto);
			AddButton(width / 2 + 40, y + 3, 0x2716, "Goto", Goto);
		}

		if (_cSign.Owned)
		{
			helptext = $"  This home is owned by {_cSign.House.Owner.Name}, so be aware that changing anything " +
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
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void BlocksPage(int width, ref int y)
	{
		if (_cSign == null)
		{
			return;
		}

		_cSign.ShowAreaPreview(Owner);

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
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.BlocksReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void FloorsPage(int width, ref int y)
	{
		_cSign.ShowFloorsPreview(Owner);

		AddHtml(0, y += 10, width, "<CENTER>Floors");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Base Floor: " + _cSign.MinZ);
		AddButton(width / 2 - 80, y + 3, 0x2716, "Base Floor", MinZSelect);
		AddButton(width / 2 + 70, y + 3, 0x2716, "Base Floor", MinZSelect);

		AddHtml(0, y += 20, width, "<CENTER>Top Floor: " + _cSign.MaxZ);
		AddButton(width / 2 - 80, y + 3, 0x2716, "Top Floor", MaxZSelect);
		AddButton(width / 2 + 70, y + 3, 0x2716, "Top Floor", MaxZSelect);

		string helptext = string.Format("   Now you will need to target the floors you wish to sell.  " +
		                                "If you only want one floor, you can skip targeting the top floor.  Everything within the base " +
		                                "and highest floor will come with the home, and the more floors, the higher the cost later on.");

		AddHtml(10, y += 35, width - 20, 110, helptext, false, false);

		y += 110;

		AddHtml(30, y += 15, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.FloorsReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void SignPage(int width, ref int y)
	{
		if (_cSign == null)
		{
			return;
		}

		_cSign.ShowSignPreview();

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
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.SignReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void BanPage(int width, ref int y)
	{
		if (_cSign == null)
		{
			return;
		}

		_cSign.ShowBanPreview();

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
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.BanReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
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
		AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Secures", _cSign.Secures.ToString());
		AddButton(width / 2 - 5, y + 3, 0x2716, "Secures", Secures);

		AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Lockdowns");
		AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Lockdowns", _cSign.Locks.ToString());
		AddButton(width / 2 - 5, y + 3, 0x2716, "Lockdowns", Lockdowns);

		string helptext = string.Format("   With this step you'll set the amount of storage for the home, or let " +
		                                "the system do so for you using the Suggest button.  In general, players get half the number of lockdowns " +
		                                "as secure storage.");

		AddHtml(10, y += 35, width - 20, 90, helptext, false, false);

		y += 90;

		AddHtml(30, y += 15, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.LocSecReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void ItemsPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Decoration Items");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Give buyer items in home");
		AddButton(width / 2 - 110, y, _cSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);
		AddButton(width / 2 + 90, y, _cSign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);

		if (_cSign.KeepItems)
		{
			AddHtml(0, y += 25, width / 2 - 25, "<DIV ALIGN=RIGHT>At cost");
			AddTextField(width / 2 + 15, y, 70, 20, 0x480, 0xBBC, "ItemsPrice", _cSign.ItemsPrice.ToString());
			AddButton(width / 2 - 10, y + 5, 0x2716, "ItemsPrice", ItemsPrice);
		}
		else
		{
			AddHtml(0, y += 25, width, "<CENTER>Don't delete items");
			AddButton(width / 2 - 110, y, _cSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
			AddButton(width / 2 + 90, y, _cSign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
		}

		var helptext =
			string.Format("   By default, the system will delete all items non-static items already " +
			              "in the home at the time of purchase.  These items are commonly referred to as Decoration Items. " +
			              "They do not include home addons, like forges and the like.  They do include containers.  You can " +
			              "allow players to keep these items by saying so here, and you may also charge them to do so!");

		AddHtml(10, y += 35, width - 20, 160, helptext, false, false);

		y += 160;

		AddHtml(30, y += 15, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.ItemsReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + (_cSign.Owned ? 2 : 1));
	}

	private void LengthPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Buy or Rent");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(120, y += 25, 50, _cSign.PriceType);
		AddButton(170, y + 8, 0x985, 0x985, "LengthUp", PriceUp);
		AddButton(170, y - 2, 0x983, 0x983, "LengthDown", PriceDown);

		if (_cSign.RentByTime != TimeSpan.Zero)
		{
			AddHtml(0, y += 25, width, "<CENTER>Recurring Rent");
			AddButton(width / 2 - 80, y, _cSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);
			AddButton(width / 2 + 60, y, _cSign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);

			if (_cSign.RecurRent)
			{
				AddHtml(0, y += 20, width, "<CENTER>Rent To Own");
				AddButton(width / 2 - 80, y, _cSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
				AddButton(width / 2 + 60, y, _cSign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
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
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.LengthReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void PricePage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Price");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Free");
		AddButton(width / 2 - 80, y, _cSign.Free ? 0xD3 : 0xD2, "Free", Free);
		AddButton(width / 2 + 60, y, _cSign.Free ? 0xD3 : 0xD2, "Free", Free);

		if (!_cSign.Free)
		{
			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>" + _cSign.PriceType + " Price");
			AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", _cSign.Price.ToString());
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
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - (_cSign.Owned ? 2 : 1));

		if (!_cSign.PriceReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void SkillsPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Skill Restictions");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Skill");
		AddTextField(width / 2 + 20, y, 100, 20, 0x480, 0xBBC, "Skill", _cSign.Skill);
		AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

		AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Amount");
		AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "SkillReq", _cSign.SkillReq.ToString(CultureInfo.InvariantCulture));
		AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

		AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Min Total");
		AddTextField(width / 2 + 20, y, 60, 20, 0x480, 0xBBC, "MinTotalSkill", _cSign.MinTotalSkill.ToString());
		AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

		AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Max Total");
		AddTextField(width / 2 + 20, y, 60, 20, 0x480, 0xBBC, "MaxTotalSkill", _cSign.MaxTotalSkill.ToString());
		AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

		string helptext =
			string.Format("   These settings are all optional.  If you want to restrict who can own " +
			              "this home by their skills, here's the place.  You can specify by the skill name and value, or by " +
			              "player's total skills.");

		AddHtml(10, y += 35, width - 20, 90, helptext, false, false);

		y += 90;

		AddHtml(30, y += 15, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (!_cSign.PriceReady)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void OtherPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Other Options");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Young");
		AddButton(width / 2 - 80, y, _cSign.YoungOnly ? 0xD3 : 0xD2, "Young Only", Young);
		AddButton(width / 2 + 60, y, _cSign.YoungOnly ? 0xD3 : 0xD2, "Young Only", Young);

		if (!_cSign.YoungOnly)
		{
			AddHtml(0, y += 25, width, "<CENTER>Innocents");
			AddButton(width / 2 - 80, y, _cSign.Murderers == Intu.No ? 0xD3 : 0xD2, "No Murderers", Murderers, Intu.No);
			AddButton(width / 2 + 60, y, _cSign.Murderers == Intu.No ? 0xD3 : 0xD2, "No Murderers", Murderers, Intu.No);
			AddHtml(0, y += 20, width, "<CENTER>Murderers");
			AddButton(width / 2 - 80, y, _cSign.Murderers == Intu.Yes ? 0xD3 : 0xD2, "Yes Murderers", Murderers,
				Intu.Yes);
			AddButton(width / 2 + 60, y, _cSign.Murderers == Intu.Yes ? 0xD3 : 0xD2, "Yes Murderers", Murderers,
				Intu.Yes);
			AddHtml(0, y += 20, width, "<CENTER>All");
			AddButton(width / 2 - 80, y, _cSign.Murderers == Intu.Neither ? 0xD3 : 0xD2, "Neither Murderers",
				Murderers, Intu.Neither);
			AddButton(width / 2 + 60, y, _cSign.Murderers == Intu.Neither ? 0xD3 : 0xD2, "Neither Murderers",
				Murderers, Intu.Neither);
		}

		AddHtml(0, y += 25, width, "<CENTER>Relock doors on demolish");
		AddButton(width / 2 - 110, y, _cSign.Relock ? 0xD3 : 0xD2, "Relock", Relock);
		AddButton(width / 2 + 90, y, _cSign.Relock ? 0xD3 : 0xD2, "Relock", Relock);

		string helptext =
			string.Format("   These options are also optional.  With the young setting, you can restrict " +
			              "who can buy the home to young players only.  Similarly, you can specify whether murderers or innocents are " +
			              " allowed to own the home.  You can also specify whether the doors within the " +
			              "home are locked when the owner demolishes their property.");

		AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

		y += 180;

		AddHtml(30, y += 15, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void OtherPage2(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Other Options 2");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Force Public");
		AddButton(width / 2 - 110, y, _cSign.ForcePublic ? 0xD3 : 0xD2, "Public", ForcePublic);
		AddButton(width / 2 + 90, y, _cSign.ForcePublic ? 0xD3 : 0xD2, "Public", ForcePublic);

		AddHtml(0, y += 25, width, "<CENTER>Force Private");
		AddButton(width / 2 - 110, y, _cSign.ForcePrivate ? 0xD3 : 0xD2, "Private", ForcePrivate);
		AddButton(width / 2 + 90, y, _cSign.ForcePrivate ? 0xD3 : 0xD2, "Private", ForcePrivate);

		//AddHtml(0, y += 25, width, "<CENTER>No Trading");
		//AddButton(width / 2 - 110, y, c_Sign.NoTrade ? 0xD3 : 0xD2, "NoTrade", NoTrade);
		//AddButton(width / 2 + 90, y, c_Sign.NoTrade ? 0xD3 : 0xD2, "NoTrade", NoTrade);

		AddHtml(0, y += 25, width, "<CENTER>No Banning");
		AddButton(width / 2 - 110, y, _cSign.NoBanning ? 0xD3 : 0xD2, "NoBan", NoBan);
		AddButton(width / 2 + 90, y, _cSign.NoBanning ? 0xD3 : 0xD2, "NoBan", NoBan);

		var helptext =
			string.Format(
				"   Another page of optional options!  Sometimes houses have features you don't want players using.  " +
				"So here you can force homes to be private or public.  You can also prevent trading of the home.  Lastly, you can remove their ability to ban players.");

		AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

		y += 180;

		AddHtml(30, y += 15, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);
	}

	private bool SkillNameExists(string text)
	{
		try
		{
			var unused = (SkillName)Enum.Parse(typeof(SkillName), text, true);
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
		if (_cSign == null)
		{
			return;
		}

		if (obj is not int i)
		{
			return;
		}

		_cPage = (Page)i;

		_cSign.ClearPreview();

		NewGump();
	}

	private void Name()
	{
		_cSign.Name = GetTextField("Name");
		Owner.SendMessage("Name set!");
		NewGump();
	}

	private void Goto()
	{
		Owner.Location = _cSign.Location;
		Owner.Z += 5;
		Owner.Map = _cSign.Map;

		NewGump();
	}

	private void Quick()
	{
		_cQuick = !_cQuick;
		NewGump();
	}

	private void BanLocSelect()
	{
		Owner.SendMessage("Target the ban location.");
		Owner.Target = new InternalTarget(this, _cSign, TargetType.BanLoc);
	}

	private void SignLocSelect()
	{
		Owner.SendMessage("Target the location for the home sign.");
		Owner.Target = new InternalTarget(this, _cSign, TargetType.SignLoc);
	}

	private void MinZSelect()
	{
		Owner.SendMessage("Target the base floor.");
		Owner.Target = new InternalTarget(this, _cSign, TargetType.MinZ);
	}

	private void MaxZSelect()
	{
		Owner.SendMessage("Target the highest floor.");
		Owner.Target = new InternalTarget(this, _cSign, TargetType.MaxZ);
	}

	private void Young()
	{
		_cSign.YoungOnly = !_cSign.YoungOnly;
		NewGump();
	}

	private void Murderers(object obj)
	{
		if (obj is not Intu intu)
		{
			return;
		}

		_cSign.Murderers = intu;

		NewGump();
	}

	private void Relock()
	{
		_cSign.Relock = !_cSign.Relock;
		NewGump();
	}

	private void ForcePrivate()
	{
		_cSign.ForcePrivate = !_cSign.ForcePrivate;
		NewGump();
	}

	private void ForcePublic()
	{
		_cSign.ForcePublic = !_cSign.ForcePublic;
		NewGump();
	}

	//private void NoTrade()
	//{
	//    c_Sign.NoTrade = !c_Sign.NoTrade;
	//    NewGump();
	//}

	private void NoBan()
	{
		_cSign.NoBanning = !_cSign.NoBanning;
		NewGump();
	}

	private void KeepItems()
	{
		_cSign.KeepItems = !_cSign.KeepItems;
		NewGump();
	}

	private void LeaveItems()
	{
		_cSign.LeaveItems = !_cSign.LeaveItems;
		NewGump();
	}

	private void ItemsPrice()
	{
		_cSign.ItemsPrice = GetTextFieldInt("ItemsPrice");
		Owner.SendMessage("Item Price set!");
		NewGump();
	}

	private void RecurRent()
	{
		_cSign.RecurRent = !_cSign.RecurRent;
		NewGump();
	}

	private void RentToOwn()
	{
		_cSign.RentToOwn = !_cSign.RentToOwn;
		NewGump();
	}

	private void Skill()
	{
		if (GetTextField("Skill") != "" && SkillNameExists(GetTextField("Skill")))
		{
			_cSign.Skill = GetTextField("Skill");
		}
		else
		{
			_cSign.Skill = "";
		}

		_cSign.SkillReq = GetTextFieldInt("SkillReq");
		_cSign.MinTotalSkill = GetTextFieldInt("MinTotalSkill");
		_cSign.MaxTotalSkill = GetTextFieldInt("MaxTotalSkill");

		Owner.SendMessage("Skill info set!");

		NewGump();
	}

	private void Claim()
	{
		_ = new TownHouseConfirmGump(Owner, _cSign);
		OnClose();
	}

	private void SuggestLocSec()
	{
		int price = _cSign.CalcVolume() * General.SuggestionFactor;
		_cSign.Secures = price / 75;
		_cSign.Locks = _cSign.Secures / 2;

		NewGump();
	}

	private void Secures()
	{
		_cSign.Secures = GetTextFieldInt("Secures");
		Owner.SendMessage("Secures set!");
		NewGump();
	}

	private void Lockdowns()
	{
		_cSign.Locks = GetTextFieldInt("Lockdowns");
		Owner.SendMessage("Lockdowns set!");
		NewGump();
	}

	private void SuggestPrice()
	{
		_cSign.Price = _cSign.CalcVolume() * General.SuggestionFactor;

		if (_cSign.RentByTime == TimeSpan.FromDays(1))
		{
			_cSign.Price /= 60;
		}
		if (_cSign.RentByTime == TimeSpan.FromDays(7))
		{
			_cSign.Price = (int)(_cSign.Price / 8.57);
		}
		if (_cSign.RentByTime == TimeSpan.FromDays(30))
		{
			_cSign.Price /= 2;
		}

		NewGump();
	}

	private void Price()
	{
		_cSign.Price = GetTextFieldInt("Price");
		Owner.SendMessage("Price set!");
		NewGump();
	}

	private void Free()
	{
		_cSign.Free = !_cSign.Free;
		NewGump();
	}

	private void AddBlock()
	{
		if (_cSign == null)
		{
			return;
		}

		Owner.SendMessage("Target the north western corner.");
		Owner.Target = new InternalTarget(this, _cSign, TargetType.BlockOne);
	}

	private void ClearAll()
	{
		if (_cSign == null)
		{
			return;
		}

		_cSign.Blocks.Clear();
		_cSign.ClearPreview();
		_cSign.UpdateBlocks();

		NewGump();
	}

	private void PriceUp()
	{
		_cSign.NextPriceType();
		NewGump();
	}

	private void PriceDown()
	{
		_cSign.PrevPriceType();
		NewGump();
	}

	protected override void OnClose()
	{
		_cSign.ClearPreview();
	}


	private class InternalTarget : Target
	{
		private readonly TownHouseSetupGump _cGump;
		private readonly TownHouseSign _cSign;
		private readonly TargetType _cType;
		private readonly Point3D _cBoundOne;

		public InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type)
			: this(gump, sign, type, Point3D.Zero)
		{
		}

		private InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type, Point3D point)
			: base(20, true, TargetFlags.None)
		{
			_cGump = gump;
			_cSign = sign;
			_cType = type;
			_cBoundOne = point;
		}

		protected override void OnTarget(Mobile m, object o)
		{
			var point = (IPoint3D)o;

			switch (_cType)
			{
				case TargetType.BanLoc:
					_cSign.BanLoc = new Point3D(point.X, point.Y, point.Z);
					_cGump.NewGump();
					break;

				case TargetType.SignLoc:
					_cSign.SignLoc = new Point3D(point.X, point.Y, point.Z);
					_cSign.MoveToWorld(_cSign.SignLoc, _cSign.Map);
					_cSign.Z -= 5;
					_cSign.ShowSignPreview();
					_cGump.NewGump();
					break;

				case TargetType.MinZ:
					_cSign.MinZ = point.Z;

					if (_cSign.MaxZ < _cSign.MinZ + 19)
					{
						_cSign.MaxZ = point.Z + 19;
					}

					if (_cSign.MaxZ == short.MaxValue)
					{
						_cSign.MaxZ = point.Z + 19;
					}

					_cGump.NewGump();
					break;

				case TargetType.MaxZ:
					_cSign.MaxZ = point.Z + 19;

					if (_cSign.MinZ > _cSign.MaxZ)
					{
						_cSign.MinZ = point.Z;
					}

					_cGump.NewGump();
					break;

				case TargetType.BlockOne:
					m.SendMessage("Now target the south eastern corner.");
					m.Target = new InternalTarget(_cGump, _cSign, TargetType.BlockTwo,
						new Point3D(point.X, point.Y, point.Z));
					break;

				case TargetType.BlockTwo:
					_cSign.Blocks.Add(
						FixRect(new Rectangle2D(_cBoundOne, new Point3D(point.X + 1, point.Y + 1, point.Z))));
					_cSign.UpdateBlocks();
					_cSign.ShowAreaPreview(m);
					_cGump.NewGump();
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
			_cGump.NewGump();
		}
	}
}
