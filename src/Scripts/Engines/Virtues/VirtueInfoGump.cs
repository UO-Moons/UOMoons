using Server.Gumps;
using Server.Network;

namespace Server;

public class VirtueInfoGump : Gump
{
	private readonly Mobile _beholder;
	private readonly int _desc;
	private readonly string _page;
	private readonly VirtueName _virtue;

	public VirtueInfoGump(Mobile beholder, VirtueName virtue, int description) : this(beholder, virtue, description, null)
	{
	}

	public VirtueInfoGump(Mobile beholder, VirtueName virtue, int description, string webPage) : base(0, 0)
	{
		_beholder = beholder;
		_virtue = virtue;
		_desc = description;
		_page = webPage;

		var value = beholder.Virtues.GetValue((int)virtue);

		AddPage(0);

		AddImage(30, 40, 2080);
		AddImage(47, 77, 2081);
		AddImage(47, 147, 2081);
		AddImage(47, 217, 2081);
		AddImage(47, 267, 2083);
		AddImage(70, 213, 2091);

		AddPage(1);

		var maxValue = VirtueHelper.GetMaxAmount(_virtue);

		int valueDesc;
		int dots;

		switch (value)
		{
			case < 4000:
				dots = value / 400;
				break;
			case < 10000:
				dots = (value - 4000) / 600;
				break;
			default:
			{
				if (value < maxValue)
					dots = (value - 10000) / ((maxValue - 10000) / 10);
				else
					dots = 10;
				break;
			}
		}

		for (var i = 0; i < 10; ++i)
			AddImage(95 + (i * 17), 50, i < dots ? 2362 : 2360);


		switch (value)
		{
			case < 1:
				valueDesc = 1052044; // You have not started on the path of this Virtue.
				break;
			case < 400:
				valueDesc = 1052045; // You have barely begun your journey through the path of this Virtue.
				break;
			case < 2000:
				valueDesc = 1052046; // You have progressed in this Virtue, but still have much to do.
				break;
			case < 3600:
				valueDesc = 1052047; // Your journey through the path of this Virtue is going well.
				break;
			case < 4000:
				valueDesc = 1052048; // You feel very close to achieving your next path in this Virtue.
				break;
			default:
			{
				valueDesc = dots switch
				{
					< 1 => 1052049,
					< 9 => 1052047,
					< 10 => 1052048,
					_ => 1052050
				};
				break;
			}
		}


		AddHtmlLocalized(157, 73, 200, 40, 1051000 + (int)virtue, false, false);
		AddHtmlLocalized(75, 95, 220, 140, description, false, false);
		AddHtmlLocalized(70, 224, 229, 60, valueDesc, false, false);

		AddButton(65, 277, 1209, 1209, 1, GumpButtonType.Reply, 0);

		AddButton(280, 43, 4014, 4014, 2, GumpButtonType.Reply, 0);

		AddHtmlLocalized(83, 275, 400, 40, (webPage == null) ? 1052055 : 1052052, false, false); // This virtue is not yet defined. OR -click to learn more (opens webpage)
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		switch (info.ButtonID)
		{
			case 1:
			{
				_beholder.SendGump(new VirtueInfoGump(_beholder, _virtue, _desc, _page));

				if (_page != null)
					state.Send(new LaunchBrowser(_page)); //No message about web browser starting on OSI
				break;
			}
			case 2:
			{
				_beholder.SendGump(new VirtueStatusGump(_beholder));
				break;
			}
		}
	}
}
