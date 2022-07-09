using Server.ContextMenus;
using System;
using System.Collections.Generic;

namespace Server.Engines.NewMagincia;

public class PlotSign : BaseItem
{
	public static readonly int RuneCost = 100;

	private MaginciaBazaarPlot _plot;

	[CommandProperty(AccessLevel.GameMaster)]
	public MaginciaBazaarPlot Plot
	{
		get => _plot;
		set { _plot = value; InvalidateProperties(); }
	}

	public override bool DisplayWeight => false;

	public PlotSign(MaginciaBazaarPlot plot)
		: base(3025)
	{
		Movable = false;
		_plot = plot;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (_plot is not {Active: true})
		{
			from.SendMessage("New Magincia Bazaar Plot {0} is inactive at this time.", _plot.PlotDef.Id);
		}
		else if (from.InRange(Location, 3))
		{
			from.CloseGump(typeof(BaseBazaarGump));
			from.SendGump(new StallLeasingGump(from, _plot));
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (_plot == null)
			return;

		if (_plot.ShopName != null)
			list.Add(1062449, _plot.ShopName); // Shop Name: ~1_NAME~

		if (_plot.Merchant != null)
			list.Add(1150529, _plot.Merchant.Name); // Proprietor: ~1_NAME~

		if (_plot.Auction != null)
		{
			int left = 1;
			if (_plot.Auction.AuctionEnd > DateTime.UtcNow)
			{
				TimeSpan ts = _plot.Auction.AuctionEnd - DateTime.UtcNow;
				left = (int)(ts.TotalHours + 1);
			}

			list.Add(1150533, left.ToString()); // Auction for Lease Ends Within ~1_HOURS~ Hours
		}

		if (!_plot.Active)
			list.Add(1153036); // Inactive
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (_plot == null)
			list.Add(1150530, "unknown"); // Stall ~1_NAME~
		else
			list.Add(1150530, _plot.PlotDef != null ? _plot.PlotDef.Id : "unknown"); // Stall ~1_NAME~
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (_plot is {Active: true})
			list.Add(new RecallRuneEntry(from, this));
	}

	private class RecallRuneEntry : ContextMenuEntry
	{
		private readonly PlotSign _sign;
		private readonly Mobile _from;

		public RecallRuneEntry(Mobile from, PlotSign sign)
			: base(1151508, -1)
		{
			_sign = sign;
			_from = from;

			Enabled = from.InRange(sign.Location, 2);
		}

		public override void OnClick()
		{
			_from.SendGump(new ShopRecallRuneGump(_from, _sign));
		}
	}

	public PlotSign(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
