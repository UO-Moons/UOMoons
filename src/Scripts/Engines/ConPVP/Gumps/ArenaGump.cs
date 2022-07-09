using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System.Collections.Generic;
using System.Text;

namespace Server.Engines.ConPVP;

public class ArenasMoongate : BaseItem
{
	public override string DefaultName => "arena moongate";

	[Constructable]
	public ArenasMoongate() : base(0x1FD4)
	{
		Movable = false;
		Light = LightType.Circle300;
	}

	public ArenasMoongate(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
		Light = LightType.Circle300;
	}

	public bool UseGate(Mobile from)
	{
		if (DuelContext.CheckCombat(from))
		{
			from.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
			return false;
		}
		else if (from.Spell != null)
		{
			from.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
			return false;
		}
		else
		{
			from.CloseGump(typeof(ArenaGump));
			from.SendGump(new ArenaGump(from, this));

			if (!from.Hidden || from.AccessLevel == AccessLevel.Player)
				Effects.PlaySound(from.Location, from.Map, 0x20E);

			return true;
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.InRange(GetWorldLocation(), 1))
			UseGate(from);
		else
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
	}

	public override bool OnMoveOver(Mobile m)
	{
		return !m.Player || UseGate(m);
	}
}

public class ArenaGump : Gump
{
	private readonly Mobile _from;
	private readonly ArenasMoongate _gate;
	private readonly List<Arena> _arenas;

	private static void Append(StringBuilder sb, LadderEntry le)
	{
		if (le == null)
			return;

		if (sb.Length > 0)
			sb.Append(", ");

		sb.Append(le.Mobile.Name);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (info.ButtonID != 1)
			return;

		int[] switches = info.Switches;

		if (switches.Length == 0)
			return;

		int opt = switches[0];

		if (opt < 0 || opt >= _arenas.Count)
			return;

		Arena arena = _arenas[opt];

		if (!_from.InRange(_gate.GetWorldLocation(), 1) || _from.Map != _gate.Map)
		{
			_from.SendLocalizedMessage(1019002); // You are too far away to use the gate.
		}
		else if (DuelContext.CheckCombat(_from))
		{
			_from.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
		}
		else if (_from.Spell != null)
		{
			_from.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
		}
		else if (_from.Map == arena.Facet && arena.Zone.Contains(_from))
		{
			_from.SendLocalizedMessage(1019003); // You are already there.
		}
		else
		{
			BaseCreature.TeleportPets(_from, arena.GateIn, arena.Facet);

			_from.Combatant = null;
			_from.Warmode = false;
			_from.Hidden = true;

			_from.MoveToWorld(arena.GateIn, arena.Facet);

			Effects.PlaySound(arena.GateIn, arena.Facet, 0x1FE);
		}
	}

	public ArenaGump(Mobile from, ArenasMoongate gate) : base(50, 50)
	{
		_from = from;
		_gate = gate;
		_arenas = Arena.Arenas;

		AddPage(0);

		int height = 12 + 20 + (_arenas.Count * 31) + 24 + 12;

		AddBackground(0, 0, 499 + 40, height, 0x2436);

		List<Arena> list = _arenas;

		for (int i = 1; i < list.Count; i += 2)
			AddImageTiled(12, 32 + (i * 31), 475 + 40, 30, 0x2430);

		AddAlphaRegion(10, 10, 479 + 40, height - 20);

		AddColumnHeader(35, null);
		AddColumnHeader(115, "Arena");
		AddColumnHeader(325, "Participants");
		AddColumnHeader(40, "Obs");

		AddButton(499 + 40 - 12 - 63 - 4 - 63, height - 12 - 24, 247, 248, 1, GumpButtonType.Reply, 0);
		AddButton(499 + 40 - 12 - 63, height - 12 - 24, 241, 242, 2, GumpButtonType.Reply, 0);

		for (int i = 0; i < list.Count; ++i)
		{
			Arena ar = list[i];

			string name = ar.Name;

			if (name == null)
				name = "(no name)";

			int x = 12;
			int y = 32 + (i * 31);

			int color = (ar.Players.Count > 0 ? 0xCCFFCC : 0xCCCCCC);

			AddRadio(x + 3, y + 1, 9727, 9730, false, i);
			x += 35;

			AddBorderedText(x + 5, y + 5, 115 - 5, name, color);
			x += 115;

			StringBuilder sb = new();

			if (ar.Players.Count > 0)
			{
				Ladder ladder = Ladder.Instance;

				if (ladder == null)
					continue;

				LadderEntry p1 = null, p2 = null, p3 = null, p4 = null;

				for (int j = 0; j < ar.Players.Count; ++j)
				{
					Mobile mob = ar.Players[j];
					LadderEntry c = ladder.Find(mob);

					if (p1 == null || c.Index < p1.Index)
					{
						p4 = p3;
						p3 = p2;
						p2 = p1;
						p1 = c;
					}
					else if (p2 == null || c.Index < p2.Index)
					{
						p4 = p3;
						p3 = p2;
						p2 = c;
					}
					else if (p3 == null || c.Index < p3.Index)
					{
						p4 = p3;
						p3 = c;
					}
					else if (p4 == null || c.Index < p4.Index)
					{
						p4 = c;
					}
				}

				Append(sb, p1);
				Append(sb, p2);
				Append(sb, p3);
				Append(sb, p4);

				if (ar.Players.Count > 4)
					sb.Append(", ...");
			}
			else
			{
				sb.Append("Empty");
			}

			AddBorderedText(x + 5, y + 5, 325 - 5, sb.ToString(), color);
			x += 325;

			AddBorderedText(x, y + 5, 40, Center(ar.Spectators.ToString()), color);
		}
	}

	private void AddBorderedText(int x, int y, int width, string text, int color)
	{
		/*AddColoredText( x - 1, y, width, text, borderColor );
		AddColoredText( x + 1, y, width, text, borderColor );
		AddColoredText( x, y - 1, width, text, borderColor );
		AddColoredText( x, y + 1, width, text, borderColor );*/
		/*AddColoredText( x - 1, y - 1, width, text, borderColor );
		AddColoredText( x + 1, y + 1, width, text, borderColor );*/
		AddColoredText(x, y, width, text, color);
	}

	private void AddColoredText(int x, int y, int width, string text, int color)
	{
		AddHtml(x, y, width, 20, color == 0 ? text : Color(text, color), false, false);
	}

	private int _columnX = 12;

	private void AddColumnHeader(int width, string name)
	{
		AddBackground(_columnX, 12, width, 20, 0x242C);
		AddImageTiled(_columnX + 2, 14, width - 4, 16, 0x2430);

		if (name != null)
			AddBorderedText(_columnX, 13, width, Center(name), 0xFFFFFF);

		_columnX += width;
	}
}
