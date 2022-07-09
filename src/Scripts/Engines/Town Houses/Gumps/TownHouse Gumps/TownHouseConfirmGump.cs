using System;

namespace Server.Engines.TownHouses;

public class TownHouseConfirmGump : GumpPlusLight
{
	private readonly TownHouseSign _cSign;
	private bool _cItems;

	public TownHouseConfirmGump(Mobile m, TownHouseSign sign) : base(m, 100, 100)
	{
		_cSign = sign;
	}

	protected override void BuildGump()
	{
		const int width = 200;
		var y = 0;

		AddHtml(0, y += 10, width, $"<CENTER>{(_cSign.RentByTime == TimeSpan.Zero ? "Purchase" : "Rent")} this House?");
		AddImage(width / 2 - 100, y + 2, 0x39);
		AddImage(width / 2 + 70, y + 2, 0x3B);

		if (_cSign.RentByTime == TimeSpan.Zero)
		{
			AddHtml(0, y += 25, width, $"<CENTER>Price: {(_cSign.Free ? "Free" : "" + _cSign.Price)}");
		}
		else if (_cSign.RecurRent)
		{
			AddHtml(0, y += 25, width, $"<CENTER>{"Recurring " + _cSign.PriceType}: {_cSign.Price}");
		}
		else
		{
			AddHtml(0, y += 25, width, $"<CENTER>{"One " + _cSign.PriceTypeShort}: {_cSign.Price}");
		}

		if (_cSign.KeepItems)
		{
			AddHtml(0, y += 20, width, "<CENTER>Cost of Items: " + _cSign.ItemsPrice);
			AddButton(20, y, _cItems ? 0xD3 : 0xD2, "Items", Items);
		}

		AddHtml(0, y += 20, width, "<CENTER>Lockdowns: " + _cSign.Locks);
		AddHtml(0, y += 20, width, "<CENTER>Secures: " + _cSign.Secures);

		AddButton(10, y += 25, 0xFB1, 0xFB3, "Cancel", Cancel);
		AddButton(width - 40, y, 0xFB7, 0xFB9, "Confirm", Confirm);

		AddBackgroundZero(0, 0, width, y + 40, 0x13BE);
	}

	private void Items()
	{
		_cItems = !_cItems;

		NewGump();
	}

	private void Cancel()
	{
	}

	private void Confirm()
	{
		_cSign.Purchase(Owner, _cItems);
	}
}
