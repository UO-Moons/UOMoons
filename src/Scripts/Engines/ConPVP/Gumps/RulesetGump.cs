using Server.Gumps;
using Server.Network;
using System.Collections;

namespace Server.Engines.ConPVP;

public class RulesetGump : Gump
{
	private readonly Mobile _from;
	private readonly Ruleset _ruleset;
	private readonly RulesetLayout _page;
	private readonly DuelContext _duelContext;
	private readonly bool _readOnly;

	public void AddGoldenButton(int x, int y, int bid)
	{
		AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
		AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
	}

	public RulesetGump(Mobile from, Ruleset ruleset, RulesetLayout page, DuelContext duelContext) : this(from, ruleset, page, duelContext, false)
	{
	}

	public RulesetGump(Mobile from, Ruleset ruleset, RulesetLayout page, DuelContext duelContext, bool readOnly) : base(readOnly ? 310 : 50, 50)
	{
		_from = from;
		_ruleset = ruleset;
		_page = page;
		_duelContext = duelContext;
		_readOnly = readOnly;

		Dragable = !readOnly;

		from.CloseGump(typeof(RulesetGump));
		from.CloseGump(typeof(DuelContextGump));
		from.CloseGump(typeof(ParticipantGump));

		RulesetLayout depthCounter = page;

		while (depthCounter != null)
		{
			depthCounter = depthCounter.Parent;
		}

		int count = page.Children.Length + page.Options.Length;

		AddPage(0);

		int height = 35 + 10 + 2 + count * 22 + 2 + 30;

		AddBackground(0, 0, 260, height, 9250);
		AddBackground(10, 10, 240, height - 20, 0xDAC);

		AddHtml(35, 25, 190, 20, Center(page.Title), false, false);

		int x = 35;
		int y = 47;

		for (int i = 0; i < page.Children.Length; ++i)
		{
			AddGoldenButton(x, y, 1 + i);
			AddHtml(x + 25, y, 250, 22, page.Children[i].Title, false, false);

			y += 22;
		}

		for (int i = 0; i < page.Options.Length; ++i)
		{
			bool enabled = ruleset.Options[page.Offset + i];

			if (readOnly)
				AddImage(x, y, enabled ? 0xD3 : 0xD2);
			else
				AddCheck(x, y, 0xD2, 0xD3, enabled, i);

			AddHtml(x + 25, y, 250, 22, page.Options[i], false, false);

			y += 22;
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_duelContext is {Registered: false})
			return;

		if (!_readOnly)
		{
			BitArray opts = new(_page.Options.Length);

			for (int i = 0; i < info.Switches.Length; ++i)
			{
				int sid = info.Switches[i];

				if (sid >= 0 && sid < _page.Options.Length)
					opts[sid] = true;
			}

			for (int i = 0; i < opts.Length; ++i)
			{
				if (_ruleset.Options[_page.Offset + i] != opts[i])
				{
					_ruleset.Options[_page.Offset + i] = opts[i];
					_ruleset.Changed = true;
				}
			}
		}

		int bid = info.ButtonID;

		if (bid == 0)
		{
			if (_page.Parent != null)
				_from.SendGump(new RulesetGump(_from, _ruleset, _page.Parent, _duelContext, _readOnly));
			else if (!_readOnly)
				_from.SendGump(new PickRulesetGump(_from, _duelContext, _ruleset));
		}
		else
		{
			bid -= 1;

			if (bid >= 0 && bid < _page.Children.Length)
				_from.SendGump(new RulesetGump(_from, _ruleset, _page.Children[bid], _duelContext, _readOnly));
		}
	}
}
