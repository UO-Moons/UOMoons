using Server.Commands;
using Server.Factions;
using Server.Items;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Engines.PartySystem;

public class Party : IParty
{
	private readonly List<Mobile> _mListeners; // staff listening

	public const int Capacity = 10;

	public static void Initialize()
	{
		EventSink.OnLogout += EventSink_Logout;
		EventSink.OnLogin += EventSink_Login;
		EventSink.OnMobileDeath += EventSink_PlayerDeath;

		CommandSystem.Register("ListenToParty", AccessLevel.GameMaster, ListenToParty_OnCommand);
	}

	public static void ListenToParty_OnCommand(CommandEventArgs e)
	{
		e.Mobile.BeginTarget(-1, false, TargetFlags.None, ListenToParty_OnTarget);
		e.Mobile.SendMessage("Target a partied player.");
	}

	public static void ListenToParty_OnTarget(Mobile from, object obj)
	{
		if (obj is Mobile mobile)
		{
			Party p = Get(mobile);

			if (p == null)
			{
				from.SendMessage("They are not in a party.");
			}
			else if (p._mListeners.Contains(from))
			{
				p._mListeners.Remove(from);
				from.SendMessage("You are no longer listening to that party.");
			}
			else
			{
				p._mListeners.Add(from);
				from.SendMessage("You are now listening to that party.");
			}
		}
	}

	public static void EventSink_PlayerDeath(Mobile from, Mobile killer, Container cont)
	{
		Party p = Get(from);

		if (p != null)
		{
			if (killer == from)
				p.SendPublicMessage(from, "I killed myself !!");
			else if (killer == null)
				p.SendPublicMessage(from, "I was killed !!");
			else
				p.SendPublicMessage(from, $"I was killed by {killer.Name} !!");
		}
	}

	private class RejoinTimer : Timer
	{
		private readonly Mobile _mMobile;

		public RejoinTimer(Mobile m) : base(TimeSpan.FromSeconds(1.0))
		{
			_mMobile = m;
		}

		protected override void OnTick()
		{
			Party p = Get(_mMobile);

			if (p == null)
				return;

			_mMobile.SendLocalizedMessage(1005437); // You have rejoined the party.
			_mMobile.Send(new PartyMemberList(p));

			Packet message = Packet.Acquire(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Label, 0x3B2, 3, 1008087, "", AffixType.Prepend | AffixType.System, _mMobile.Name, ""));
			Packet attrs = Packet.Acquire(new MobileAttributesN(_mMobile));

			foreach (PartyMemberInfo mi in p.Members)
			{
				Mobile m = mi.Mobile;

				if (m != _mMobile)
				{
					m.Send(message);
					m.Send(new MobileStatusCompact(_mMobile.CanBeRenamedBy(m), _mMobile));
					m.Send(attrs);
					_mMobile.Send(new MobileStatusCompact(m.CanBeRenamedBy(_mMobile), m));
					_mMobile.Send(new MobileAttributesN(m));
				}
			}

