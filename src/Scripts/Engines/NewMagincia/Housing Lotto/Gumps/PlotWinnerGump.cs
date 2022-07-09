using Server.Gumps;
using Server.Network;

namespace Server.Engines.NewMagincia;

public class PlotWinnerGump : Gump
{
	private readonly MaginciaHousingPlot _plot;

	private const int BlueColor = 0x1E90FF;
	private const int GreenColor = 0x7FFFD4;
	private const int EntryColor = 0xFF7F50;

	public PlotWinnerGump(MaginciaHousingPlot plot) : base(75, 75)
	{
		_plot = plot;

		AddBackground(0, 0, 424, 351, 9500);
		AddImage(5, 10, 5411);

		AddHtmlLocalized(170, 13, 150, 16, 1150484, GreenColor, false, false); // WRIT OF LEASE

		string args = $"{plot.Identifier}\t{plot.Map}\t{plot.Bounds.X} {plot.Bounds.Y}";
		AddHtmlLocalized(10, 40, 404, 180, 1150499, args, BlueColor, true, true);

		AddButton(5, 235, 4005, 4007, 1, GumpButtonType.Reply, 0);
		AddHtmlLocalized(50, 235, 150, 16, 1150498, EntryColor, false, false); // CLAIM DEED
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		Mobile winner = state.Mobile;

		if (info.ButtonID == 1)
		{
			WritOfLease writ = new(_plot);
			_plot.Writ = writ;
			_plot.Winner = null;

			if (winner.Backpack == null || !winner.Backpack.TryDropItem(winner, writ, false))
			{
				winner.SendLocalizedMessage(1150501); // Your backpack is full, so the deed has been placed in your bank box.
				winner.BankBox.DropItem(writ);
			}
			else
				winner.SendLocalizedMessage(1150500); // The deed has been placed in your backpack.

			MaginciaLottoSystem.GetWinnerGump(winner);
		}
	}
}
