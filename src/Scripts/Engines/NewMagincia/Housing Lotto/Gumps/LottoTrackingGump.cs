using Server.Gumps;
using Server.Network;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.NewMagincia
{
    public class LottoTrackingGump : Gump
    {
	    private const int LabelColor = 0xFFFFFF;
	    private readonly List<MaginciaHousingPlot> _list;

        public LottoTrackingGump() : base(50, 50)
        {
            AddBackground(0, 0, 410, 564, 9500);

            AddHtml(205, 10, 205, 20, "<DIV ALIGN=RIGHT><Basefont Color=#FFFFFF>New Magincia Lotto Tracking</DIV>", false, false);
            AddHtml(10, 10, 205, 20, Color($"Gold Sink: {MaginciaLottoSystem.GoldSink:###,###,###}", 0xFFFFFF), false, false);

            AddHtml(45, 40, 40, 20, Color("ID", LabelColor), false, false);
            AddHtml(85, 40, 60, 20, Color("Facet", LabelColor), false, false);
            AddHtml(145, 40, 40, 20, Color("#bids", LabelColor), false, false);

            _list = new List<MaginciaHousingPlot>(MaginciaLottoSystem.Plots);

            int y = 60;
            int x = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                MaginciaHousingPlot plot = _list[i];

                if (plot == null)
                    continue;

                int bids = plot.Participants.Values.Sum();

                AddButton(10 + x, y, 4005, 4007, i + 5, GumpButtonType.Reply, 0);
                AddHtml(45 + x, y, 40, 22, Color(plot.Identifier, LabelColor), false, false);
                AddHtml(85 + x, y, 60, 22, Color(plot.Map.ToString(), LabelColor), false, false);

                if (plot.LottoOngoing)
                    AddHtml(145 + x, y, 40, 22, Color(bids.ToString(), LabelColor), false, false);
                else if (plot.Complete)
                    AddHtml(145 + x, y, 40, 22, Color("Owned", "red"), false, false);
                else
                    AddHtml(145 + x, y, 40, 22, Color("Expired", "red"), false, false);

                if (i == 21)
                {
                    y = 60;
                    x = 200;

                    AddHtml(45 + x, 40, 40, 20, Color("ID", LabelColor), false, false);
                    AddHtml(85 + x, 40, 60, 20, Color("Facet", LabelColor), false, false);
                    AddHtml(145 + x, 40, 40, 20, Color("#bids", LabelColor), false, false);
                }
                else
                    y += 22;
            }
        }

        private new string Color(string str, int color)
        {
            return $"<BASEFONT COLOR=#{color:X6}>{str}</BASEFONT>";
        }

        private string Color(string str, string color)
        {
            return $"<BASEFONT COLOR={color}>{str}</BASEFONT>";
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            if (info.ButtonID >= 5 && from.AccessLevel > AccessLevel.Player)
            {
                int index = info.ButtonID - 5;

                if (index >= 0 && index < _list.Count)
                {
                    MaginciaHousingPlot plot = _list[index];

                    if (plot != null)
                    {
                        from.SendGump(new PlotTrackingGump(plot));
                    }
                }
            }
        }
    }

    public class PlotTrackingGump : Gump
    {
        public PlotTrackingGump(MaginciaHousingPlot plot) : base(50, 50)
        {
            //int partCount = plot.Participants.Count;
            int y = 544;
            int x = 600;

            AddBackground(0, 0, x, y, 9500);

            AddHtml(10, 10, 580, 20, $"<Center><Basefont Color=#FFFFFF>Plot {plot.Identifier}</Center>", false, false);

            AddHtml(10, 40, 80, 20, Color("Player", 0xFFFFFF), false, false);
            AddHtml(92, 40, 60, 20, Color("Tickets", 0xFFFFFF), false, false);
            AddHtml(154, 40, 60, 20, Color("Total Gold", 0xFFFFFF), false, false);

            x = 0;
            y = 60;
            int goldSink = 0;

            List<Mobile> mobiles = new(plot.Participants.Keys);
            List<int> amounts = new(plot.Participants.Values);

            for (int i = 0; i < mobiles.Count; i++)
            {
                Mobile m = mobiles[i];
                int amt = amounts[i];
                int total = amt * plot.LottoPrice;
                goldSink += total;

                AddHtml(10 + x, y, 80, 22, Color(m.Name, 0xFFFFFF), false, false);
                AddHtml(92 + x, y, 60, 22, Color(amt.ToString(), 0xFFFFFF), false, false);
                AddHtml(154 + x, y, 60, 22, Color(total.ToString(), 0xFFFFFF), false, false);

                if (i == 21)
                {
                    x = 200;
                    y = 60;

                    AddHtml(10 + x, 40, 80, 20, Color("Player", 0xFFFFFF), false, false);
                    AddHtml(92 + x, 40, 60, 20, Color("Tickets", 0xFFFFFF), false, false);
                    AddHtml(154 + x, 40, 60, 20, Color("Total Gold", 0xFFFFFF), false, false);
                }
                else if (i == 43)
                {
                    x = 400;
                    y = 60;

                    AddHtml(10 + x, 40, 80, 20, Color("Player", 0xFFFFFF), false, false);
                    AddHtml(92 + x, 40, 60, 20, Color("Tickets", 0xFFFFFF), false, false);
                    AddHtml(154 + x, 40, 60, 20, Color("Total Gold", 0xFFFFFF), false, false);
                }
                else
                    y += 22;
            }

            AddHtml(10, 10, 150, 20, Color($"Gold Sink: {goldSink.ToString()}", 0xFFFFFF), false, false);

            AddButton(10, 544 - 32, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddHtml(45, 544 - 32, 150, 20, Color("Back", 0xFFFFFF), false, false);
        }

        private new string Color(string str, int color)
        {
            return $"<BASEFONT COLOR=#{color:X6}>{str}</BASEFONT>";
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            if (info.ButtonID == 1)
                from.SendGump(new LottoTrackingGump());
        }
    }
}
