using Server.Mobiles;
using Server.Network;

namespace Server.Gumps;

public class SelectTitleGump : Gump
{
	private readonly PlayerMobile _mFrom;
	private readonly int _mPage;
	public SelectTitleGump(PlayerMobile from, int page)
		: base(50, 50)
	{
		_mFrom = from;
		_mPage = page;

		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);
		AddBackground(0, 0, 270, 120, 0x13BE);
		AddBackground(10, 10, 250, 100, 0xBB8);

		AddHtmlLocalized(20, 15, 230, 20, 1073994, 0x1, false, false); // Your title will be:

		if (page > -1 && page < from.RewardTitles.Count)
		{
			if (from.RewardTitles[page] is int @int)
				AddHtmlLocalized(20, 35, 230, 40, @int, 0x32, true, false);
			else if (from.RewardTitles[page] is string @string)
				AddHtml(20, 35, 230, 40, $"<BASEFONT COLOR=#{0x32:X6}>{@string}</BASEFONT>", true, false);
		}
		else
			AddHtmlLocalized(20, 35, 230, 40, 1073995, 0x32, true, false);

		AddHtmlLocalized(55, 80, 75, 20, 1073996, 0x0, false, false); // ACCEPT
		AddHtmlLocalized(170, 80, 75, 20, 1073997, 0x0, false, false); // NEXT

		AddButton(20, 80, 0xFA5, 0xFA7, (int)Buttons.Accept, GumpButtonType.Reply, 0);
		AddButton(135, 80, 0xFA5, 0xFA7, (int)Buttons.Next, GumpButtonType.Reply, 0);
	}

	private enum Buttons
	{
		Next,
		Accept,
	}
	public override void OnResponse(NetState state, RelayInfo info)
	{
		switch (info.ButtonID)
		{
			case (int)Buttons.Accept:
				_mFrom.SelectRewardTitle(_mPage);
				break;
			case (int)Buttons.Next:
				if (_mPage == _mFrom.RewardTitles.Count - 1)
					_mFrom.SendGump(new SelectTitleGump(_mFrom, -1));
				else if (_mPage < _mFrom.RewardTitles.Count - 1 && _mPage > -1)
					_mFrom.SendGump(new SelectTitleGump(_mFrom, _mPage + 1));
				else
					_mFrom.SendGump(new SelectTitleGump(_mFrom, 0));

				break;
		}
	}
}
