using System;
using Server.Network;
using Server.Mobiles;
using Server.Engines.Quests;

namespace Server.Gumps;

public class ConfirmHeritageQuestGump : Gump
{
	private readonly HeritageQuester _mQuester;
	public ConfirmHeritageQuestGump(HeritageQuester quester)
		: base(50, 50)
	{
		_mQuester = quester;

		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);

		AddBackground(0, 0, 240, 135, 0x2422);

		object message = _mQuester.ConfirmMessage;

		switch (message)
		{
			case int i:
				AddHtmlLocalized(15, 15, 210, 75, i, 0x0, false, false);
				break;
			case string s:
				AddHtml(15, 15, 210, 75, s, false, false);
				break;
		}

		AddButton(160, 95, 0xF7, 0xF8, (int)Buttons.Okay, GumpButtonType.Reply, 0);
		AddButton(90, 95, 0xF2, 0xF1, (int)Buttons.Close, GumpButtonType.Reply, 0);
	}

	private enum Buttons
	{
		Close,
		Okay,
	}
	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (_mQuester == null)
			return;

		if (info.ButtonID != (int) Buttons.Okay) return;
		Mobile m = state.Mobile;

		if (!HeritageQuester.Check(m)) return;
		HeritageQuester.AddPending(m, _mQuester);
		Timer.DelayCall(TimeSpan.FromMinutes(1), new TimerStateCallback(CloseHeritageGump), m);

		state.Mobile.Send(new HeritagePacket(m.Female, (short)(_mQuester.Race.RaceID + 1)));
	}

	private static void CloseHeritageGump(object args)
	{
		if (args is not Mobile m) return;
		if (!HeritageQuester.IsPending(m)) return;
		m.Send(HeritagePacket.Close);

		HeritageQuester.RemovePending(m);
	}
}
