using Server.Accounting;
using System;

namespace Server.Engines.NewMagincia;

[PropertyObject]
public class MaginciaBazaarPlot
{
	private BaseBazaarMulti _plotMulti;
	private BaseBazaarBroker _merchant;

	[CommandProperty(AccessLevel.GameMaster)]
	public PlotDef PlotDef { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public string ShopName { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public BaseBazaarMulti PlotMulti
	{
		get => _plotMulti;
		set
		{
			if (_plotMulti != null && _plotMulti != value && value != null)
			{
				_plotMulti.Delete();
				_plotMulti = null;
			}

			_plotMulti = value;

			_plotMulti?.MoveToWorld(PlotDef.MultiLocation, PlotDef.Map);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public BaseBazaarBroker Merchant
	{
		get => _merchant;
		set
		{
			_merchant = value;

			if (_merchant != null)
			{
				_merchant.Plot = this;

				Point3D p = PlotDef.Location;
				p.X++;
				p.Y++;
				p.Z = 27;

				if (_plotMulti != null && _plotMulti.Fillers.Count > 0)
				{
					p = _plotMulti.Fillers[0].Location;
					p.Z = _plotMulti.Fillers[0].Z + TileData.ItemTable[_plotMulti.Fillers[0].ItemId & TileData.MaxItemValue].CalcHeight;
				}

				_merchant.MoveToWorld(p, PlotDef.Map);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public PlotSign Sign { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public MaginciaPlotAuction Auction { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Active => MaginciaBazaar.IsActivePlot(this);

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime AuctionEnds => Auction?.AuctionEnd ?? DateTime.MinValue;

	public MaginciaBazaarPlot(PlotDef definition)
	{
		PlotDef = definition;
		Owner = null;
		_plotMulti = null;
		_merchant = null;
		ShopName = null;
	}

	public bool IsOwner(Mobile from)
	{
		if (from == null || Owner == null)
			return false;

		if (from == Owner)
			return true;

		return from.Account is Account acct1 && Owner.Account is Account acct2 && acct1 == acct2;
	}

	public void AddPlotSign()
	{
		Sign = new PlotSign(this);
		Sign.MoveToWorld(PlotDef.SignLoc, PlotDef.Map);
	}

	public void Reset()
	{
		if (_plotMulti != null)
			Timer.DelayCall(TimeSpan.FromMinutes(2), DeleteMulti_Callback);

		EndTempMultiTimer();

		_merchant?.Dismiss();

		Owner = null;
		ShopName = null;
		_merchant = null;
		ShopName = null;
	}

	public void NewAuction(TimeSpan time)
	{
		Auction = new MaginciaPlotAuction(this, time);

		Sign?.InvalidateProperties();
	}

	private void DeleteMulti_Callback()
	{
		_plotMulti?.Delete();

		_plotMulti = null;
	}

	public void OnTick()
	{
		Auction?.OnTick();

		_merchant?.OnTick();

		Sign?.InvalidateProperties();
	}

	#region Stall Style Multis
	private Timer _timer;

	public void AddTempMulti(int idx1, int idx2)
	{
		if (_plotMulti != null)
		{
			_plotMulti.Delete();
			_plotMulti = null;
		}

		BaseBazaarMulti multi;

		if (idx1 == 0)
		{
			multi = idx2 switch
			{
				0 => new CommodityStyle1(),
				1 => new CommodityStyle2(),
				2 => new CommodityStyle3(),
				_ => null
			};
		}
		else
		{
			multi = idx2 switch
			{
				0 => new PetStyle1(),
				1 => new PetStyle2(),
				2 => new PetStyle3(),
				_ => null
			};
		}

		if (multi != null)
		{
			PlotMulti = multi;
			BeginTempMultiTimer();
		}
	}

	public void ConfirmMulti(bool commodity)
	{
		EndTempMultiTimer();

		if (commodity)
			Merchant = new CommodityBroker(this);
		else
			Merchant = new PetBroker(this);
	}

	public void RemoveTempPlot()
	{
		EndTempMultiTimer();

		if (_plotMulti != null)
		{
			_plotMulti.Delete();
			_plotMulti = null;
		}
	}

	public void BeginTempMultiTimer()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}

		_timer = new InternalTimer(this);
		_timer.Start();
	}

	public void EndTempMultiTimer()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}
	}

	public bool HasTempMulti()
	{
		return _timer != null;
	}

	private class InternalTimer : Timer
	{
		private readonly MaginciaBazaarPlot _plot;

		public InternalTimer(MaginciaBazaarPlot plot) : base(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
		{
			_plot = plot;
		}

		protected override void OnTick()
		{
			_plot?.RemoveTempPlot();
		}
	}

	#endregion

	public override string ToString()
	{
		return "...";
	}

	public bool TrySetShopName(Mobile from, string text)
	{
		if (text == null || !Guilds.BaseGuildGump.CheckProfanity(text) || text.Length == 0 || text.Length > 40)
			return false;

		ShopName = text;

		_merchant?.InvalidateProperties();

		Sign?.InvalidateProperties();

		from.SendLocalizedMessage(1150333); // Your shop has been renamed.

		return true;
	}

	public void FireBroker()
	{
		if (_merchant != null)
		{
			_merchant.Delete();
			_merchant = null;

			if (_plotMulti != null)
			{
				_plotMulti.Delete();
				_plotMulti = null;
			}
		}
	}

	public void Abandon()
	{
		Reset();

		Auction?.ChangeAuctionTime(MaginciaBazaar.GetShortAuctionTime);
	}

	public int GetBid(Mobile from)
	{
		if (Auction != null && Auction.Auctioners.ContainsKey(from))
			return Auction.Auctioners[from].Amount;
		return 0;
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		PlotDef.Serialize(writer);

		writer.Write(Owner);
		writer.Write(ShopName);
		writer.Write(_merchant);
		writer.Write(Sign);
		writer.Write(_plotMulti);

		if (Auction != null)
		{
			writer.Write(true);
			Auction.Serialize(writer);
		}
		else
			writer.Write(false);
	}

	public MaginciaBazaarPlot(GenericReader reader)
	{
		reader.ReadInt();

		PlotDef = new PlotDef(reader);

		Owner = reader.ReadMobile();
		ShopName = reader.ReadString();
		_merchant = reader.ReadMobile() as BaseBazaarBroker;
		Sign = reader.ReadItem() as PlotSign;
		_plotMulti = reader.ReadItem() as BaseBazaarMulti;

		if (reader.ReadBool())
			Auction = new MaginciaPlotAuction(reader, this);

		if (_merchant != null)
			_merchant.Plot = this;

		if (Sign != null)
			Sign.Plot = this;
	}
}

[PropertyObject]
public class PlotDef
{
	private Point3D _location;

	[CommandProperty(AccessLevel.GameMaster)]
	public string Id { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Location { get => _location;
		set => _location = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Map Map { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D SignLoc => new(_location.X + 1, _location.Y - 2, _location.Z);

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D MultiLocation => new(_location.X, _location.Y, _location.Z + 2);

	public PlotDef(string id, Point3D pnt, int mapId)
	{
		Id = id;
		_location = pnt;
		Map = Map.Maps[mapId];
	}

	public override string ToString()
	{
		return "...";
	}

	public PlotDef(GenericReader reader)
	{
		reader.ReadInt();

		Id = reader.ReadString();
		_location = reader.ReadPoint3D();
		Map = reader.ReadMap();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(Id);
		writer.Write(_location);
		writer.Write(Map);
	}
}
