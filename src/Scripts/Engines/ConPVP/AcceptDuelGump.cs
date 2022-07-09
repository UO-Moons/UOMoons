using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections;

namespace Server.Engines.ConPVP;

public class AcceptDuelGump : Gump
{
	private readonly Mobile _challenger, _challenged;
	private readonly DuelContext _context;
	private readonly Participant _participant;
	private readonly int _slot;

	private const int LabelColor32 = 0xFFFFFF;
	private const int BlackColor32 = 0x000008;

	private bool _active = true;

	public AcceptDuelGump(Mobile challenger, Mobile challenged, DuelContext context, Participant p, int slot) : base(50, 50)
	{
		_challenger = challenger;
		_challenged = challenged;
		_context = context;
		_participant = p;
		_slot = slot;

		challenged.CloseGump(typeof(AcceptDuelGump));

		Closable = false;

		AddPage(0);

		//AddBackground( 0, 0, 400, 220, 9150 );
		AddBackground(1, 1, 398, 218, 3600);
		//AddBackground( 16, 15, 369, 189, 9100 );

		AddImageTiled(16, 15, 369, 189, 3604);
		AddAlphaRegion(16, 15, 369, 189);

		AddImage(215, -43, 0xEE40);
		//AddImage( 330, 141, 0x8BA );

		AddHtml(22 - 1, 22, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
		AddHtml(22 + 1, 22, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
		AddHtml(22, 22 - 1, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
		AddHtml(22, 22 + 1, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
		AddHtml(22, 22, 294, 20, Color(Center("Duel Challenge"), LabelColor32), false, false);

		string fmt;

		fmt = p.Contains(challenger) ? "You have been asked to join sides with {0} in a duel. Do you accept?" : "You have been challenged to a duel from {0}. Do you accept?";

		AddHtml(22 - 1, 50, 294, 40, Color(string.Format(fmt, challenger.Name), BlackColor32), false, false);
		AddHtml(22 + 1, 50, 294, 40, Color(string.Format(fmt, challenger.Name), BlackColor32), false, false);
		AddHtml(22, 50 - 1, 294, 40, Color(string.Format(fmt, challenger.Name), BlackColor32), false, false);
		AddHtml(22, 50 + 1, 294, 40, Color(string.Format(fmt, challenger.Name), BlackColor32), false, false);
		AddHtml(22, 50, 294, 40, Color(string.Format(fmt, challenger.Name), 0xB0C868), false, false);

		AddImageTiled(32, 88, 264, 1, 9107);
		AddImageTiled(42, 90, 264, 1, 9157);

		AddRadio(24, 100, 9727, 9730, true, 1);
		AddHtml(60 - 1, 105, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
		AddHtml(60 + 1, 105, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
		AddHtml(60, 105 - 1, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
		AddHtml(60, 105 + 1, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
		AddHtml(60, 105, 250, 20, Color("Yes, I will fight this duel.", LabelColor32), false, false);

		AddRadio(24, 135, 9727, 9730, false, 2);
		AddHtml(60 - 1, 140, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
		AddHtml(60 + 1, 140, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
		AddHtml(60, 140 - 1, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
		AddHtml(60, 140 + 1, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
		AddHtml(60, 140, 250, 20, Color("No, I do not wish to fight.", LabelColor32), false, false);

		AddRadio(24, 170, 9727, 9730, false, 3);
		AddHtml(60 - 1, 175, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
		AddHtml(60 + 1, 175, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
		AddHtml(60, 175 - 1, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
		AddHtml(60, 175 + 1, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
		AddHtml(60, 175, 250, 20, Color("No, knave. Do not ask again.", LabelColor32), false, false);

		AddButton(314, 173, 247, 248, 1, GumpButtonType.Reply, 0);

		Timer.DelayCall(TimeSpan.FromSeconds(15.0), AutoReject);
	}

	public void AutoReject()
	{
		if (!_active)
			return;

		_active = false;

		_challenged.CloseGump(typeof(AcceptDuelGump));

		_challenger.SendMessage("{0} seems unresponsive.", _challenged.Name);
		_challenged.SendMessage("You decline the challenge.");
	}

	private static readonly Hashtable m_IgnoreLists = new();

	private class IgnoreEntry
	{
		public readonly Mobile m_Ignored;
		public DateTime Expire;

		public Mobile Ignored => m_Ignored;
		public bool Expired => DateTime.UtcNow >= Expire;

		private static readonly TimeSpan ExpireDelay = TimeSpan.FromMinutes(15.0);

		public void Refresh()
		{
			Expire = DateTime.UtcNow + ExpireDelay;
		}

		public IgnoreEntry(Mobile ignored)
		{
			m_Ignored = ignored;
			Refresh();
		}
	}

	public static void BeginIgnore(Mobile source, Mobile toIgnore)
	{
		ArrayList list = (ArrayList)m_IgnoreLists[source];

		if (list == null)
			m_IgnoreLists[source] = list = new ArrayList();

		for (int i = 0; i < list.Count; ++i)
		{
			IgnoreEntry ie = (IgnoreEntry)list[i];

			if (ie != null && ie.Ignored == toIgnore)
			{
				ie.Refresh();
				return;
			}

			if (ie is {Expired: true})
			{
				list.RemoveAt(i--);
			}
		}

		list.Add(new IgnoreEntry(toIgnore));
	}

	public static bool IsIgnored(Mobile source, Mobile check)
	{
		ArrayList list = (ArrayList)m_IgnoreLists[source];

		if (list == null)
			return false;

		for (int i = 0; i < list.Count; ++i)
		{
			IgnoreEntry ie = (IgnoreEntry)list[i];

			if (ie is {Expired: true})
				list.RemoveAt(i--);
			else if (ie != null && ie.Ignored == check)
				return true;
		}

		return false;
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (info.ButtonID != 1 || !_active || !_context.Registered)
			return;

		_active = false;

		if (!_context.Participants.Contains(_participant))
			return;

		if (info.IsSwitched(1))
		{
			if (_challenged is not PlayerMobile pm)
				return;

			if (pm.DuelContext != null)
			{
				pm.SendMessage(0x22,
					pm.DuelContext.Initiator == pm
						? "You have already started a duel."
						: "You have already been challenged in a duel.");

				_challenger.SendMessage("{0} cannot fight because they are already assigned to another duel.", pm.Name);
			}
			else if (DuelContext.CheckCombat(pm))
			{
				pm.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
				_challenger.SendMessage("{0} cannot fight because they have recently been in combat with another player.", pm.Name);
			}
			else if (TournamentController.IsActive)
			{
				pm.SendMessage(0x22, "A tournament is currently active and you may not duel.");
				_challenger.SendMessage(0x22, "A tournament is currently active and you may not duel.");
			}
			else
			{
				bool added = false;

				if (_slot >= 0 && _slot < _participant.Players.Length && _participant.Players[_slot] == null)
				{
					added = true;
					_participant.Players[_slot] = new DuelPlayer(_challenged, _participant);
				}
				else
				{
					for (int i = 0; i < _participant.Players.Length; ++i)
					{
						if (_participant.Players[i] == null)
						{
							added = true;
							_participant.Players[i] = new DuelPlayer(_challenged, _participant);
							break;
						}
					}
				}

				if (added)
				{
					_challenger.SendMessage("{0} has accepted the request.", _challenged.Name);
					_challenged.SendMessage("You have accepted the request from {0}.", _challenger.Name);

					NetState ns = _challenger.NetState;

					if (ns != null)
					{
						foreach (Gump g in ns.Gumps)
						{
							if (g is ParticipantGump pg)
							{
								if (pg.Participant == _participant)
								{
									_challenger.SendGump(new ParticipantGump(_challenger, _context, _participant));
									break;
								}
							}
							else if (g is DuelContextGump dcg)
							{
								if (dcg.Context == _context)
								{
									_challenger.SendGump(new DuelContextGump(_challenger, _context));
									break;
								}
							}
						}
					}
				}
				else
				{
					_challenger.SendMessage("The participant list was full and so {0} could not join.", _challenged.Name);
					_challenged.SendMessage("The participant list was full and so you could not join the fight {1} {0}.", _challenger.Name, _participant.Contains(_challenger) ? "with" : "against");
				}
			}
		}
		else
		{
			if (info.IsSwitched(3))
				BeginIgnore(_challenged, _challenger);

			_challenger.SendMessage("{0} does not wish to fight.", _challenged.Name);
			_challenged.SendMessage("You chose not to fight {1} {0}.", _challenger.Name, _participant.Contains(_challenger) ? "with" : "against");
		}
	}
}
