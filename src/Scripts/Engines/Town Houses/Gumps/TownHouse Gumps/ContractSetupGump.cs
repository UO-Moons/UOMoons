using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Engines.TownHouses;

public class ContractSetupGump : GumpPlusLight
{
	private enum Page
	{
		Blocks,
		Floors,
		Sign,
		LocSec,
		Length,
		Price
	}

	private enum TargetType
	{
		SignLoc,
		MinZ,
		MaxZ,
		BlockOne,
		BlockTwo
	}

	private readonly RentalContract _cContract;
	private Page _cPage;

	public ContractSetupGump(Mobile m, RentalContract contract) : base(m, 50, 50)
	{
		m.CloseGump(typeof(ContractSetupGump));

		_cContract = contract;
	}

	protected override void BuildGump()
	{
		const int width = 300;
		int y = 0;

		switch (_cPage)
		{
			case Page.Blocks:
				BlocksPage(width, ref y);
				break;
			case Page.Floors:
				FloorsPage(width, ref y);
				break;
			case Page.Sign:
				SignPage(width, ref y);
				break;
			case Page.LocSec:
				LocSecPage(width, ref y);
				break;
			case Page.Length:
				LengthPage(width, ref y);
				break;
			case Page.Price:
				PricePage(width, ref y);
				break;
		}

		AddBackgroundZero(0, 0, width, y + 40, 0x13BE);
	}

	private void BlocksPage(int width, ref int y)
	{
		if (_cContract == null)
		{
			return;
		}

		_cContract.ShowAreaPreview(Owner);

		AddHtml(0, y += 10, width, "<CENTER>Create the Area");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		y += 25;

		if (!General.HasOtherContract(_cContract.ParentHouse, _cContract))
		{
			AddHtml(60, y, 90, "Entire House");
			AddButton(30, y, _cContract.EntireHouse ? 0xD3 : 0xD2, "Entire House", EntireHouse);
		}

		if (!_cContract.EntireHouse)
		{
			AddHtml(170, y, 70, "Add Area");
			AddButton(240, y, 0x15E1, 0x15E5, "Add Area", AddBlock);

			AddHtml(170, y += 20, 70, "Clear All");
			AddButton(240, y, 0x15E1, 0x15E5, "Clear All", ClearBlocks);
		}

		var helptext = string.Format("   Welcome to the rental contract setup menu!  To begin, you must " +
		                             "first create the area which you wish to sell.  As seen above, there are two ways to do this: " +
		                             "rent the entire house, or parts of it.  As you create the area, a simple preview will show you exactly " +
		                             "what area you've selected so far.  You can make all sorts of odd shapes by using multiple areas!");

		AddHtml(10, y += 35, width - 20, 170, helptext, false, false);

		y += 170;

		if (!_cContract.EntireHouse && _cContract.Blocks.Count == 0)
		{
			return;
		}
		AddHtml(width - 60, y += 20, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage,
			(int)_cPage + (_cContract.EntireHouse ? 4 : 1));
	}

	private void FloorsPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Floors");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(40, y += 25, 80, "Base Floor");
		AddButton(110, y, 0x15E1, 0x15E5, "Base Floor", MinZSelect);

		AddHtml(160, y, 80, "Top Floor");
		AddButton(230, y, 0x15E1, 0x15E5, "Top Floor", MaxZSelect);

		AddHtml(100, y += 25, 100,
			string.Format("{0} total floor{1}", _cContract.Floors > 10 ? "1" : "" + _cContract.Floors,
				_cContract.Floors == 1 || _cContract.Floors > 10 ? "" : "s"));

		string helptext = string.Format("   Now you will need to target the floors you wish to rent out.  " +
		                                "If you only want one floor, you can skip targeting the top floor.  Everything within the base " +
		                                "and highest floor will come with the rental, and the more floors, the higher the cost later on.");

		AddHtml(10, y += 35, width - 20, 120, helptext, false, false);

		y += 120;

		AddHtml(30, y += 20, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (_cContract.MinZ == short.MinValue)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void SignPage(int width, ref int y)
	{
		if (_cContract == null)
		{
			return;
		}

		_cContract.ShowSignPreview();

		AddHtml(0, y += 10, width, "<CENTER>Their Sign Location");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(100, y += 25, 80, "Set Location");
		AddButton(180, y, 0x15E1, 0x15E5, "Sign Loc", SignLocSelect);

		var helptext = string.Format("   With this sign, the rentee will have all the powers an owner has " +
		                             "over their area.  If they use this power to demolish their rental unit, they have broken their " +
		                             "contract and will not receive their security deposit.  They can also ban you from their rental home!");

		AddHtml(10, y += 35, width - 20, 110, helptext, false, false);

		y += 110;

		AddHtml(30, y += 20, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (_cContract.SignLoc != Point3D.Zero)
		{
			AddHtml(width - 60, y, 60, "Next");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
		}
	}

	private void LocSecPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Lockdowns and Secures");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Suggest Secures");
		AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);
		AddButton(width / 2 + 60, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);

