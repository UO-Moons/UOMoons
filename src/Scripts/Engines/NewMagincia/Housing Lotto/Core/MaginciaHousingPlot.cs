using Server.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.NewMagincia;

[PropertyObject]
public class MaginciaHousingPlot
{
	private Rectangle2D _bounds;

	[CommandProperty(AccessLevel.GameMaster)]
	public string Identifier { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public WritOfLease Writ { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Rectangle2D Bounds => _bounds;

	[CommandProperty(AccessLevel.GameMaster)]
	public MaginciaPlotStone Stone { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsPrimeSpot { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Complete { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Winner { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Map Map { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime Expires { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D RecallLoc => new(_bounds.X, _bounds.Y, Map.GetAverageZ(_bounds.X, _bounds.Y));

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsAvailable => !Complete;

	#region Lotto Info

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime LottoEnds { get; set; }

	public Dictionary<Mobile, int> Participants { get; } = new();

	[CommandProperty(AccessLevel.GameMaster)]
	public int LottoPrice => IsPrimeSpot ? 10000 : 2000;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool LottoOngoing => IsAvailable && LottoEnds > DateTime.UtcNow && LottoEnds != DateTime.MinValue;
	#endregion

	public MaginciaHousingPlot(string identifier, Rectangle2D bounds, bool prime, Map map)
	{
		Identifier = identifier;
		_bounds = bounds;
		IsPrimeSpot = prime;
		Map = map;
		Writ = null;
		Complete = false;
		Expires = DateTime.MinValue;
	}

	public void AddPlotStone()
	{
		AddPlotStone(MaginciaLottoSystem.GetPlotStoneLoc(this));
	}

	public void AddPlotStone(Point3D p)
	{
		Stone = new MaginciaPlotStone
		{
			Plot = this
		};
		Stone.MoveToWorld(p, Map);
	}

	public override string ToString()
	{
		return "...";
	}

	public bool CanPurchaseLottoTicket(Mobile from)
	{
		if (IsPrimeSpot)
		{
			if (from.Account is not Account acct)
				return false;

			return CheckAccount(acct);
		}

		return true;
	}

	private static bool CheckAccount(Account acct)
	{
		return acct.Where(m => m != null).All(m => !MaginciaLottoSystem.Plots.Any(plot => plot.IsPrimeSpot && plot.Participants.ContainsKey(m)));
	}

	public void PurchaseLottoTicket(Mobile from, int toBuy)
	{
		if (Participants.ContainsKey(from))
			Participants[from] += toBuy;
		else
			Participants[from] = toBuy;
	}

	public void EndLotto()
	{
		if (Participants.Count == 0)
		{
			ResetLotto();
			return;
		}

		List<Mobile> raffle = new List<Mobile>();

		foreach (var kvp in Participants.Where(kvp => kvp.Value != 0))
		{
			for (int i = 0; i < kvp.Value; i++)
				raffle.Add(kvp.Key);
		}

		Mobile winner = raffle[Utility.Random(raffle.Count)];

		if (winner != null)
			OnLottoComplete(winner);
		else
			ResetLotto();

		Participants.Clear();
	}

	public void OnLottoComplete(Mobile winner)
	{
		Complete = true;
		Winner = winner;
		LottoEnds = DateTime.MinValue;
		Expires = DateTime.UtcNow + TimeSpan.FromDays(MaginciaLottoSystem.WritExpirePeriod);

		if (winner.HasGump(typeof(PlotWinnerGump)))
			return;

		Account acct = winner.Account as Account;

		if (acct == null)
			return;

		if (acct.Where(m => m != null).Any(m => m.NetState != null))
		{
			winner.SendGump(new PlotWinnerGump(this));
		}
	}

	public void SendMessage_Callback(object o)
	{
		if (o is object[] obj)
		{
			Mobile winner = obj[0] as Mobile;
			NewMaginciaMessage message = obj[1] as NewMaginciaMessage;

			MaginciaLottoSystem.SendMessageTo(winner, message);
		}
	}

	public void ResetLotto()
	{
		if (MaginciaLottoSystem.AutoResetLotto && MaginciaLottoSystem.Instance != null)
			LottoEnds = DateTime.UtcNow + MaginciaLottoSystem.Instance.LottoDuration;
		else
			LottoEnds = DateTime.MinValue;
	}

	public MaginciaHousingPlot(GenericReader reader)
	{
		reader.ReadInt();

		Identifier = reader.ReadString();
		Writ = reader.ReadItem() as WritOfLease;
		Stone = reader.ReadItem() as MaginciaPlotStone;
		LottoEnds = reader.ReadDateTime();
		_bounds = reader.ReadRect2D();
		Map = reader.ReadMap();
		IsPrimeSpot = reader.ReadBool();
		Complete = reader.ReadBool();
		Winner = reader.ReadMobile();
		Expires = reader.ReadDateTime();

		int c = reader.ReadInt();
		for (int i = 0; i < c; i++)
		{
			Mobile m = reader.ReadMobile();
			int amount = reader.ReadInt();

			if (m != null)
				Participants[m] = amount;
		}

		if ((Stone == null || Stone.Deleted) && LottoOngoing && MaginciaLottoSystem.IsRegisteredPlot(this))
			AddPlotStone();
		else if (Stone != null)
			Stone.Plot = this;

		if (Writ != null)
			Writ.Plot = this;
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(Identifier);
		writer.Write(Writ);
		writer.Write(Stone);
		writer.Write(LottoEnds);
		writer.Write(_bounds);
		writer.Write(Map);
		writer.Write(IsPrimeSpot);
		writer.Write(Complete);
		writer.Write(Winner);
		writer.Write(Expires);

		writer.Write(Participants.Count);

		foreach (KeyValuePair<Mobile, int> kvp in Participants)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value);
		}
	}
}