			Packet.Release(message);
			Packet.Release(attrs);
		}
	}

	public static void EventSink_Login(Mobile from)
	{
		Party p = Get(from);

		if (p != null)
			new RejoinTimer(from).Start();
		else
			from.Party = null;
	}

	public static void EventSink_Logout(Mobile from)
	{
		Party p = Get(from);

		p?.Remove(from);

		from.Party = null;
	}

	public static Party Get(Mobile m)
	{
		return m?.Party as Party;
	}

	public Party(Mobile leader)
	{
		Leader = leader;

		Members = new List<PartyMemberInfo>();
		Candidates = new List<Mobile>();
		_mListeners = new List<Mobile>();

		Members.Add(new PartyMemberInfo(leader));
	}

	public void Add(Mobile m)
	{
		PartyMemberInfo mi = this[m];

		if (mi == null)
		{
			Members.Add(new PartyMemberInfo(m));
			m.Party = this;

			Packet memberList = Packet.Acquire(new PartyMemberList(this));
			Packet attrs = Packet.Acquire(new MobileAttributesN(m));

			for (int i = 0; i < Members.Count; ++i)
			{
				Mobile f = Members[i].Mobile;

				f.Send(memberList);

				if (f != m)
				{
					f.Send(new MobileStatusCompact(m.CanBeRenamedBy(f), m));
					f.Send(attrs);
					m.Send(new MobileStatusCompact(f.CanBeRenamedBy(m), f));
					m.Send(new MobileAttributesN(f));
				}
			}

			Packet.Release(memberList);
			Packet.Release(attrs);
		}
	}

	public void OnAccept(Mobile from)
	{
		OnAccept(from, false);
	}

	public void OnAccept(Mobile from, bool force)
	{
		Faction ourFaction = Faction.Find(Leader);
		Faction theirFaction = Faction.Find(from);

		if (!force && ourFaction != null && theirFaction != null && ourFaction != theirFaction)
			return;

		//  : joined the party.
		SendToAll(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Label, 0x3B2, 3, 1008094, "", AffixType.Prepend | AffixType.System, from.Name, ""));

		from.SendLocalizedMessage(1005445); // You have been added to the party.

		Candidates.Remove(from);
		Add(from);
	}

	public void OnDecline(Mobile from, Mobile leader)
	{
		//  : Does not wish to join the party.
		leader.SendLocalizedMessage(1008091, false, from.Name);

		from.SendLocalizedMessage(1008092); // You notify them that you do not wish to join the party.

		Candidates.Remove(from);
		from.Send(new PartyEmptyList(from));

		if (Candidates.Count == 0 && Members.Count <= 1)
		{
			for (int i = 0; i < Members.Count; ++i)
			{
				this[i].Mobile.Send(new PartyEmptyList(this[i].Mobile));
				this[i].Mobile.Party = null;
			}

			Members.Clear();
		}
	}

	public void Remove(Mobile m)
	{
		if (m == Leader)
		{
			Disband();
		}
		else
		{
			for (int i = 0; i < Members.Count; ++i)
			{
				if (Members[i].Mobile == m)
				{
					Members.RemoveAt(i);

					m.Party = null;
					m.Send(new PartyEmptyList(m));

					m.SendLocalizedMessage(1005451); // You have been removed from the party.

					SendToAll(new PartyRemoveMember(m, this));
					SendToAll(1005452); // A player has been removed from your party.

					break;
				}
			}

			if (Members.Count == 1)
			{
				SendToAll(1005450); // The last person has left the party...
				Disband();
			}
		}
	}

	public bool Contains(Mobile m)
	{
		return (this[m] != null);
	}

	public void Disband()
	{
		SendToAll(1005449); // Your party has disbanded.

		for (int i = 0; i < Members.Count; ++i)
		{
			this[i].Mobile.Send(new PartyEmptyList(this[i].Mobile));
			this[i].Mobile.Party = null;
		}

		Members.Clear();
	}

	public static void Invite(Mobile from, Mobile target)
	{
		Faction ourFaction = Faction.Find(from);
		Faction theirFaction = Faction.Find(target);

		if (ourFaction != null && theirFaction != null && ourFaction != theirFaction)
		{
			from.SendLocalizedMessage(1008088); // You cannot have players from opposing factions in the same party!
			target.SendLocalizedMessage(1008093); // The party cannot have members from opposing factions.
			return;
		}

		Party p = Party.Get(from);

		if (p == null)
			from.Party = p = new Party(from);

		if (!p.Candidates.Contains(target))
			p.Candidates.Add(target);

		//  : You are invited to join the party. Type /accept to join or /decline to decline the offer.
		target.Send(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Label, 0x3B2, 3, 1008089, "", AffixType.Prepend | AffixType.System, from.Name, ""));

		from.SendLocalizedMessage(1008090); // You have invited them to join the party.

		target.Send(new PartyInvitation(from));
		target.Party = from;

		DeclineTimer.Start(target, from);
	}

	public void SendToAll(int number)
	{
		SendToAll(number, "", 0x3B2);
	}

	public void SendToAll(int number, string args)
	{
		SendToAll(number, args, 0x3B2);
	}

	public void SendToAll(int number, string args, int hue)
	{
		SendToAll(new MessageLocalized(Serial.MinusOne, -1, MessageType.Regular, hue, 3, number, "System", args));
	}

	public void SendPublicMessage(Mobile from, string text)
	{
		SendToAll(new PartyTextMessage(true, from, text));

		for (int i = 0; i < _mListeners.Count; ++i)
		{
			Mobile mob = _mListeners[i];

			if (mob.Party != this)
				_mListeners[i].SendMessage("[{0}]: {1}", from.Name, text);
		}

		SendToStaffMessage(from, "[Party]: {0}", text);
	}

	public void SendPrivateMessage(Mobile from, Mobile to, string text)
	{
		to.Send(new PartyTextMessage(false, from, text));

		for (int i = 0; i < _mListeners.Count; ++i)
		{
			Mobile mob = _mListeners[i];

			if (mob.Party != this)
				_mListeners[i].SendMessage("[{0}]->[{1}]: {2}", from.Name, to.Name, text);
		}

		SendToStaffMessage(from, "[Party]->[{0}]: {1}", to.Name, text);
	}

	private void SendToStaffMessage(Mobile from, string text)
	{
		Packet p = null;

		foreach (NetState ns in from.GetClientsInRange(8))
		{
			Mobile mob = ns.Mobile;

			if (mob is {AccessLevel: >= AccessLevel.GameMaster} && mob.AccessLevel > from.AccessLevel && mob.Party != this && !_mListeners.Contains(mob))
			{
				p ??= Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language,
					from.Name, text));

				ns.Send(p);
			}
		}

		Packet.Release(p);
	}
	private void SendToStaffMessage(Mobile from, string format, params object[] args)
	{
		SendToStaffMessage(from, string.Format(format, args));
	}

	public void SendToAll(Packet p)
	{
		p.Acquire();

		for (int i = 0; i < Members.Count; ++i)
			Members[i].Mobile.Send(p);

		if (p is MessageLocalized or MessageLocalizedAffix or UnicodeMessage or AsciiMessage)
		{
			for (int i = 0; i < _mListeners.Count; ++i)
			{
				Mobile mob = _mListeners[i];

				if (mob.Party != this)
					mob.Send(p);
			}
		}

		p.Release();
	}

	public void OnStamChanged(Mobile m)
	{
		Packet p = null;

		for (int i = 0; i < Members.Count; ++i)
		{
			Mobile c = Members[i].Mobile;

			if (c != m && m.Map == c.Map && c.InUpdateRange(m) && c.CanSee(m))
			{
				p ??= Packet.Acquire(new MobileStamN(m));

				c.Send(p);
			}
		}

		Packet.Release(p);
	}

	public void OnManaChanged(Mobile m)
	{
		Packet p = null;

		for (int i = 0; i < Members.Count; ++i)
		{
			Mobile c = Members[i].Mobile;

			if (c != m && m.Map == c.Map && c.InUpdateRange(m) && c.CanSee(m))
			{
				p ??= Packet.Acquire(new MobileManaN(m));

				c.Send(p);
			}
		}

		Packet.Release(p);
	}

	public void OnStatsQuery(Mobile beholder, Mobile beheld)
	{
		if (beholder != beheld && Contains(beholder) && beholder.Map == beheld.Map && beholder.InUpdateRange(beheld))
		{
			if (!beholder.CanSee(beheld))
				beholder.Send(new MobileStatusCompact(beheld.CanBeRenamedBy(beholder), beheld));

			beholder.Send(new MobileAttributesN(beheld));
		}
	}

	public int Count => Members.Count;
	public bool Active => Members.Count > 1;
	public Mobile Leader { get; }
	public List<PartyMemberInfo> Members { get; }
	public List<Mobile> Candidates { get; }

	public PartyMemberInfo this[int index] => Members[index];
	public PartyMemberInfo this[Mobile m]
	{
		get
		{
			for (int i = 0; i < Members.Count; ++i)
				if (Members[i].Mobile == m)
					return Members[i];

			return null;
		}
	}
}