		AddHtml(30, y += 25, width / 2 - 20,
			"<DIV ALIGN=RIGHT>Secures (Max: " +
			(General.RemainingSecures(_cContract.ParentHouse) + _cContract.Secures) + ")");
		AddTextField(width / 2 + 50, y, 50, 20, 0x480, 0xBBC, "Secures", _cContract.Secures.ToString());
		AddButton(width / 2 + 25, y + 3, 0x2716, "Secures", Secures);

		AddHtml(30, y += 20, width / 2 - 20,
			"<DIV ALIGN=RIGHT>Lockdowns (Max: " +
			(General.RemainingLocks(_cContract.ParentHouse) + _cContract.Locks) + ")");
		AddTextField(width / 2 + 50, y, 50, 20, 0x480, 0xBBC, "Lockdowns", _cContract.Locks.ToString());
		AddButton(width / 2 + 25, y + 3, 0x2716, "Lockdowns", Lockdowns);

		string helptext =
			string.Format(
				"   Without giving storage, this wouldn't be much of a home!  Here you give them lockdowns " +
				"and secures from your own home.  Use the suggest button for an idea of how much you should give.  Be very careful when " +
				"renting your property: if you use too much storage you begin to use storage you reserved for your clients.  " +
				"You will receive a 48 hour warning when this happens, but after that the contract disappears!");

		AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

		y += 180;

