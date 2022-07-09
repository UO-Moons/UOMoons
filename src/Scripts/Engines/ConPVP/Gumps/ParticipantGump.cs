using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.ConPVP;

public class ParticipantGump : Gump
{
	public Mobile From { get; }
	public DuelContext Context { get; }
	public Participant Participant { get; }

	public void AddGoldenButton(int x, int y, int bid)
	{
		AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
		AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
	}

	public void AddGoldenButtonLabeled(int x, int y, int bid, string text)
	{
		AddGoldenButton(x, y, bid);
		AddHtml(x + 25, y, 200, 20, text, false, false);
	}

	public ParticipantGump(Mobile from, DuelContext context, Participant p) : base(50, 50)
	{
		From = from;
		Context = context;
		Participant = p;

		from.CloseGump(typeof(RulesetGump));
		from.CloseGump(typeof(DuelContextGump));
		from.CloseGump(typeof(ParticipantGump));

		int count = p.Players.Length;

		if (count < 4)
			count = 4;

		AddPage(0);

		int height = 35 + 10 + 22 + 22 + 30 + 22 + 2 + (count * 22) + 2 + 30;

		AddBackground(0, 0, 300, height, 9250);
		AddBackground(10, 10, 280, height - 20, 0xDAC);

		AddButton(240, 25, 0xFB1, 0xFB3, 3, GumpButtonType.Reply, 0);

		//AddButton( 223, 54, 0x265A, 0x265A, 4, GumpButtonType.Reply, 0 );

		AddHtml(35, 25, 230, 20, Center("Participant Setup"), false, false);

		int x = 35;
		int y = 47;

		AddHtml(x, y, 200, 20, $"Team Size: {p.Players.Length}", false, false); y += 22;

		AddGoldenButtonLabeled(x + 20, y, 1, "Increase"); y += 22;
		AddGoldenButtonLabeled(x + 20, y, 2, "Decrease"); y += 30;

		AddHtml(35, y, 230, 20, Center("Players"), false, false); y += 22;

		for (int i = 0; i < p.Players.Length; ++i)
		{
			DuelPlayer pl = p.Players[i];

			AddGoldenButtonLabeled(x, y, 5 + i, $"{1 + i}: {(pl == null ? "Empty" : pl.Mobile.Name)}"); y += 22;
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (!Context.Registered)
			return;

		int bid = info.ButtonID;

		if (bid == 0)
		{
			From.SendGump(new DuelContextGump(From, Context));
		}
		else if (bid == 1)
		{
			if (Participant.Count < 8)
				Participant.Resize(Participant.Count + 1);
			else
				From.SendMessage("You may not raise the team size any further.");

			From.SendGump(new ParticipantGump(From, Context, Participant));
		}
		else if (bid == 2)
		{
			if (Participant.Count > 1 && Participant.Count > Participant.FilledSlots)
				Participant.Resize(Participant.Count - 1);
			else
				From.SendMessage("You may not lower the team size any further.");

			From.SendGump(new ParticipantGump(From, Context, Participant));
		}
		else if (bid == 3)
		{
			if (Participant.FilledSlots > 0)
			{
				From.SendMessage("There is at least one currently active player. You must remove them first.");
				From.SendGump(new ParticipantGump(From, Context, Participant));
			}
			else if (Context.Participants.Count > 2)
			{
				Context.Participants.Remove(Participant);
				From.SendGump(new DuelContextGump(From, Context));
			}
			else
			{
				From.SendMessage("Duels must have at least two participating parties.");
				From.SendGump(new ParticipantGump(From, Context, Participant));
			}
		}
		else
		{
			bid -= 5;

			if (bid >= 0 && bid < Participant.Players.Length)
			{
				if (Participant.Players[bid] == null)
				{
					From.Target = new ParticipantTarget(Context, Participant, bid);
					From.SendMessage("Target a player.");
				}
				else
				{
					Participant.Players[bid].Mobile.SendMessage("You have been removed from the duel.");

					if (Participant.Players[bid].Mobile is PlayerMobile mobile)
						mobile.DuelPlayer = null;

					Participant.Players[bid] = null;
					From.SendMessage("They have been removed from the duel.");
					From.SendGump(new ParticipantGump(From, Context, Participant));
				}
			}
		}
	}

	private class ParticipantTarget : Target
	{
		private readonly DuelContext _context;
		private readonly Participant _participant;
		private readonly int _index;

		public ParticipantTarget(DuelContext context, Participant p, int index) : base(12, false, TargetFlags.None)
		{
			_context = context;
			_participant = p;
			_index = index;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!_context.Registered)
				return;

			int index = _index;

			if (index < 0 || index >= _participant.Players.Length)
				return;

			if (targeted is not Mobile mob)
			{
				from.SendMessage("That is not a player.");
			}
			else if (!mob.Player)
			{
				mob.SayTo(from, mob.Body.IsHuman ? 1005443 : 1005444);
			}
			else if (AcceptDuelGump.IsIgnored(mob, from) || mob.Blessed)
			{
				from.SendMessage("They ignore your offer.");
			}
			else
			{
				if (mob is not PlayerMobile pm)
					return;

				if (pm.DuelContext != null)
					from.SendMessage("{0} cannot fight because they are already assigned to another duel.", pm.Name);
				else if (DuelContext.CheckCombat(pm))
					from.SendMessage("{0} cannot fight because they have recently been in combat with another player.", pm.Name);
				else if (mob.HasGump(typeof(AcceptDuelGump)))
					from.SendMessage("{0} has already been offered a duel.");
				else
				{
					from.SendMessage("You send {0} to {1}.", _participant.Find(from) == null ? "a challenge" : "an invitation", mob.Name);
					mob.SendGump(new AcceptDuelGump(from, mob, _context, _participant, _index));
				}
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			from.SendGump(new ParticipantGump(from, _context, _participant));
		}
	}
}
