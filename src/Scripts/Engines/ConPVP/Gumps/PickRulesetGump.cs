using Server.Gumps;
using Server.Network;

namespace Server.Engines.ConPVP;

public class PickRulesetGump : Gump
{
	private readonly Mobile _from;
	private readonly DuelContext _context;
	private readonly Ruleset _ruleset;
	private readonly Ruleset[] _defaults;
	private readonly Ruleset[] _flavors;

	public PickRulesetGump(Mobile from, DuelContext context, Ruleset ruleset) : base(50, 50)
	{
		_from = from;
		_context = context;
		_ruleset = ruleset;
		_defaults = ruleset.Layout.Defaults;
		_flavors = ruleset.Layout.Flavors;

		int height = 25 + 20 + (_defaults.Length + 1) * 22 + 6 + 20 + _flavors.Length * 22 + 25;

		AddPage(0);

		AddBackground(0, 0, 260, height, 9250);
		AddBackground(10, 10, 240, height - 20, 0xDAC);

		AddHtml(35, 25, 190, 20, Center("Rules"), false, false);

		int y = 25 + 20;

		for (int i = 0; i < _defaults.Length; ++i)
		{
			Ruleset cur = _defaults[i];

			AddHtml(35 + 14, y, 176, 20, cur.Title, false, false);

			if (ruleset.Base == cur && !ruleset.Changed)
				AddImage(35, y + 4, 0x939);
			else if (ruleset.Base == cur)
				AddButton(35, y + 4, 0x93A, 0x939, 2 + i, GumpButtonType.Reply, 0);
			else
				AddButton(35, y + 4, 0x938, 0x939, 2 + i, GumpButtonType.Reply, 0);

			y += 22;
		}

		AddHtml(35 + 14, y, 176, 20, "Custom", false, false);
		AddButton(35, y + 4, ruleset.Changed ? 0x939 : 0x938, 0x939, 1, GumpButtonType.Reply, 0);

		y += 22;
		y += 6;

		AddHtml(35, y, 190, 20, Center("Flavors"), false, false);
		y += 20;

		for (int i = 0; i < _flavors.Length; ++i)
		{
			Ruleset cur = _flavors[i];

			AddHtml(35 + 14, y, 176, 20, cur.Title, false, false);

			if (ruleset.Flavors.Contains(cur))
				AddButton(35, y + 4, 0x939, 0x938, 2 + _defaults.Length + i, GumpButtonType.Reply, 0);
			else
				AddButton(35, y + 4, 0x938, 0x939, 2 + _defaults.Length + i, GumpButtonType.Reply, 0);

			y += 22;
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_context != null && !_context.Registered)
			return;

		switch (info.ButtonID)
		{
			case 0: // closed
			{
				if (_context != null)
					_from.SendGump(new DuelContextGump(_from, _context));

				break;
			}
			case 1: // customize
			{
				_from.SendGump(new RulesetGump(_from, _ruleset, _ruleset.Layout, _context));
				break;
			}
			default:
			{
				int idx = info.ButtonID - 2;

				if (idx >= 0 && idx < _defaults.Length)
				{
					_ruleset.ApplyDefault(_defaults[idx]);
					_from.SendGump(new PickRulesetGump(_from, _context, _ruleset));
				}
				else
				{
					idx -= _defaults.Length;

					if (idx >= 0 && idx < _flavors.Length)
					{
						if (_ruleset.Flavors.Contains(_flavors[idx]))
							_ruleset.RemoveFlavor(_flavors[idx]);
						else
							_ruleset.AddFlavor(_flavors[idx]);

						_from.SendGump(new PickRulesetGump(_from, _context, _ruleset));
					}
				}

				break;
			}
		}
	}
}