		AddHtml(30, y += 20, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (_cContract.Locks == 0 || _cContract.Secures == 0)
		{
			return;
		}
		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void LengthPage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Time Period");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(120, y += 25, 50, _cContract.PriceType);
		AddButton(170, y + 8, 0x985, "LengthUp", LengthUp);
		AddButton(170, y - 2, 0x983, "LengthDown", LengthDown);

		var helptext =
			$"   Every {_cContract.PriceTypeShort.ToLower()} the bank will automatically transfer the rental cost from them to you.  " +
			"By using the arrows, you can cycle through other time periods to something better fitting your needs.";

		AddHtml(10, y += 35, width - 20, 100, helptext, false, false);

		y += 100;

		AddHtml(30, y += 20, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - (_cContract.EntireHouse ? 4 : 1));

		AddHtml(width - 60, y, 60, "Next");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)_cPage + 1);
	}

	private void PricePage(int width, ref int y)
	{
		AddHtml(0, y += 10, width, "<CENTER>Charge Per Period");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		AddHtml(0, y += 25, width, "<CENTER>Free");
		AddButton(width / 2 - 80, y, _cContract.Free ? 0xD3 : 0xD2, "Free", Free);
		AddButton(width / 2 + 60, y, _cContract.Free ? 0xD3 : 0xD2, "Free", Free);

		if (!_cContract.Free)
		{
			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Per " + _cContract.PriceTypeShort);
			AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", _cContract.Price.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Price", Price);

			AddHtml(0, y += 20, width, "<CENTER>Suggest");
			AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest", SuggestPrice);
			AddButton(width / 2 + 60, y + 3, 0x2716, "Suggest", SuggestPrice);
		}

		var helptext =
			$"   Now you can finalize the contract by including your price per {_cContract.PriceTypeShort}.  " +
			"Once you finalize, the only way you can modify it is to dump it and start a new contract!  By " +
			"using the suggest button, a price will automatically be figured based on the following:<BR>";

		helptext += $"<CENTER>Volume: {_cContract.CalcVolume()}<BR>";
		helptext += $"Cost per unit: {General.SuggestionFactor} gold</CENTER>";
		helptext += "<br>   You may also give this space away for free using the option above.";

		AddHtml(10, y += 35, width - 20, 150, helptext, false, true);

		y += 150;

		AddHtml(30, y += 20, 80, "Previous");
		AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)_cPage - 1);

		if (_cContract.Price == 0)
		{
			return;
		}
		AddHtml(width - 70, y, 60, "Finalize");
		AddButton(width - 30, y, 0x15E1, 0x15E5, "Finalize", FinalizeSetup);
	}

	protected override void OnClose()
	{
		_cContract.ClearPreview();
	}

	private void SuggestPrice()
	{
		if (_cContract == null)
		{
			return;
		}

		_cContract.Price = _cContract.CalcVolume() * General.SuggestionFactor;

		if (_cContract.RentByTime == TimeSpan.FromDays(1))
		{
			_cContract.Price /= 60;
		}
		if (_cContract.RentByTime == TimeSpan.FromDays(7))
		{
			_cContract.Price = (int)(_cContract.Price / 8.57);
		}
		if (_cContract.RentByTime == TimeSpan.FromDays(30))
		{
			_cContract.Price /= 2;
		}

		NewGump();
	}

	private void SuggestLocSec()
	{
		int price = _cContract.CalcVolume() * General.SuggestionFactor;
		_cContract.Secures = price / 75;
		_cContract.Locks = _cContract.Secures / 2;

		_cContract.FixLocSec();

		NewGump();
	}

	private void Price()
	{
		_cContract.Price = GetTextFieldInt("Price");
		Owner.SendMessage("Price set!");
		NewGump();
	}

	private void Secures()
	{
		_cContract.Secures = GetTextFieldInt("Secures");
		Owner.SendMessage("Secures set!");
		NewGump();
	}

	private void Lockdowns()
	{
		_cContract.Locks = GetTextFieldInt("Lockdowns");
		Owner.SendMessage("Lockdowns set!");
		NewGump();
	}

	private void ChangePage(object obj)
	{
		if (_cContract == null || obj is not int i)
		{
			return;
		}

		_cContract.ClearPreview();

		_cPage = (Page)i;

		NewGump();
	}

	private void EntireHouse()
	{
		if (_cContract == null || _cContract.ParentHouse == null)
		{
			return;
		}

		_cContract.EntireHouse = !_cContract.EntireHouse;

		_cContract.ClearPreview();

		if (_cContract.EntireHouse)
		{
			var list = new List<Rectangle2D>();

			bool once = false;
			foreach (Rectangle3D rect in VersionCommand.RegionArea(_cContract.ParentHouse.Region))
			{
				list.Add(new Rectangle2D(new Point2D(rect.Start.X, rect.Start.Y),
					new Point2D(rect.End.X, rect.End.Y)));

				if (once)
				{
					continue;
				}

				if (rect.Start.Z >= rect.End.Z)
				{
					_cContract.MinZ = rect.End.Z;
					_cContract.MaxZ = rect.Start.Z;
				}
				else
				{
					_cContract.MinZ = rect.Start.Z;
					_cContract.MaxZ = rect.End.Z;
				}

				once = true;
			}

			_cContract.Blocks = list;
		}
		else
		{
			_cContract.Blocks.Clear();
			_cContract.MinZ = short.MinValue;
			_cContract.MaxZ = short.MinValue;
		}

		NewGump();
	}

	private void SignLocSelect()
	{
		Owner.Target = new InternalTarget(this, _cContract, TargetType.SignLoc);
	}

	private void MinZSelect()
	{
		Owner.SendMessage("Target the base floor for your rental area.");
		Owner.Target = new InternalTarget(this, _cContract, TargetType.MinZ);
	}


	private void MaxZSelect()
	{
		Owner.SendMessage("Target the highest floor for your rental area.");
		Owner.Target = new InternalTarget(this, _cContract, TargetType.MaxZ);
	}

	private void LengthUp()
	{
		if (_cContract == null)
		{
			return;
		}

		_cContract.NextPriceType();

		if (_cContract.RentByTime == TimeSpan.FromDays(0))
		{
			_cContract.RentByTime = TimeSpan.FromDays(1);
		}

		NewGump();
	}

	private void LengthDown()
	{
		if (_cContract == null)
		{
			return;
		}

		_cContract.PrevPriceType();

		if (_cContract.RentByTime == TimeSpan.FromDays(0))
		{
			_cContract.RentByTime = TimeSpan.FromDays(30);
		}

		NewGump();
	}

	private void Free()
	{
		_cContract.Free = !_cContract.Free;

		NewGump();
	}

	private void AddBlock()
	{
		Owner.SendMessage("Target the north western corner.");
		Owner.Target = new InternalTarget(this, _cContract, TargetType.BlockOne);
	}

	private void ClearBlocks()
	{
		if (_cContract == null)
		{
			return;
		}

		_cContract.Blocks.Clear();

		_cContract.ClearPreview();

		NewGump();
	}

	private void FinalizeSetup()
	{
		if (_cContract == null)
		{
			return;
		}

		if (_cContract.Price == 0)
		{
			Owner.SendMessage("You can't rent the area for 0 gold!");
			NewGump();
			return;
		}

		_cContract.Completed = true;
		_cContract.BanLoc = _cContract.ParentHouse.Region.GoLocation;

		if (_cContract.EntireHouse)
		{
			Point3D point = _cContract.ParentHouse.Sign.Location;
			_cContract.SignLoc = new Point3D(point.X, point.Y, point.Z - 5);
			_cContract.Secures = Core.AOS
				? _cContract.ParentHouse.GetAosMaxSecures()
				: _cContract.ParentHouse.MaxSecures;
			_cContract.Locks = Core.AOS
				? _cContract.ParentHouse.GetAosMaxLockdowns()
				: _cContract.ParentHouse.MaxLockDowns;
		}

		Owner.SendMessage("You have finalized this rental contract.  Now find someone to sign it!");
	}

	private class InternalTarget : Target
	{
		private readonly ContractSetupGump _cGump;
		private readonly RentalContract _cContract;
		private readonly TargetType _cType;
		private readonly Point3D _cBoundOne;

		public InternalTarget(ContractSetupGump gump, RentalContract contract, TargetType type)
			: this(gump, contract, type, Point3D.Zero)
		{
		}

		private InternalTarget(ContractSetupGump gump, RentalContract contract, TargetType type, Point3D point)
			: base(20, true, TargetFlags.None)
		{
			_cGump = gump;
			_cContract = contract;
			_cType = type;
			_cBoundOne = point;
		}

		protected override void OnTarget(Mobile m, object o)
		{
			var point = (IPoint3D)o;

			if (_cContract == null || _cContract.ParentHouse == null)
			{
				return;
			}

			if (!_cContract.ParentHouse.Region.Contains(new Point3D(point.X, point.Y, point.Z)))
			{
				m.SendMessage("You must target within the home.");
				m.Target = new InternalTarget(_cGump, _cContract, _cType, _cBoundOne);
				return;
			}

			switch (_cType)
			{
				case TargetType.SignLoc:
					_cContract.SignLoc = new Point3D(point.X, point.Y, point.Z);
					_cContract.ShowSignPreview();
					_cGump.NewGump();
					break;

				case TargetType.MinZ:
					if (!_cContract.ParentHouse.Region.Contains(new Point3D(point.X, point.Y, point.Z)))
					{
						m.SendMessage("That isn't within your house.");
					}
					else if (_cContract.HasContractedArea(point.Z))
					{
						m.SendMessage("That area is already taken by another rental contract.");
					}
					else
					{
						_cContract.MinZ = point.Z;

						if (_cContract.MaxZ < _cContract.MinZ + 19)
						{
							_cContract.MaxZ = point.Z + 19;
						}
					}

					_cContract.ShowFloorsPreview(m);
					_cGump.NewGump();
					break;

				case TargetType.MaxZ:
					if (!_cContract.ParentHouse.Region.Contains(new Point3D(point.X, point.Y, point.Z)))
					{
						m.SendMessage("That isn't within your house.");
					}
					else if (_cContract.HasContractedArea(point.Z))
					{
						m.SendMessage("That area is already taken by another rental contract.");
					}
					else
					{
						_cContract.MaxZ = point.Z + 19;

						if (_cContract.MinZ > _cContract.MaxZ)
						{
							_cContract.MinZ = point.Z;
						}
					}

					_cContract.ShowFloorsPreview(m);
					_cGump.NewGump();
					break;

				case TargetType.BlockOne:
					m.SendMessage("Now target the south eastern corner.");
					m.Target = new InternalTarget(_cGump, _cContract, TargetType.BlockTwo,
						new Point3D(point.X, point.Y, point.Z));
					break;

				case TargetType.BlockTwo:
					Rectangle2D rect =
						TownHouseSetupGump.FixRect(new Rectangle2D(_cBoundOne,
							new Point3D(point.X + 1, point.Y + 1, point.Z)));

					if (_cContract.HasContractedArea(rect, point.Z))
					{
						m.SendMessage("That area is already taken by another rental contract.");
					}
					else
					{
						_cContract.Blocks.Add(rect);
						_cContract.ShowAreaPreview(m);
					}

					_cGump.NewGump();
					break;
			}
		}

		protected override void OnTargetCancel(Mobile m, TargetCancelType cancelType)
		{
			_cGump.NewGump();
		}
	}
}
