using Server.ContextMenus;
using Server.Ethics;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Engines.ConPVP;

public enum TournamentStage
{
	Inactive,
	Signup,
	Fighting
}

public enum GroupingType
{
	HighVsLow,
	Nearest,
	Random
}

public enum TieType
{
	Random,
	Highest,
	Lowest,
	FullElimination,
	FullAdvancement
}

public class TournamentRegistrar : Banker
{
	[CommandProperty(AccessLevel.GameMaster)]
	public TournamentController Tournament { get; set; }

	[Constructable]
	public TournamentRegistrar()
	{
		Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), Announce_Callback);
	}

	private void Announce_Callback()
	{
		Tournament tourny = null;

		if (Tournament != null)
			tourny = Tournament.Tournament;

		if (tourny is {Stage: TournamentStage.Signup})
			PublicOverheadMessage(MessageType.Regular, 0x35, false, "Come one, come all! Do you aspire to be a fighter of great renown? Join this tournament and show the world your abilities.");
	}

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		base.OnMovement(m, oldLocation);

		Tournament tourny = null;

		if (Tournament != null)
			tourny = Tournament.Tournament;

		if (InRange(m, 4) && !InRange(oldLocation, 4) && tourny is {Stage: TournamentStage.Signup} && m.CanBeginAction(this))
		{
			Ladder ladder = Ladder.Instance;

			LadderEntry entry = ladder?.Find(m);

			if (entry != null && Ladder.GetLevel(entry.Experience) < tourny.LevelRequirement)
				return;

			if (tourny.IsFactionRestricted && Faction.Find(m) == null)
			{
				return;
			}

			if (tourny.HasParticipant(m))
				return;

			PrivateOverheadMessage(MessageType.Regular, 0x35, false,
				$"Hello m'{(m.Female ? "Lady" : "Lord")}. Dost thou wish to enter this tournament? You need only to write your name in this book.", m.NetState);
			m.BeginAction(this);
			Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerStateCallback(ReleaseLock_Callback), m);
		}
	}

	private void ReleaseLock_Callback(object obj)
	{
		((Mobile)obj).EndAction(this);
	}

	public TournamentRegistrar(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(Tournament);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Tournament = reader.ReadItem() as TournamentController;
				break;
			}
		}

		Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), Announce_Callback);
	}
}

public class TournamentSignupItem : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public TournamentController Tournament { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Registrar { get; set; }

	public override string DefaultName => "tournament signup book";

	[Constructable]
	public TournamentSignupItem() : base(4029)
	{
		Movable = false;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
		}
		else
		{
			Tournament tourny = Tournament?.Tournament;

			if (tourny != null)
			{
				if (Registrar != null)
					Registrar.Direction = Registrar.GetDirectionTo(this);

				switch (tourny.Stage)
				{
					case TournamentStage.Fighting:
					{
						if (Registrar != null)
						{
							if (tourny.HasParticipant(from))
							{
								Registrar.PrivateOverheadMessage(MessageType.Regular,
									0x35, false, "Excuse me? You are already signed up.", from.NetState);
							}
							else
							{
								Registrar.PrivateOverheadMessage(MessageType.Regular,
									0x22, false, "The tournament has already begun. You are too late to signup now.", from.NetState);
							}
						}

						break;
					}
					case TournamentStage.Inactive:
					{
						Registrar?.PrivateOverheadMessage(MessageType.Regular,
							0x35, false, "The tournament is closed.", from.NetState);

						break;
					}
					case TournamentStage.Signup:
					{
						Ladder ladder = Ladder.Instance;

						LadderEntry entry = ladder?.Find(from);

						if (entry != null && Ladder.GetLevel(entry.Experience) < tourny.LevelRequirement)
						{
							Registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x35, false, "You have not yet proven yourself a worthy dueler.", from.NetState);

							break;
						}

						if (tourny.IsFactionRestricted && Faction.Find(from) == null)
						{
							Registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x35, false, "Only those who have declared their faction allegiance may participate.", from.NetState);

							break;
						}

						if (from.HasGump(typeof(AcceptTeamGump)))
						{
							Registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x22, false, "You must first respond to the offer I've given you.", from.NetState);
						}
						else if (from.HasGump(typeof(AcceptDuelGump)))
						{
							Registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x22, false, "You must first cancel your duel offer.", from.NetState);
						}
						else if (from is PlayerMobile {DuelContext: { }})
						{
							Registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x22, false, "You are already participating in a duel.", from.NetState);
						}
						else if (!tourny.HasParticipant(from))
						{
							ArrayList players = new()
							{
								from
							};
							from.CloseGump(typeof(ConfirmSignupGump));
							from.SendGump(new ConfirmSignupGump(from, Registrar, tourny, players));
						}
						else
						{
							Registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x35, false, "You have already entered this tournament.", from.NetState);
						}

						break;
					}
				}
			}
		}
	}

	public TournamentSignupItem(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(Tournament);
		writer.Write(Registrar);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Tournament = reader.ReadItem() as TournamentController;
				Registrar = reader.ReadMobile();
				break;
			}
		}
	}
}

public class ConfirmSignupGump : Gump
{
	private readonly Mobile _from;
	private readonly Tournament _tournament;
	private readonly ArrayList _players;
	private readonly Mobile _registrar;

	private const int BlackColor32 = 0x000008;
	private const int LabelColor32 = 0xFFFFFF;

	private void AddBorderedText(int x, int y, int width, int height, string text, int color, int borderColor)
	{
		AddColoredText(x - 1, y - 1, width, height, text, borderColor);
		AddColoredText(x - 1, y + 1, width, height, text, borderColor);
		AddColoredText(x + 1, y - 1, width, height, text, borderColor);
		AddColoredText(x + 1, y + 1, width, height, text, borderColor);
		AddColoredText(x, y, width, height, text, color);
	}

	private void AddColoredText(int x, int y, int width, int height, string text, int color)
	{
		AddHtml(x, y, width, height, color == 0 ? text : Color(text, color), false, false);
	}

	public void AddGoldenButton(int x, int y, int bid)
	{
		AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
		AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
	}

	public ConfirmSignupGump(Mobile from, Mobile registrar, Tournament tourny, ArrayList players) : base(50, 50)
	{
		_from = from;
		_registrar = registrar;
		_tournament = tourny;
		_players = players;

		_from.CloseGump(typeof(AcceptTeamGump));
		_from.CloseGump(typeof(AcceptDuelGump));
		_from.CloseGump(typeof(DuelContextGump));
		_from.CloseGump(typeof(ConfirmSignupGump));

		#region Rules
		Ruleset ruleset = tourny.Ruleset;
		Ruleset basedef = ruleset.Base;

		int height = 185 + 60 + 12;

		int changes = 0;

		BitArray defs;

		if (ruleset.Flavors.Count > 0)
		{
			defs = new BitArray(basedef.Options);

			for (int i = 0; i < ruleset.Flavors.Count; ++i)
			{
				var bitArray = ((Ruleset) ruleset.Flavors[i])?.Options;
				if (bitArray != null)
					defs.Or(bitArray);
			}

			height += ruleset.Flavors.Count * 18;
		}
		else
		{
			defs = basedef.Options;
		}

		BitArray opts = ruleset.Options;

		for (int i = 0; i < opts.Length; ++i)
		{
			if (defs[i] != opts[i])
				++changes;
		}

		height += changes * 22;

		height += 10 + 22 + 25 + 25;

		if (tourny.PlayersPerParticipant > 1)
			height += 36 + tourny.PlayersPerParticipant * 20;
		#endregion

		Closable = false;

		AddPage(0);

		//AddBackground( 0, 0, 400, 220, 9150 );
		AddBackground(1, 1, 398, height, 3600);
		//AddBackground( 16, 15, 369, 189, 9100 );

		AddImageTiled(16, 15, 369, height - 29, 3604);
		AddAlphaRegion(16, 15, 369, height - 29);

		AddImage(215, -43, 0xEE40);
		//AddImage( 330, 141, 0x8BA );

		StringBuilder sb = new();

		if (tourny.TournyType == TournyType.FreeForAll)
		{
			sb.Append("FFA");
		}
		else if (tourny.TournyType == TournyType.RandomTeam)
		{
			sb.Append(tourny.ParticipantsPerMatch);
			sb.Append("-Team");
		}
		else if (tourny.TournyType == TournyType.Faction)
		{
			sb.Append(tourny.ParticipantsPerMatch);
			sb.Append("-Team Faction");
		}
		else if (tourny.TournyType == TournyType.RedVsBlue)
		{
			sb.Append("Red v Blue");
		}
		else
		{
			for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
			{
				if (sb.Length > 0)
					sb.Append('v');

				sb.Append(tourny.PlayersPerParticipant);
			}
		}

		if (tourny.EventController != null)
			sb.Append(' ').Append(tourny.EventController.Title);

		sb.Append(" Tournament Signup");

		AddBorderedText(22, 22, 294, 20, Center(sb.ToString()), LabelColor32, BlackColor32);
		AddBorderedText(22, 50, 294, 40, "You have requested to join the tournament. Do you accept the rules?", 0xB0C868, BlackColor32);

		AddImageTiled(32, 88, 264, 1, 9107);
		AddImageTiled(42, 90, 264, 1, 9157);

		#region Rules
		int y = 100;

		string groupText = null;

		switch (tourny.GroupType)
		{
			case GroupingType.HighVsLow: groupText = "High vs Low"; break;
			case GroupingType.Nearest: groupText = "Closest opponent"; break;
			case GroupingType.Random: groupText = "Random"; break;
		}

		AddBorderedText(35, y, 190, 20, $"Grouping: {groupText}", LabelColor32, BlackColor32);
		y += 20;

		string tieText = tourny.TieType switch
		{
			TieType.Random => "Random",
			TieType.Highest => "Highest advances",
			TieType.Lowest => "Lowest advances",
			TieType.FullAdvancement => tourny.ParticipantsPerMatch == 2 ? "Both advance" : "Everyone advances",
			TieType.FullElimination => tourny.ParticipantsPerMatch == 2 ? "Both eliminated" : "Everyone eliminated",
			_ => null
		};

		AddBorderedText(35, y, 190, 20, $"Tiebreaker: {tieText}", LabelColor32, BlackColor32);
		y += 20;

		string sdText = "Off";

		if (tourny.SuddenDeath > TimeSpan.Zero)
		{
			sdText = $"{(int) tourny.SuddenDeath.TotalMinutes}:{tourny.SuddenDeath.Seconds:D2}";

			sdText = tourny.SuddenDeathRounds > 0 ? $"{sdText} (first {tourny.SuddenDeathRounds} rounds)" : $"{sdText} (all rounds)";
		}

		AddBorderedText(35, y, 240, 20, $"Sudden Death: {sdText}", LabelColor32, BlackColor32);
		y += 20;

		y += 6;
		AddImageTiled(32, y - 1, 264, 1, 9107);
		AddImageTiled(42, y + 1, 264, 1, 9157);
		y += 6;

		AddBorderedText(35, y, 190, 20, $"Ruleset: {basedef.Title}", LabelColor32, BlackColor32);
		y += 20;

		for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
			AddBorderedText(35, y, 190, 20, $" + {((Ruleset) ruleset.Flavors[i])?.Title}", LabelColor32, BlackColor32);

		y += 4;

		if (changes > 0)
		{
			AddBorderedText(35, y, 190, 20, "Modifications:", LabelColor32, BlackColor32);
			y += 20;

			for (int i = 0; i < opts.Length; ++i)
			{
				if (defs[i] != opts[i])
				{
					string name = ruleset.Layout.FindByIndex(i);

					if (name != null) // sanity
					{
						AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
						AddBorderedText(60, y, 165, 22, name, LabelColor32, BlackColor32);
					}

					y += 22;
				}
			}
		}
		else
		{
			AddBorderedText(35, y, 190, 20, "Modifications: None", LabelColor32, BlackColor32);
			y += 20;
		}
		#endregion

		#region Team
		if (tourny.PlayersPerParticipant > 1)
		{
			y += 8;
			AddImageTiled(32, y - 1, 264, 1, 9107);
			AddImageTiled(42, y + 1, 264, 1, 9157);
			y += 8;

			AddBorderedText(35, y, 190, 20, "Your Team", LabelColor32, BlackColor32);
			y += 20;

			for (int i = 0; i < players.Count; ++i, y += 20)
			{
				if (i == 0)
					AddImage(35, y, 0xD2);
				else
					AddGoldenButton(35, y, 1 + i);

				AddBorderedText(60, y, 200, 20, ((Mobile)players[i]).Name, LabelColor32, BlackColor32);
			}

			for (int i = players.Count; i < tourny.PlayersPerParticipant; ++i, y += 20)
			{
				if (i == 0)
					AddImage(35, y, 0xD2);
				else
					AddGoldenButton(35, y, 1 + i);

				AddBorderedText(60, y, 200, 20, "(Empty)", LabelColor32, BlackColor32);
			}
		}
		#endregion

		y += 8;
		AddImageTiled(32, y - 1, 264, 1, 9107);
		AddImageTiled(42, y + 1, 264, 1, 9157);
		y += 8;

		AddRadio(24, y, 9727, 9730, true, 1);
		AddBorderedText(60, y + 5, 250, 20, "Yes, I wish to join the tournament.", LabelColor32, BlackColor32);
		y += 35;

		AddRadio(24, y, 9727, 9730, false, 2);
		AddBorderedText(60, y + 5, 250, 20, "No, I do not wish to join.", LabelColor32, BlackColor32);
		y += 35;

		y -= 3;
		AddButton(314, y, 247, 248, 1, GumpButtonType.Reply, 0);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (info.ButtonID == 1 && info.IsSwitched(1))
		{
			switch (_tournament.Stage)
			{
				case TournamentStage.Fighting:
				{
					if (_registrar != null)
					{
						if (_tournament.HasParticipant(_from))
						{
							_registrar.PrivateOverheadMessage(MessageType.Regular,
								0x35, false, "Excuse me? You are already signed up.", _from.NetState);
						}
						else
						{
							_registrar.PrivateOverheadMessage(MessageType.Regular,
								0x22, false, "The tournament has already begun. You are too late to signup now.", _from.NetState);
						}
					}

					break;
				}
				case TournamentStage.Inactive:
				{
					_registrar?.PrivateOverheadMessage(MessageType.Regular,
						0x35, false, "The tournament is closed.", _from.NetState);

					break;
				}
				case TournamentStage.Signup:
				{
					if (_players.Count != _tournament.PlayersPerParticipant)
					{
						_registrar?.PrivateOverheadMessage(MessageType.Regular,
							0x35, false, "You have not yet chosen your team.", _from.NetState);

						_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
						break;
					}

					Ladder ladder = Ladder.Instance;

					for (int i = 0; i < _players.Count; ++i)
					{
						Mobile mob = (Mobile)_players[i];

						LadderEntry entry = ladder?.Find(mob);

						if (entry != null && Ladder.GetLevel(entry.Experience) < _tournament.LevelRequirement)
						{
							if (mob != null)
								_registrar?.PrivateOverheadMessage(MessageType.Regular,
									0x35, false,
									mob == _from
										? "You have not yet proven yourself a worthy dueler."
										: $"{mob.Name} has not yet proven themselves a worthy dueler.", _from.NetState);

							_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
							return;
						}

						if (_tournament.IsFactionRestricted && Faction.Find(mob) == null)
						{
							_registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x35, false, "Only those who have declared their faction allegiance may participate.", _from.NetState);

							_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
							return;
						}
						if (_tournament.HasParticipant(mob))
						{
							if (mob != null)
								_registrar?.PrivateOverheadMessage(MessageType.Regular,
									0x35, false,
									mob == _from
										? "You have already entered this tournament."
										: $"{mob.Name} has already entered this tournament.", _from.NetState);

							_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
							return;
						}
						if (mob is PlayerMobile {DuelContext: { }})
						{
							_registrar?.PrivateOverheadMessage(MessageType.Regular,
								0x35, false,
								mob == _from
									? "You are already assigned to a duel. You must yield it before joining this tournament."
									: $"{mob.Name} is already assigned to a duel. They must yield it before joining this tournament.",
								_from.NetState);

							_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
							return;
						}
					}

					if (_registrar != null)
					{
						string fmt = _tournament.PlayersPerParticipant switch
						{
							1 =>
								"As you say m'{0}. I've written your name to the bracket. The tournament will begin {1}.",
							2 =>
								"As you wish m'{0}. The tournament will begin {1}, but first you must name your partner.",
							_ => "As you wish m'{0}. The tournament will begin {1}, but first you must name your team."
						};

						int minutesUntil = (int)Math.Round((_tournament.SignupStart + _tournament.SignupPeriod - DateTime.UtcNow).TotalMinutes);

						var timeUntil = minutesUntil == 0 ? "momentarily" : $"in {minutesUntil} minute{(minutesUntil == 1 ? "" : "s")}";

						_registrar.PrivateOverheadMessage(MessageType.Regular,
							0x35, false, string.Format(fmt, _from.Female ? "Lady" : "Lord", timeUntil), _from.NetState);
					}

					TournyParticipant part = new(_from);
					part.Players.Clear();
					part.Players.AddRange(_players);

					_tournament.Participants.Add(part);

					break;
				}
			}
		}
		else if (info.ButtonID > 1)
		{
			int index = info.ButtonID - 1;

			if (index > 0 && index < _players.Count)
			{
				_players.RemoveAt(index);
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
			}
			else if (_players.Count < _tournament.PlayersPerParticipant)
			{
				_from.BeginTarget(12, false, TargetFlags.None, AddPlayer_OnTarget);
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
			}
		}
	}

	private void AddPlayer_OnTarget(Mobile from, object obj)
	{
		if (obj is not Mobile mob || mob == from)
		{
			_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

			_registrar?.PrivateOverheadMessage(MessageType.Regular,
				0x22, false, "Excuse me?", from.NetState);
		}
		else if (!mob.Player)
		{
			_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

			mob.SayTo(from, mob.Body.IsHuman ? 1005443 : 1005444);
		}
		else if (AcceptDuelGump.IsIgnored(mob, from) || mob.Blessed)
		{
			_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

			_registrar?.PrivateOverheadMessage(MessageType.Regular,
				0x22, false, "They ignore your invitation.", from.NetState);
		}
		else
		{
			if (mob is not PlayerMobile pm)
				return;

			if (pm.DuelContext != null)
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They are already assigned to another duel.", from.NetState);
			}
			else if (mob.HasGump(typeof(AcceptTeamGump)))
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They have already been offered a partnership.", from.NetState);
			}
			else if (mob.HasGump(typeof(ConfirmSignupGump)))
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They are already trying to join this tournament.", from.NetState);
			}
			else if (_players.Contains(mob))
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "You have already named them as a team member.", from.NetState);
			}
			else if (_tournament.HasParticipant(mob))
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They have already entered this tournament.", from.NetState);
			}
			else if (_players.Count >= _tournament.PlayersPerParticipant)
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "Your team is full.", from.NetState);
			}
			else
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));
				mob.SendGump(new AcceptTeamGump(from, mob, _tournament, _registrar, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x59, false,
					$"As you command m'{(from.Female ? "Lady" : "Lord")}. I've given your offer to {mob.Name}.", from.NetState);
			}
		}
	}
}

public class AcceptTeamGump : Gump
{
	private bool _active;

	private readonly Mobile _from;
	private readonly Mobile _requested;
	private readonly Tournament _tournament;
	private readonly Mobile _registrar;
	private readonly ArrayList _players;

	private const int BlackColor32 = 0x000008;
	private const int LabelColor32 = 0xFFFFFF;

	private void AddBorderedText(int x, int y, int width, int height, string text, int color, int borderColor)
	{
		AddColoredText(x - 1, y - 1, width, height, text, borderColor);
		AddColoredText(x - 1, y + 1, width, height, text, borderColor);
		AddColoredText(x + 1, y - 1, width, height, text, borderColor);
		AddColoredText(x + 1, y + 1, width, height, text, borderColor);
		AddColoredText(x, y, width, height, text, color);
	}

	private void AddColoredText(int x, int y, int width, int height, string text, int color)
	{
		AddHtml(x, y, width, height, color == 0 ? text : Color(text, color), false, false);
	}

	public AcceptTeamGump(Mobile from, Mobile requested, Tournament tourny, Mobile registrar, ArrayList players) : base(50, 50)
	{
		_from = from;
		_requested = requested;
		_tournament = tourny;
		_registrar = registrar;
		_players = players;

		_active = true;

		#region Rules
		Ruleset ruleset = tourny.Ruleset;
		Ruleset basedef = ruleset.Base;

		int height = 185 + 35 + 60 + 12;

		int changes = 0;

		BitArray defs;

		if (ruleset.Flavors.Count > 0)
		{
			defs = new BitArray(basedef.Options);

			for (int i = 0; i < ruleset.Flavors.Count; ++i)
			{
				var bitArray = ((Ruleset) ruleset.Flavors[i])?.Options;
				if (bitArray != null)
					defs.Or(bitArray);
			}

			height += ruleset.Flavors.Count * 18;
		}
		else
		{
			defs = basedef.Options;
		}

		BitArray opts = ruleset.Options;

		for (int i = 0; i < opts.Length; ++i)
		{
			if (defs[i] != opts[i])
				++changes;
		}

		height += changes * 22;

		height += 10 + 22 + 25 + 25;
		#endregion

		Closable = false;

		AddPage(0);

		AddBackground(1, 1, 398, height, 3600);

		AddImageTiled(16, 15, 369, height - 29, 3604);
		AddAlphaRegion(16, 15, 369, height - 29);

		AddImage(215, -43, 0xEE40);

		StringBuilder sb = new();

		if (tourny.TournyType == TournyType.FreeForAll)
		{
			sb.Append("FFA");
		}
		else if (tourny.TournyType == TournyType.RandomTeam)
		{
			sb.Append(tourny.ParticipantsPerMatch);
			sb.Append("-Team");
		}
		else if (tourny.TournyType == TournyType.Faction)
		{
			sb.Append(tourny.ParticipantsPerMatch);
			sb.Append("-Team Faction");
		}
		else if (tourny.TournyType == TournyType.RedVsBlue)
		{
			sb.Append("Red v Blue");
		}
		else
		{
			for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
			{
				if (sb.Length > 0)
					sb.Append('v');

				sb.Append(tourny.PlayersPerParticipant);
			}
		}

		if (tourny.EventController != null)
			sb.Append(' ').Append(tourny.EventController.Title);

		sb.Append(" Tournament Invitation");

		AddBorderedText(22, 22, 294, 20, Center(sb.ToString()), LabelColor32, BlackColor32);

		AddBorderedText(22, 50, 294, 40,
			$"You have been asked to partner with {from.Name} in a tournament. Do you accept?",
			0xB0C868, BlackColor32);

		AddImageTiled(32, 88, 264, 1, 9107);
		AddImageTiled(42, 90, 264, 1, 9157);

		#region Rules
		int y = 100;

		string groupText = null;

		switch (tourny.GroupType)
		{
			case GroupingType.HighVsLow: groupText = "High vs Low"; break;
			case GroupingType.Nearest: groupText = "Closest opponent"; break;
			case GroupingType.Random: groupText = "Random"; break;
		}

		AddBorderedText(35, y, 190, 20, $"Grouping: {groupText}", LabelColor32, BlackColor32);
		y += 20;

		string tieText = tourny.TieType switch
		{
			TieType.Random => "Random",
			TieType.Highest => "Highest advances",
			TieType.Lowest => "Lowest advances",
			TieType.FullAdvancement => tourny.ParticipantsPerMatch == 2 ? "Both advance" : "Everyone advances",
			TieType.FullElimination => tourny.ParticipantsPerMatch == 2 ? "Both eliminated" : "Everyone eliminated",
			_ => null
		};

		AddBorderedText(35, y, 190, 20, $"Tiebreaker: {tieText}", LabelColor32, BlackColor32);
		y += 20;

		string sdText = "Off";

		if (tourny.SuddenDeath > TimeSpan.Zero)
		{
			sdText = $"{(int) tourny.SuddenDeath.TotalMinutes}:{tourny.SuddenDeath.Seconds:D2}";

			sdText = tourny.SuddenDeathRounds > 0 ? $"{sdText} (first {tourny.SuddenDeathRounds} rounds)" : $"{sdText} (all rounds)";
		}

		AddBorderedText(35, y, 240, 20, $"Sudden Death: {sdText}", LabelColor32, BlackColor32);
		y += 20;

		y += 6;
		AddImageTiled(32, y - 1, 264, 1, 9107);
		AddImageTiled(42, y + 1, 264, 1, 9157);
		y += 6;

		AddBorderedText(35, y, 190, 20, $"Ruleset: {basedef.Title}", LabelColor32, BlackColor32);
		y += 20;

		for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
			AddBorderedText(35, y, 190, 20, $" + {((Ruleset) ruleset.Flavors[i])?.Title}", LabelColor32, BlackColor32);

		y += 4;

		if (changes > 0)
		{
			AddBorderedText(35, y, 190, 20, "Modifications:", LabelColor32, BlackColor32);
			y += 20;

			for (int i = 0; i < opts.Length; ++i)
			{
				if (defs[i] != opts[i])
				{
					string name = ruleset.Layout.FindByIndex(i);

					if (name != null) // sanity
					{
						AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
						AddBorderedText(60, y, 165, 22, name, LabelColor32, BlackColor32);
					}

					y += 22;
				}
			}
		}
		else
		{
			AddBorderedText(35, y, 190, 20, "Modifications: None", LabelColor32, BlackColor32);
			y += 20;
		}
		#endregion

		y += 8;
		AddImageTiled(32, y - 1, 264, 1, 9107);
		AddImageTiled(42, y + 1, 264, 1, 9157);
		y += 8;

		AddRadio(24, y, 9727, 9730, true, 1);
		AddBorderedText(60, y + 5, 250, 20, "Yes, I will join them.", LabelColor32, BlackColor32);
		y += 35;

		AddRadio(24, y, 9727, 9730, false, 2);
		AddBorderedText(60, y + 5, 250, 20, "No, I do not wish to fight.", LabelColor32, BlackColor32);
		y += 35;

		AddRadio(24, y, 9727, 9730, false, 3);
		AddBorderedText(60, y + 5, 270, 20, "No, most certainly not. Do not ask again.", LabelColor32, BlackColor32);
		y += 35;

		y -= 3;
		AddButton(314, y, 247, 248, 1, GumpButtonType.Reply, 0);

		Timer.DelayCall(TimeSpan.FromSeconds(15.0), AutoReject);
	}

	public void AutoReject()
	{
		if (!_active)
			return;

		_active = false;

		_requested.CloseGump(typeof(AcceptTeamGump));
		_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

		if (_registrar != null)
		{
			_registrar.PrivateOverheadMessage(MessageType.Regular,
				0x22, false, $"{_requested.Name} seems unresponsive.", _from.NetState);

			_registrar.PrivateOverheadMessage(MessageType.Regular,
				0x22, false, $"You have declined the partnership with {_from.Name}.", _requested.NetState);
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (info.ButtonID != 1 || !_active)
			return;

		_active = false;

		if (info.IsSwitched(1))
		{
			if (_requested is not PlayerMobile pm)
				return;

			if (AcceptDuelGump.IsIgnored(_requested, _from) || _requested.Blessed)
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They ignore your invitation.", _from.NetState);
			}
			else if (pm.DuelContext != null)
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They are already assigned to another duel.", _from.NetState);
			}
			else if (_players.Contains(_requested))
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "You have already named them as a team member.", _from.NetState);
			}
			else if (_tournament.HasParticipant(_requested))
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "They have already entered this tournament.", _from.NetState);
			}
			else if (_players.Count >= _tournament.PlayersPerParticipant)
			{
				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				_registrar?.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, "Your team is full.", _from.NetState);
			}
			else
			{
				_players.Add(_requested);

				_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

				if (_registrar != null)
				{
					_registrar.PrivateOverheadMessage(MessageType.Regular,
						0x59, false, $"{_requested.Name} has accepted your offer of partnership.", _from.NetState);

					_registrar.PrivateOverheadMessage(MessageType.Regular,
						0x59, false, $"You have accepted the partnership with {_from.Name}.", _requested.NetState);
				}
			}
		}
		else
		{
			if (info.IsSwitched(3))
				AcceptDuelGump.BeginIgnore(_requested, _from);

			_from.SendGump(new ConfirmSignupGump(_from, _registrar, _tournament, _players));

			if (_registrar != null)
			{
				_registrar.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, $"{_requested.Name} has declined your offer of partnership.", _from.NetState);

				_registrar.PrivateOverheadMessage(MessageType.Regular,
					0x22, false, $"You have declined the partnership with {_from.Name}.", _requested.NetState);
			}
		}
	}
}

public class TournamentController : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public Tournament Tournament { get; set; }

	private static readonly ArrayList m_Instances = new();

	public static bool IsActive
	{
		get
		{
			return m_Instances.Cast<TournamentController>().Any(controller => controller is {Deleted: false, Tournament: { }} && controller.Tournament.Stage != TournamentStage.Inactive);
		}
	}

	public override string DefaultName => "tournament controller";

	[Constructable]
	public TournamentController() : base(0x1B7A)
	{
		Visible = false;
		Movable = false;

		Tournament = new Tournament();
		m_Instances.Add(this);
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (from.AccessLevel >= AccessLevel.GameMaster && Tournament != null)
		{
			list.Add(new EditEntry(Tournament));

			if (Tournament.CurrentStage == TournamentStage.Inactive)
				list.Add(new StartEntry(Tournament));
		}
	}

	private class EditEntry : ContextMenuEntry
	{
		private readonly Tournament _tournament;

		public EditEntry(Tournament tourny) : base(5101)
		{
			_tournament = tourny;
		}

		public override void OnClick()
		{
			Owner.From.SendGump(new PropertiesGump(Owner.From, _tournament));
		}
	}

	private class StartEntry : ContextMenuEntry
	{
		private readonly Tournament _tournament;

		public StartEntry(Tournament tourny) : base(5113)
		{
			_tournament = tourny;
		}

		public override void OnClick()
		{
			if (_tournament.Stage == TournamentStage.Inactive)
			{
				_tournament.SignupStart = DateTime.UtcNow;
				_tournament.Stage = TournamentStage.Signup;
				_tournament.Participants.Clear();
				_tournament.Pyramid.Levels.Clear();
				_tournament.Alert("Hear ye! Hear ye!", "Tournament signup has opened. You can enter by signing up with the registrar.");
			}
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.AccessLevel >= AccessLevel.GameMaster && Tournament != null)
		{
			from.CloseGump(typeof(PickRulesetGump));
			from.CloseGump(typeof(RulesetGump));
			from.SendGump(new PickRulesetGump(from, null, Tournament.Ruleset));
		}
	}

	public TournamentController(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		Tournament.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		Tournament = version switch
		{
			0 => new Tournament(reader),
			_ => Tournament
		};

		m_Instances.Add(this);
	}

	public override void OnDelete()
	{
		base.OnDelete();

		m_Instances.Remove(this);
	}
}

public enum TournyType
{
	Standard,
	FreeForAll,
	RandomTeam,
	RedVsBlue,
	Faction
}

[PropertyObject]
public class Tournament
{
	private int _participantsPerMatch;
	private int _playersPerParticipant;

	public bool IsNotoRestricted => TournyType != TournyType.Standard;

	[CommandProperty(AccessLevel.GameMaster)]
	public EventController EventController { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SuddenDeathRounds { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TournyType TournyType { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public GroupingType GroupType { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TieType TieType { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan SuddenDeath { get; set; }

	public Ruleset Ruleset { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ParticipantsPerMatch
	{
		get => _participantsPerMatch;
		set
		{
			value = value switch
			{
				< 2 => 2,
				> 10 => 10,
				_ => value
			};
			_participantsPerMatch = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int PlayersPerParticipant
	{
		get => _playersPerParticipant;
		set
		{
			value = value switch
			{
				< 1 => 1,
				> 10 => 10,
				_ => value
			};
			_playersPerParticipant = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int LevelRequirement { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool FactionRestricted { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan SignupPeriod { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime SignupStart { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TournamentStage CurrentStage { get; private set; }

	public TournamentStage Stage
	{
		get => CurrentStage;
		set => CurrentStage = value;
	}

	public TournyPyramid Pyramid { get; set; }

	public ArrayList Arenas { get; set; }

	public ArrayList Participants { get; set; }

	public ArrayList Undefeated { get; set; }

	public bool IsFactionRestricted => FactionRestricted || TournyType == TournyType.Faction;

	public bool HasParticipant(Mobile mob)
	{
		for (int i = 0; i < Participants.Count; ++i)
		{
			TournyParticipant part = (TournyParticipant)Participants[i];

			if (part != null && part.Players.Contains(mob))
				return true;
		}

		return false;
	}

	public void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(0); // version

		writer.Write(FactionRestricted);

		writer.Write(EventController);

		writer.WriteEncodedInt(SuddenDeathRounds);

		writer.WriteEncodedInt((int)TournyType);

		writer.WriteEncodedInt((int)GroupType);
		writer.WriteEncodedInt((int)TieType);
		writer.Write(SuddenDeath);

		writer.WriteEncodedInt(_participantsPerMatch);
		writer.WriteEncodedInt(_playersPerParticipant);
		writer.Write(SignupPeriod);
	}

	public Tournament(GenericReader reader)
	{
		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 0:
			{
				FactionRestricted = reader.ReadBool();

				EventController = reader.ReadItem() as EventController;

				SuddenDeathRounds = reader.ReadEncodedInt();

				TournyType = (TournyType)reader.ReadEncodedInt();

				GroupType = (GroupingType)reader.ReadEncodedInt();
				TieType = (TieType)reader.ReadEncodedInt();
				SignupPeriod = reader.ReadTimeSpan();

				_participantsPerMatch = reader.ReadEncodedInt();
				_playersPerParticipant = reader.ReadEncodedInt();
				SignupPeriod = reader.ReadTimeSpan();
				CurrentStage = TournamentStage.Inactive;
				Pyramid = new TournyPyramid();
				Ruleset = new Ruleset(RulesetLayout.Root);
				Ruleset.ApplyDefault(Ruleset.Layout.Defaults[0]);
				Participants = new ArrayList();
				Undefeated = new ArrayList();
				Arenas = new ArrayList();

				break;
			}
		}

		Timer.DelayCall(SliceInterval, SliceInterval, Slice);
	}

	public Tournament()
	{
		_participantsPerMatch = 2;
		_playersPerParticipant = 1;
		Pyramid = new TournyPyramid();
		Ruleset = new Ruleset(RulesetLayout.Root);
		Ruleset.ApplyDefault(Ruleset.Layout.Defaults[0]);
		Participants = new ArrayList();
		Undefeated = new ArrayList();
		Arenas = new ArrayList();
		SignupPeriod = TimeSpan.FromMinutes(10.0);

		Timer.DelayCall(SliceInterval, SliceInterval, Slice);
	}

	public void HandleTie(Arena arena, TournyMatch match, ArrayList remaining)
	{
		if (remaining.Count == 1)
			HandleWon(arena, match, (TournyParticipant)remaining[0]);

		if (remaining.Count < 2)
			return;

		StringBuilder sb = new();

		sb.Append("The match has ended in a tie ");

		sb.Append(remaining.Count == 2 ? "between " : "among ");

		sb.Append(remaining.Count);

		sb.Append(((TournyParticipant) remaining[0]).Players.Count == 1 ? " players: " : " teams: ");

		bool hasAppended = false;

		for (int j = 0; j < match.Participants.Count; ++j)
		{
			TournyParticipant part = (TournyParticipant)match.Participants[j];

			if (remaining.Contains(part))
			{
				if (hasAppended)
					sb.Append(", ");

				if (part != null) sb.Append(part.NameList);
				hasAppended = true;
			}
			else
			{
				Undefeated.Remove(part);
			}
		}

		sb.Append(". ");

		string whole = remaining.Count == 2 ? "both" : "all";

		TieType tieType = TieType;

		if (tieType == TieType.FullElimination && remaining.Count >= Undefeated.Count)
			_ = TieType.FullAdvancement;

		switch (TieType)
		{
			case TieType.FullAdvancement:
			{
				sb.AppendFormat("In accordance with the rules, {0} parties are advanced.", whole);
				break;
			}
			case TieType.FullElimination:
			{
				for (int j = 0; j < remaining.Count; ++j)
					Undefeated.Remove(remaining[j]);

				sb.AppendFormat("In accordance with the rules, {0} parties are eliminated.", whole);
				break;
			}
			case TieType.Random:
			{
				TournyParticipant advanced = (TournyParticipant)remaining[Utility.Random(remaining.Count)];

				for (int i = 0; i < remaining.Count; ++i)
				{
					if (remaining[i] != advanced)
						Undefeated.Remove(remaining[i]);
				}

				if (advanced != null)
					sb.AppendFormat("In accordance with the rules, {0} {1} advanced.", advanced.NameList, advanced.Players.Count == 1 ? "is" : "are");

				break;
			}
			case TieType.Highest:
			{
				TournyParticipant advanced = null;

				for (int i = 0; i < remaining.Count; ++i)
				{
					TournyParticipant part = (TournyParticipant)remaining[i];

					if (advanced == null || part.TotalLadderXp > advanced.TotalLadderXp)
						advanced = part;
				}

				for (int i = 0; i < remaining.Count; ++i)
				{
					if (remaining[i] != advanced)
						Undefeated.Remove(remaining[i]);
				}

				if (advanced != null)
					sb.Append(
						$"In accordance with the rules, {advanced.NameList} {(advanced.Players.Count == 1 ? "is" : "are")} advanced.");

				break;
			}
			case TieType.Lowest:
			{
				TournyParticipant advanced = null;

				for (int i = 0; i < remaining.Count; ++i)
				{
					TournyParticipant part = (TournyParticipant)remaining[i];

					if (advanced == null || part.TotalLadderXp < advanced.TotalLadderXp)
						advanced = part;
				}

				for (int i = 0; i < remaining.Count; ++i)
				{
					if (remaining[i] != advanced)
						Undefeated.Remove(remaining[i]);
				}

				if (advanced != null)
					sb.Append(
						$"In accordance with the rules, {advanced.NameList} {(advanced.Players.Count == 1 ? "is" : "are")} advanced.");

				break;
			}
		}

		Alert(arena, sb.ToString());
	}

	public void OnEliminated(DuelPlayer player)
	{
		Participant part = player.Participant;

		if (!part.Eliminated)
			return;

		if (TournyType == TournyType.FreeForAll)
		{
			int rem = part.Context.Participants.Cast<Participant>().Count(check => check is {Eliminated: false});

			TournyParticipant tp = part.TournyPart;

			if (tp == null)
				return;

			switch (rem)
			{
				case 1:
					GiveAwards(tp.Players, TrophyRank.Silver, ComputeCashAward() / 2);
					break;
				case 2:
					GiveAwards(tp.Players, TrophyRank.Bronze, ComputeCashAward() / 4);
					break;
			}
		}
	}

	public void HandleWon(Arena arena, TournyMatch match, TournyParticipant winner)
	{
		StringBuilder sb = new();

		sb.Append("The match is complete. ");
		sb.Append(winner.NameList);

		sb.Append(winner.Players.Count > 1 ? " have bested " : " has bested ");

		if (match.Participants.Count > 2)
			sb.AppendFormat("{0} other {1}: ", match.Participants.Count - 1, winner.Players.Count == 1 ? "players" : "teams");

		bool hasAppended = false;

		for (int j = 0; j < match.Participants.Count; ++j)
		{
			TournyParticipant part = (TournyParticipant)match.Participants[j];

			if (part == winner)
				continue;

			Undefeated.Remove(part);

			if (hasAppended)
				sb.Append(", ");

			if (part != null) sb.Append(part.NameList);
			hasAppended = true;
		}

		sb.Append('.');

		if (TournyType == TournyType.Standard)
			Alert(arena, sb.ToString());
	}

	private static readonly TimeSpan SliceInterval = TimeSpan.FromSeconds(12.0);

	private int ComputeCashAward()
	{
		return Participants.Count * _playersPerParticipant * 2500;
	}

	private void GiveAwards()
	{
		switch (TournyType)
		{
			case TournyType.FreeForAll:
			{
				if (Pyramid.Levels.Count < 1)
					break;

				PyramidLevel top = Pyramid.Levels[^1] as PyramidLevel;

				if (top != null && (top.FreeAdvance != null || top.Matches.Count != 1))
					break;

				if (top?.Matches[0] is TournyMatch match)
				{
					TournyParticipant winner = match.Winner;

					if (winner != null)
						GiveAwards(winner.Players, TrophyRank.Gold, ComputeCashAward());
				}

				break;
			}
			case TournyType.Standard:
			{
				if (Pyramid.Levels.Count < 2)
					break;

				PyramidLevel top = Pyramid.Levels[^1] as PyramidLevel;

				if (top != null && (top.FreeAdvance != null || top.Matches.Count != 1))
					break;

				int cash = ComputeCashAward();

				if (top?.Matches[0] is TournyMatch match)
				{
					TournyParticipant winner = match.Winner;

					for (int i = 0; i < match.Participants.Count; ++i)
					{
						TournyParticipant part = (TournyParticipant) match.Participants[i];

						if (part == winner)
						{
							if (part != null) GiveAwards(part.Players, TrophyRank.Gold, cash);
						}
						else if (part != null) GiveAwards(part.Players, TrophyRank.Silver, cash / 2);
					}

					PyramidLevel next = Pyramid.Levels[^2] as PyramidLevel;

					if (next != null && next.Matches.Count > 2)
						break;

					if (next != null)
					{

						for (int i = 0; i < next.Matches.Count; ++i)
						{
							match = (TournyMatch) next.Matches[i];
							if (match != null)
							{
								winner = match.Winner;

								for (int j = 0; j < match.Participants.Count; ++j)
								{
									TournyParticipant part = (TournyParticipant) match.Participants[j];

									if (part != winner)
										if (part != null)
											GiveAwards(part.Players, TrophyRank.Bronze, cash / 4);
								}
							}
						}
					}
				}

				break;
			}
		}
	}

	private void GiveAwards(IList players, TrophyRank rank, int cash)
	{
		switch (players.Count)
		{
			case 0:
				return;
			case > 1:
				cash /= players.Count - 1;
				break;
		}

		cash += 500;
		cash /= 1000;
		cash *= 1000;

		StringBuilder sb = new();

		if (TournyType == TournyType.FreeForAll)
		{
			sb.Append(Participants.Count * _playersPerParticipant);
			sb.Append("-man FFA");
		}
		else if (TournyType == TournyType.RandomTeam)
		{
			sb.Append(_participantsPerMatch);
			sb.Append("-Team");
		}
		else if (TournyType == TournyType.Faction)
		{
			sb.Append(_participantsPerMatch);
			sb.Append("-Team Faction");
		}
		else if (TournyType == TournyType.RedVsBlue)
		{
			sb.Append("Red v Blue");
		}
		else
		{
			for (int i = 0; i < _participantsPerMatch; ++i)
			{
				if (sb.Length > 0)
					sb.Append('v');

				sb.Append(_playersPerParticipant);
			}
		}

		if (EventController != null)
			sb.Append(' ').Append(EventController.Title);

		sb.Append(" Champion");

		string title = sb.ToString();

		for (int i = 0; i < players.Count; ++i)
		{
			Mobile mob = (Mobile)players[i];

			if (mob == null || mob.Deleted)
				continue;

			Item item = new Trophy(title, rank);

			if (!mob.PlaceInBackpack(item))
				mob.BankBox.DropItem(item);

			if (cash > 0)
			{
				item = new BankCheck(cash);

				if (!mob.PlaceInBackpack(item))
					mob.BankBox.DropItem(item);

				mob.SendMessage("You have been awarded a {0} trophy and {1:N0}gp for your participation in this tournament.", rank.ToString().ToLower(), cash);
			}
			else
			{
				mob.SendMessage("You have been awarded a {0} trophy for your participation in this tournament.", rank.ToString().ToLower());
			}
		}
	}

	public void Slice()
	{
		if (CurrentStage == TournamentStage.Signup)
		{
			TimeSpan until = SignupStart + SignupPeriod - DateTime.UtcNow;

			if (until <= TimeSpan.Zero)
			{
				for (int i = Participants.Count - 1; i >= 0; --i)
				{
					TournyParticipant part = (TournyParticipant)Participants[i];
					bool bad = false;

					if (part != null)
					{
						if (part.Players.Cast<Mobile>().Any(check => check != null && (check.Deleted ||
							    check.Map == null || check.Map == Map.Internal || !check.Alive ||
							    Sigil.ExistsOn(check) || check.Region.IsPartOf(typeof(Regions.Jail)))))
						{
							bad = true;
						}
					}

					if (bad)
					{
						for (int j = 0; j < part.Players.Count; ++j)
							((Mobile)part.Players[j])?.SendMessage("You have been disqualified from the tournament.");

						Participants.RemoveAt(i);
					}
				}

				if (Participants.Count >= 2)
				{
					CurrentStage = TournamentStage.Fighting;

					Undefeated.Clear();

					Pyramid.Levels.Clear();
					Pyramid.AddLevel(_participantsPerMatch, Participants, GroupType, TournyType);

					PyramidLevel level = (PyramidLevel)Pyramid.Levels[0];

					if (level is {FreeAdvance: { }})
						Undefeated.Add(level.FreeAdvance);

					if (level != null)
						for (int i = 0; i < level.Matches.Count; ++i)
						{
							TournyMatch match = (TournyMatch) level.Matches[i];

							if (match != null) Undefeated.AddRange(match.Participants);
						}

					Alert("Hear ye! Hear ye!", "The tournament will begin shortly.");
				}
				else
				{
					/*Alert( "Is this all?", "Pitiful. Signup extended." );
					m_SignupStart = DateTime.UtcNow;*/

					Alert("Is this all?", "Pitiful. Tournament cancelled.");
					CurrentStage = TournamentStage.Inactive;
				}
			}
			else if (Math.Abs(until.TotalSeconds - TimeSpan.FromMinutes(1.0).TotalSeconds) < SliceInterval.TotalSeconds / 2)
			{
				Alert("Last call!", "If you wish to enter the tournament, sign up with the registrar now.");
			}
			else if (Math.Abs(until.TotalSeconds - TimeSpan.FromMinutes(5.0).TotalSeconds) < SliceInterval.TotalSeconds / 2)
			{
				Alert("The tournament will begin in 5 minutes.", "Sign up now before it's too late.");
			}
		}
		else if (CurrentStage == TournamentStage.Fighting)
		{
			if (Undefeated.Count == 1)
			{
				TournyParticipant winner = (TournyParticipant)Undefeated[0];

				try
				{
					if (EventController != null)
						Alert("The tournament has completed!",
							$"Team {EventController.GetTeamName((((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner))} has won!");
					else switch (TournyType)
					{
						case TournyType.RandomTeam:
							Alert("The tournament has completed!",
								$"Team {(((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) + 1} has won!");
							break;
						case TournyType.Faction when _participantsPerMatch == 4:
						{
							string name = "(null)";

							switch ((((TournyMatch)((PyramidLevel)Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner))
							{
								case 0:
								{
									name = "Minax";
									break;
								}
								case 1:
								{
									name = "Council of Mages";
									break;
								}
								case 2:
								{
									name = "True Britannians";
									break;
								}
								case 3:
								{
									name = "Shadowlords";
									break;
								}
							}

							Alert("The tournament has completed!", $"The {name} team has won!");
							break;
						}
						case TournyType.Faction when _participantsPerMatch == 2:
							Alert("The tournament has completed!",
								$"The {((((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) == 0 ? "Evil" : "Hero")} team has won!");
							break;
						case TournyType.Faction:
							Alert("The tournament has completed!",
								$"Team {(((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) + 1} has won!");
							break;
						case TournyType.RedVsBlue:
							Alert("The tournament has completed!",
								$"Team {((((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) == 0 ? "Red" : "Blue")} has won!");
							break;
						default:
							if (winner != null)
								Alert("The tournament has completed!",
									$"{winner.NameList} {(winner.Players.Count > 1 ? "are" : "is")} the champion{(winner.Players.Count == 1 ? "" : "s")}.");
							break;
					}
				}
				catch
				{
					// ignored
				}

				GiveAwards();

				CurrentStage = TournamentStage.Inactive;
				Undefeated.Clear();
			}
			else if (Pyramid.Levels.Count > 0)
			{
				PyramidLevel activeLevel = (PyramidLevel)Pyramid.Levels[^1];
				bool stillGoing = false;

				if (activeLevel != null)
				{
					for (int i = 0; i < activeLevel.Matches.Count; ++i)
					{
						TournyMatch match = (TournyMatch) activeLevel.Matches[i];

						if (match is {Winner: null})
						{
							stillGoing = true;

							if (!match.InProgress)
							{
								for (int j = 0; j < Arenas.Count; ++j)
								{
									Arena arena = (Arena) Arenas[j];

									if (arena is {IsOccupied: false})
									{
										match.Start(arena, this);
										break;
									}
								}
							}
						}
					}
				}

				if (!stillGoing)
				{
					for (int i = Undefeated.Count - 1; i >= 0; --i)
					{
						TournyParticipant part = (TournyParticipant)Undefeated[i];
						bool bad = false;

						if (part != null)
						{
							for (int j = 0; j < part.Players.Count; ++j)
							{
								Mobile check = (Mobile) part.Players[j];

								if (check != null && (check.Deleted || check.Map == null || check.Map == Map.Internal ||
								                      !check.Alive || Sigil.ExistsOn(check) ||
								                      check.Region.IsPartOf(typeof(Regions.Jail))))
								{
									bad = true;
									break;
								}
							}
						}

						if (bad)
						{
							for (int j = 0; j < part.Players.Count; ++j)
								((Mobile)part.Players[j])?.SendMessage("You have been disqualified from the tournament.");

							Undefeated.RemoveAt(i);

							if (Undefeated.Count == 1)
							{
								TournyParticipant winner = (TournyParticipant)Undefeated[0];

								try
								{
									if (EventController != null)
										Alert("The tournament has completed!",
											$"Team {EventController.GetTeamName((((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner))} has won");
									else if (TournyType == TournyType.RandomTeam)
										Alert("The tournament has completed!",
											$"Team {(((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) + 1} has won!");
									else if (TournyType == TournyType.Faction)
									{
										if (_participantsPerMatch == 4)
										{
											string name =
												(((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!)
													.Participants.IndexOf(winner) switch
													{
														0 => "Minax",
														1 => "Council of Mages",
														2 => "True Britannians",
														3 => "Shadowlords",
														_ => "(null)"
													};

											Alert("The tournament has completed!", $"The {name} team has won!");
										}
										else if (_participantsPerMatch == 2)
										{
											Alert("The tournament has completed!",
												$"The {((((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) == 0 ? "Evil" : "Hero")} team has won!");
										}
										else
										{
											Alert("The tournament has completed!",
												$"Team {(((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) + 1} has won!");
										}
									}
									else if (TournyType == TournyType.RedVsBlue)
										Alert("The tournament has completed!",
											$"Team {((((TournyMatch) ((PyramidLevel) Pyramid.Levels[0])?.Matches[0])!).Participants.IndexOf(winner) == 0 ? "Red" : "Blue")} has won!");
									else if (winner != null)
										Alert("The tournament has completed!",
											$"{winner.NameList} {(winner.Players.Count > 1 ? "are" : "is")} the champion{(winner.Players.Count == 1 ? "" : "s")}.");
								}
								catch
								{
									// ignored
								}

								GiveAwards();

								CurrentStage = TournamentStage.Inactive;
								Undefeated.Clear();
								break;
							}
						}
					}

					if (Undefeated.Count > 1)
						Pyramid.AddLevel(_participantsPerMatch, Undefeated, GroupType, TournyType);
				}
			}
		}
	}

	public void Alert(params string[] alerts)
	{
		for (int i = 0; i < Arenas.Count; ++i)
			Alert((Arena)Arenas[i], alerts);
	}

	public void Alert(Arena arena, params string[] alerts)
	{
		if (arena is {Announcer: { }})
		{
			for (int j = 0; j < alerts.Length; ++j)
				Timer.DelayCall(TimeSpan.FromSeconds(Math.Max(j - 0.5, 0.0)), new TimerStateCallback(Alert_Callback), new object[] { arena.Announcer, alerts[j] });
		}
	}

	private void Alert_Callback(object state)
	{
		object[] states = (object[])state;

		((Mobile) states[0])?.PublicOverheadMessage(MessageType.Regular, 0x35, false, (string)states[1]);
	}
}

public class TournyPyramid
{
	public ArrayList Levels { get; set; }

	public TournyPyramid()
	{
		Levels = new ArrayList();
	}

	public void AddLevel(int partsPerMatch, ArrayList participants, GroupingType groupType, TournyType tournyType)
	{
		ArrayList copy = new(participants);

		if (groupType is GroupingType.Nearest or GroupingType.HighVsLow)
			copy.Sort();

		PyramidLevel level = new();

		switch (tournyType)
		{
			case TournyType.RedVsBlue:
			{
				TournyParticipant[] parts = new TournyParticipant[2];

				for (int i = 0; i < parts.Length; ++i)
					parts[i] = new TournyParticipant(new ArrayList());

				for (int i = 0; i < copy.Count; ++i)
				{
					ArrayList players = ((TournyParticipant)copy[i])?.Players;

					if (players != null)
						for (int j = 0; j < players.Count; ++j)
						{
							Mobile mob = (Mobile) players[j];

							if (mob is {Murderer: true})
								parts[0].Players.Add(mob);
							else
								parts[1].Players.Add(mob);
						}
				}

				level.Matches.Add(new TournyMatch(new ArrayList(parts)));
				break;
			}
			case TournyType.Faction:
			{
				TournyParticipant[] parts = new TournyParticipant[partsPerMatch];

				for (int i = 0; i < parts.Length; ++i)
					parts[i] = new TournyParticipant(new ArrayList());

				for (int i = 0; i < copy.Count; ++i)
				{
					ArrayList players = ((TournyParticipant)copy[i])?.Players;

					if (players != null)
						for (int j = 0; j < players.Count; ++j)
						{
							Mobile mob = (Mobile) players[j];

							int index = -1;

							if (partsPerMatch == 4)
							{
								Faction fac = Faction.Find(mob);

								if (fac != null)
								{
									index = fac.Definition.Sort;
								}
							}
							else if (partsPerMatch == 2)
							{
								if (Ethic.Evil.IsEligible(mob))
								{
									index = 0;
								}
								else if (Ethic.Hero.IsEligible(mob))
								{
									index = 1;
								}
							}

							if (index < 0 || index >= partsPerMatch)
							{
								index = i % partsPerMatch;
							}

							parts[index].Players.Add(mob);
						}
				}

				level.Matches.Add(new TournyMatch(new ArrayList(parts)));
				break;
			}
			case TournyType.RandomTeam:
			{
				TournyParticipant[] parts = new TournyParticipant[partsPerMatch];

				for (int i = 0; i < partsPerMatch; ++i)
					parts[i] = new TournyParticipant(new ArrayList());

				for (int i = 0; i < copy.Count; ++i)
				{
					var arrayList = ((TournyParticipant) copy[i])?.Players;
					if (arrayList != null)
						parts[i % parts.Length].Players.AddRange(arrayList);
				}

				level.Matches.Add(new TournyMatch(new ArrayList(parts)));
				break;
			}
			case TournyType.FreeForAll:
			{
				level.Matches.Add(new TournyMatch(copy));
				break;
			}
			case TournyType.Standard:
			{
				if (partsPerMatch >= 2 && participants.Count % partsPerMatch == 1)
				{
					int lowAdvances = (from TournyParticipant p in participants select p.FreeAdvances).Prepend(int.MaxValue).Min();

					ArrayList toAdvance = new();

					for (int i = 0; i < participants.Count; ++i)
					{
						TournyParticipant p = (TournyParticipant)participants[i];

						if (p.FreeAdvances == lowAdvances)
							toAdvance.Add(p);
					}

					if (toAdvance.Count == 0)
						toAdvance = copy; // sanity

					int idx = Utility.Random(toAdvance.Count);

					((TournyParticipant)toAdvance[idx])?.AddLog("Advanced automatically due to an odd number of challengers.");
					level.FreeAdvance = (TournyParticipant)toAdvance[idx];
					if (level.FreeAdvance != null) ++level.FreeAdvance.FreeAdvances;
					copy.Remove(toAdvance[idx]);
				}

				while (copy.Count >= partsPerMatch)
				{
					ArrayList thisMatch = new();

					for (int i = 0; i < partsPerMatch; ++i)
					{
						int idx = groupType switch
						{
							GroupingType.HighVsLow => i * (copy.Count - 1) / (partsPerMatch - 1),
							GroupingType.Nearest => 0,
							GroupingType.Random => Utility.Random(copy.Count),
							_ => 0
						};

						thisMatch.Add(copy[idx]);
						copy.RemoveAt(idx);
					}

					level.Matches.Add(new TournyMatch(thisMatch));
				}

				if (copy.Count > 1)
					level.Matches.Add(new TournyMatch(copy));

				break;
			}
		}

		Levels.Add(level);
	}
}

public class PyramidLevel
{
	public ArrayList Matches { get; set; }
	public TournyParticipant FreeAdvance { get; set; }

	public PyramidLevel()
	{
		Matches = new ArrayList();
	}
}

public class TournyMatch
{
	public ArrayList Participants { get; set; }
	public TournyParticipant Winner { get; set; }
	public DuelContext Context { get; set; }

	public bool InProgress => Context is {Registered: true};

	public void Start(Arena arena, Tournament tourny)
	{
		TournyParticipant first = (TournyParticipant)Participants[0];

		if (first != null)
		{
			DuelContext dc = new((Mobile)first.Players[0], tourny.Ruleset.Layout, false);
			dc.Ruleset.Options.SetAll(false);
			dc.Ruleset.Options.Or(tourny.Ruleset.Options);

			for (int i = 0; i < Participants.Count; ++i)
			{
				TournyParticipant tournyPart = (TournyParticipant)Participants[i];
				if (tournyPart != null)
				{
					Participant duelPart = new(dc, tournyPart.Players.Count)
					{
						TournyPart = tournyPart
					};

					for (int j = 0; j < tournyPart.Players.Count; ++j)
						duelPart.Add((Mobile)tournyPart.Players[j]);

					for (int j = 0; j < duelPart.Players.Length; ++j)
					{
						if (duelPart.Players[j] != null)
							duelPart.Players[j].Ready = true;
					}

					dc.Participants.Add(duelPart);
				}
			}

			if (tourny.EventController != null)
				dc.EventGame = tourny.EventController.Construct(dc);

			dc._Tournament = tourny;
			dc.Match = this;

			dc.OverrideArena = arena;

			if (tourny.SuddenDeath > TimeSpan.Zero && (tourny.SuddenDeathRounds == 0 || tourny.Pyramid.Levels.Count <= tourny.SuddenDeathRounds))
				dc.StartSuddenDeath(tourny.SuddenDeath);

			dc.SendReadyGump(0);

			if (dc.StartedBeginCountdown)
			{
				Context = dc;

				for (int i = 0; i < Participants.Count; ++i)
				{
					TournyParticipant p = (TournyParticipant)Participants[i];

					if (p != null)
						for (int j = 0; j < p.Players.Count; ++j)
						{
							Mobile mob = (Mobile) p.Players[j];

							if (mob != null)
							{
								foreach (Mobile view in mob.GetMobilesInRange(Map.GlobalUpdateRange))
								{
									if (!mob.CanSee(view))
										mob.Send(view.RemovePacket);
								}

								mob.LocalOverheadMessage(MessageType.Emote, 0x3B2, false,
									"* Your mind focuses intently on the fight and all other distractions fade away *");
							}
						}
				}
			}
			else
			{
				dc.Unregister();
				dc.StopCountdown();
			}
		}
	}

	public TournyMatch(ArrayList participants)
	{
		Participants = participants;

		for (int i = 0; i < participants.Count; ++i)
		{
			TournyParticipant part = (TournyParticipant)participants[i];

			StringBuilder sb = new();

			sb.Append("Matched in a duel against ");

			if (participants.Count > 2)
				sb.AppendFormat("{0} other {1}: ", participants.Count - 1, part.Players.Count == 1 ? "players" : "teams");

			bool hasAppended = false;

			for (int j = 0; j < participants.Count; ++j)
			{
				if (i == j)
					continue;

				if (hasAppended)
					sb.Append(", ");

				sb.Append(((TournyParticipant)participants[j]).NameList);
				hasAppended = true;
			}

			_ = sb.Append('.');

			part.AddLog(sb.ToString());
		}
	}
}

public class TournyParticipant : IComparable
{
	public ArrayList Players { get; set; }
	public ArrayList Log { get; set; }
	public int FreeAdvances { get; set; }

	public int TotalLadderXp
	{
		get
		{
			Ladder ladder = Ladder.Instance;

			if (ladder == null)
				return 0;

			return (from Mobile mob in Players select ladder.Find(mob) into entry where entry != null select entry.Experience).Sum();
		}
	}

	public string NameList
	{
		get
		{
			StringBuilder sb = new();

			for (int i = 0; i < Players.Count; ++i)
			{
				if (Players[i] == null)
					continue;

				Mobile mob = (Mobile)Players[i];

				if (sb.Length > 0)
				{
					if (Players.Count == 2)
						sb.Append(" and ");
					else if (i + 1 < Players.Count)
						sb.Append(", ");
					else
						sb.Append(", and ");
				}

				sb.Append(mob.Name);
			}

			return sb.Length == 0 ? "Empty" : sb.ToString();
		}
	}

	public void AddLog(string text)
	{
		Log.Add(text);
	}

	public void AddLog(string format, params object[] args)
	{
		AddLog(string.Format(format, args));
	}

	public void WonMatch(TournyMatch match)
	{
		AddLog("Match won.");
	}

	public void LostMatch(TournyMatch match)
	{
		AddLog("Match lost.");
	}

	public TournyParticipant(IEntity owner)
	{
		Log = new ArrayList();
		Players = new ArrayList
		{
			owner
		};
	}

	public TournyParticipant(ArrayList players)
	{
		Log = new ArrayList();
		Players = players;
	}

	public int CompareTo(object obj)
	{
		TournyParticipant p = (TournyParticipant)obj;

		return p.TotalLadderXp - TotalLadderXp;
	}
}

public enum TournyBracketGumpType
{
	Index,
	RulesInfo,
	ParticipantList,
	ParticipantInfo,
	RoundList,
	RoundInfo,
	MatchInfo,
	PlayerInfo
}

public class TournamentBracketGump : Gump
{
	private readonly Mobile _from;
	private readonly Tournament _tournament;
	private readonly TournyBracketGumpType _type;
	private readonly ArrayList _list;
	private readonly int _page;
	private int _perPage;
	private readonly object _object;

	private const int BlackColor32 = 0x000008;
	//private const int LabelColor32 = 0xFFFFFF;

	public void AddRightArrow(int x, int y, int bid, string text)
	{
		AddButton(x, y, 0x15E1, 0x15E5, bid, GumpButtonType.Reply, 0);

		if (text != null)
			AddHtml(x + 20, y - 1, 230, 20, text, false, false);
	}

	public void AddRightArrow(int x, int y, int bid)
	{
		AddRightArrow(x, y, bid, null);
	}

	public void AddLeftArrow(int x, int y, int bid, string text)
	{
		AddButton(x, y, 0x15E3, 0x15E7, bid, GumpButtonType.Reply, 0);

		if (text != null)
			AddHtml(x + 20, y - 1, 230, 20, text, false, false);
	}

	public void AddLeftArrow(int x, int y, int bid)
	{
		AddLeftArrow(x, y, bid, null);
	}

	public static int ToButtonId(int type, int index)
	{
		return 1 + index * 7 + type;
	}

	public static bool FromButtonId(int bid, out int type, out int index)
	{
		type = (bid - 1) % 7;
		index = (bid - 1) / 7;
		return bid >= 1;
	}

	public void StartPage(out int index, out int count, out int y, int perPage)
	{
		_perPage = perPage;

		index = Math.Max(_page * perPage, 0);
		count = Math.Max(Math.Min(_list.Count - index, perPage), 0);

		y = 53 + (12 - perPage) * 18;

		if (_page > 0)
			AddLeftArrow(242, 35, ToButtonId(1, 0));

		if ((_page + 1) * perPage < _list.Count)
			AddRightArrow(260, 35, ToButtonId(1, 1));
	}

	public TournamentBracketGump(Mobile from, Tournament tourny, TournyBracketGumpType type, ArrayList list, int page, object obj) : base(50, 50)
	{
		_from = from;
		_tournament = tourny;
		_type = type;
		_list = list;
		_page = page;
		_object = obj;
		_perPage = 12;

		switch (type)
		{
			case TournyBracketGumpType.Index:
			{
				AddPage(0);
				AddBackground(0, 0, 300, 300, 9380);

				StringBuilder sb = new();

				switch (tourny.TournyType)
				{
					case TournyType.FreeForAll:
						sb.Append("FFA");
						break;
					case TournyType.RandomTeam:
						sb.Append(tourny.ParticipantsPerMatch);
						sb.Append("-Team");
						break;
					case TournyType.RedVsBlue:
						sb.Append("Red v Blue");
						break;
					case TournyType.Faction:
						sb.Append(tourny.ParticipantsPerMatch);
						sb.Append("-Team Faction");
						break;
					default:
					{
						for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
						{
							if (sb.Length > 0)
								sb.Append('v');

							sb.Append(tourny.PlayersPerParticipant);
						}

						break;
					}
				}

				if (tourny.EventController != null)
					sb.Append(' ').Append(tourny.EventController.Title);

				sb.Append(" Tournament Bracket");

				AddHtml(25, 35, 250, 20, Center(sb.ToString()), false, false);

				AddRightArrow(25, 53, ToButtonId(0, 4), "Rules");
				AddRightArrow(25, 71, ToButtonId(0, 1), "Participants");

				if (_tournament.Stage == TournamentStage.Signup)
				{
					TimeSpan until = _tournament.SignupStart + _tournament.SignupPeriod - DateTime.UtcNow;
					string text;
					int secs = (int)until.TotalSeconds;

					if (secs > 0)
					{
						int mins = secs / 60;
						secs %= 60;

						switch (mins)
						{
							case > 0 when secs > 0:
								text =
									$"The tournament will begin in {mins} minute{(mins == 1 ? "" : "s")} and {secs} second{(secs == 1 ? "" : "s")}.";
								break;
							case > 0:
								text = $"The tournament will begin in {mins} minute{(mins == 1 ? "" : "s")}.";
								break;
							default:
							{
								text = secs > 0 ? $"The tournament will begin in {secs} second{(secs == 1 ? "" : "s")}." : "The tournament will begin shortly.";
								break;
							}
						}
					}
					else
					{
						text = "The tournament will begin shortly.";
					}

					AddHtml(25, 92, 250, 40, text, false, false);
				}
				else
				{
					AddRightArrow(25, 89, ToButtonId(0, 2), "Rounds");
				}

				break;
			}
			case TournyBracketGumpType.RulesInfo:
			{
				Ruleset ruleset = tourny.Ruleset;
				Ruleset basedef = ruleset.Base;

				BitArray defs;

				if (ruleset.Flavors.Count > 0)
				{
					defs = new BitArray(basedef.Options);

					for (int i = 0; i < ruleset.Flavors.Count; ++i)
						defs.Or(((Ruleset)ruleset.Flavors[i]).Options);
				}
				else
				{
					defs = basedef.Options;
				}

				int changes = 0;

				BitArray opts = ruleset.Options;

				for (int i = 0; i < opts.Length; ++i)
				{
					if (defs[i] != opts[i])
						++changes;
				}

				AddPage(0);
				AddBackground(0, 0, 300, 60 + 18 + 20 + 20 + 20 + 8 + 20 + ruleset.Flavors.Count * 18 + 4 + 20 + changes * 22 + 6, 9380);

				AddLeftArrow(25, 11, ToButtonId(0, 0));
				AddHtml(25, 35, 250, 20, Center("Rules"), false, false);

				int y = 53;

				string groupText = tourny.GroupType switch
				{
					GroupingType.HighVsLow => "High vs Low",
					GroupingType.Nearest => "Closest opponent",
					GroupingType.Random => "Random",
					_ => null
				};

				AddHtml(35, y, 190, 20, $"Grouping: {groupText}", false, false);
				y += 20;

				string tieText = tourny.TieType switch
				{
					TieType.Random => "Random",
					TieType.Highest => "Highest advances",
					TieType.Lowest => "Lowest advances",
					TieType.FullAdvancement => tourny.ParticipantsPerMatch == 2 ? "Both advance" : "Everyone advances",
					TieType.FullElimination => tourny.ParticipantsPerMatch == 2
						? "Both eliminated"
						: "Everyone eliminated",
					_ => null
				};

				AddHtml(35, y, 190, 20, $"Tiebreaker: {tieText}", false, false);
				y += 20;

				string sdText = "Off";

				if (tourny.SuddenDeath > TimeSpan.Zero)
				{
					sdText = $"{(int) tourny.SuddenDeath.TotalMinutes}:{tourny.SuddenDeath.Seconds:D2}";

					sdText = tourny.SuddenDeathRounds > 0 ? $"{sdText} (first {tourny.SuddenDeathRounds} rounds)" : $"{sdText} (all rounds)";
				}

				AddHtml(35, y, 240, 20, $"Sudden Death: {sdText}", false, false);
				y += 20;

				y += 8;

				AddHtml(35, y, 190, 20, $"Ruleset: {basedef.Title}", false, false);
				y += 20;

				for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
					AddHtml(35, y, 190, 20, $" + {((Ruleset) ruleset.Flavors[i]).Title}", false, false);

				y += 4;

				if (changes > 0)
				{
					AddHtml(35, y, 190, 20, "Modifications:", false, false);
					y += 20;

					for (int i = 0; i < opts.Length; ++i)
					{
						if (defs[i] != opts[i])
						{
							string name = ruleset.Layout.FindByIndex(i);

							if (name != null) // sanity
							{
								AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
								AddHtml(60, y, 165, 22, name, false, false);
							}

							y += 22;
						}
					}
				}
				else
				{
					AddHtml(35, y, 190, 20, "Modifications: None", false, false);
				}

				break;
			}
			case TournyBracketGumpType.ParticipantList:
			{
				AddPage(0);
				AddBackground(0, 0, 300, 300, 9380);

				_list ??= new ArrayList(tourny.Participants);

				AddLeftArrow(25, 11, ToButtonId(0, 0));
				AddHtml(25, 35, 250, 20, Center($"{_list.Count} Participant{(_list.Count == 1 ? "" : "s")}"), false, false);

				StartPage(out int index, out int count, out int y, 12);

				for (int i = 0; i < count; ++i, y += 18)
				{
					TournyParticipant part = (TournyParticipant)_list[index + i];
					string name = part.NameList;

					if (_tournament.TournyType != TournyType.Standard && part.Players.Count == 1)
					{
						if (part.Players[0] is PlayerMobile {DuelPlayer: { }} pm)
							name = Color(name, pm.DuelPlayer.Eliminated ? 0x6633333 : 0x336666);
					}

					AddRightArrow(25, y, ToButtonId(2, index + i), name);
				}

				break;
			}
			case TournyBracketGumpType.ParticipantInfo:
			{
				if (obj is not TournyParticipant part)
					break;

				AddPage(0);
				AddBackground(0, 0, 300, 60 + 18 + 20 + part.Players.Count * 18 + 20 + 20 + 160, 9380);

				AddLeftArrow(25, 11, ToButtonId(0, 1));
				AddHtml(25, 35, 250, 20, Center("Participants"), false, false);

				int y = 53;

				AddHtml(25, y, 200, 20, part.Players.Count == 1 ? "Players" : "Team", false, false);
				y += 20;

				for (int i = 0; i < part.Players.Count; ++i)
				{
					Mobile mob = (Mobile)part.Players[i];
					string name = mob.Name;

					if (_tournament.TournyType != TournyType.Standard)
					{
						if (mob is PlayerMobile {DuelPlayer: { }} pm)
							name = Color(name, pm.DuelPlayer.Eliminated ? 0x6633333 : 0x336666);
					}

					AddRightArrow(35, y, ToButtonId(4, i), name);
					y += 18;
				}

				AddHtml(25, y, 200, 20,
					$"Free Advances: {(part.FreeAdvances == 0 ? "None" : part.FreeAdvances.ToString())}", false, false);
				y += 20;

				AddHtml(25, y, 200, 20, "Log:", false, false);
				y += 20;

				StringBuilder sb = new();

				for (int i = 0; i < part.Log.Count; ++i)
				{
					if (sb.Length > 0)
						sb.Append("<br>");

					sb.Append(part.Log[i]);
				}

				if (sb.Length == 0)
					sb.Append("Nothing logged yet.");

				AddHtml(25, y, 250, 150, Color(sb.ToString(), BlackColor32), false, true);

				break;
			}
			case TournyBracketGumpType.PlayerInfo:
			{
				AddPage(0);
				AddBackground(0, 0, 300, 300, 9380);

				AddLeftArrow(25, 11, ToButtonId(0, 3));
				AddHtml(25, 35, 250, 20, Center("Participants"), false, false);


				if (obj is not Mobile mob)
					break;
				Ladder ladder = Ladder.Instance;
				LadderEntry entry = ladder?.Find(mob);

				AddHtml(25, 53, 250, 20, $"Name: {mob.Name}", false, false);
				AddHtml(25, 73, 250, 20,
					$"Guild: {(mob.Guild == null ? "None" : mob.Guild.Name + " [" + mob.Guild.Abbreviation + "]")}", false, false);
				AddHtml(25, 93, 250, 20, $"Rank: {(entry == null ? "N/A" : LadderGump.Rank(entry.Index + 1))}", false, false);
				AddHtml(25, 113, 250, 20, $"Level: {(entry == null ? 0 : Ladder.GetLevel(entry.Experience))}", false, false);
				AddHtml(25, 133, 250, 20, $"Wins: {entry?.Wins ?? 0:N0}", false, false);
				AddHtml(25, 153, 250, 20, $"Losses: {entry?.Losses ?? 0:N0}", false, false);

				break;
			}
			case TournyBracketGumpType.RoundList:
			{
				AddPage(0);
				AddBackground(0, 0, 300, 300, 9380);

				AddLeftArrow(25, 11, ToButtonId(0, 0));
				AddHtml(25, 35, 250, 20, Center("Rounds"), false, false);

				_list ??= new ArrayList(tourny.Pyramid.Levels);

				StartPage(out int index, out int count, out int y, 12);

				for (int i = 0; i < count; ++i, y += 18)
				{
					_ = (PyramidLevel)_list[index + i];

					AddRightArrow(25, y, ToButtonId(3, index + i), "Round #" + (index + i + 1));
				}

				break;
			}
			case TournyBracketGumpType.RoundInfo:
			{
				AddPage(0);
				AddBackground(0, 0, 300, 300, 9380);

				AddLeftArrow(25, 11, ToButtonId(0, 2));
				AddHtml(25, 35, 250, 20, Center("Rounds"), false, false);

				if (_object is not PyramidLevel level)
					break;

				_list ??= new ArrayList(level.Matches);

				AddRightArrow(25, 53, ToButtonId(5, 0),
					$"Free Advance: {(level.FreeAdvance == null ? "None" : level.FreeAdvance.NameList)}");

				AddHtml(25, 73, 200, 20, $"{_list.Count} Match{(_list.Count == 1 ? "" : "es")}", false, false);

				StartPage(out int index, out int count, out int y, 10);

				for (int i = 0; i < count; ++i, y += 18)
				{
					TournyMatch match = (TournyMatch)_list[index + i];

					int color = -1;

					if (match.InProgress)
						color = 0x336666;
					else if (match.Context != null && match.Winner == null)
						color = 0x666666;

					StringBuilder sb = new();

					if (_tournament.TournyType == TournyType.Standard)
					{
						for (int j = 0; j < match.Participants.Count; ++j)
						{
							if (sb.Length > 0)
								sb.Append(" vs ");

							TournyParticipant part = (TournyParticipant)match.Participants[j];
							string txt = part.NameList;

							txt = color switch
							{
								-1 when match.Context != null && match.Winner == part => Color(txt, 0x336633),
								-1 when match.Context != null => Color(txt, 0x663333),
								_ => txt
							};

							sb.Append(txt);
						}
					}
					else if (_tournament.EventController != null || _tournament.TournyType == TournyType.RandomTeam || _tournament.TournyType == TournyType.RedVsBlue || _tournament.TournyType == TournyType.Faction)
					{
						for (int j = 0; j < match.Participants.Count; ++j)
						{
							if (sb.Length > 0)
								sb.Append(" vs ");

							TournyParticipant part = (TournyParticipant)match.Participants[j];
							string txt;

							if (_tournament.EventController != null)
								txt = $"Team {_tournament.EventController.GetTeamName(j)} ({part.Players.Count})";
							else if (_tournament.TournyType == TournyType.RandomTeam)
								txt = $"Team {j + 1} ({part.Players.Count})";
							else if (_tournament.TournyType == TournyType.Faction)
							{
								switch (_tournament.ParticipantsPerMatch)
								{
									case 4:
									{
										string name = "(null)";

										switch (j)
										{
											case 0:
											{
												name = "Minax";
												break;
											}
											case 1:
											{
												name = "Council of Mages";
												break;
											}
											case 2:
											{
												name = "True Britannians";
												break;
											}
											case 3:
											{
												name = "Shadowlords";
												break;
											}
										}

										txt = $"{name} ({part.Players.Count})";
										break;
									}
									case 2:
										txt = $"{(j == 0 ? "Evil" : "Hero")} Team ({part.Players.Count})";
										break;
									default:
										txt = $"Team {j + 1} ({part.Players.Count})";
										break;
								}
							}
							else
								txt = $"Team {(j == 0 ? "Red" : "Blue")} ({part.Players.Count})";

							txt = color switch
							{
								-1 when match.Context != null && match.Winner == part => Color(txt, 0x336633),
								-1 when match.Context != null => Color(txt, 0x663333),
								_ => txt
							};

							sb.Append(txt);
						}
					}
					else if (_tournament.TournyType == TournyType.FreeForAll)
					{
						sb.Append("Free For All");
					}

					string str = sb.ToString();

					if (color >= 0)
						str = Color(str, color);

					AddRightArrow(25, y, ToButtonId(5, index + i + 1), str);
				}

				break;
			}
			case TournyBracketGumpType.MatchInfo:
			{
				if (obj is not TournyMatch match)
					break;

				int ct = _tournament.TournyType == TournyType.FreeForAll ? 2 : match.Participants.Count;

				AddPage(0);
				AddBackground(0, 0, 300, 60 + 18 + 20 + 20 + 20 + ct * 18 + 6, 9380);

				AddLeftArrow(25, 11, ToButtonId(0, 5));
				AddHtml(25, 35, 250, 20, Center("Rounds"), false, false);

				AddHtml(25, 53, 250, 20, $"Winner: {(match.Winner == null ? "N/A" : match.Winner.NameList)}", false, false);
				AddHtml(25, 73, 250, 20,
					$"State: {(match.InProgress ? "In progress" : match.Context != null ? "Complete" : "Waiting")}", false, false);
				AddHtml(25, 93, 250, 20, "Participants:", false, false);

				if (_tournament.TournyType == TournyType.Standard)
				{
					for (int i = 0; i < match.Participants.Count; ++i)
					{
						TournyParticipant part = (TournyParticipant)match.Participants[i];

						AddRightArrow(25, 113 + i * 18, ToButtonId(6, i), part.NameList);
					}
				}
				else if (_tournament.EventController != null || _tournament.TournyType == TournyType.RandomTeam || _tournament.TournyType == TournyType.RedVsBlue || _tournament.TournyType == TournyType.Faction)
				{
					for (int i = 0; i < match.Participants.Count; ++i)
					{
						TournyParticipant part = (TournyParticipant)match.Participants[i];

						if (_tournament.EventController != null)
							AddRightArrow(25, 113 + i * 18, ToButtonId(6, i),
								$"Team {_tournament.EventController.GetTeamName(i)} ({part.Players.Count})");
						else if (_tournament.TournyType == TournyType.RandomTeam)
							AddRightArrow(25, 113 + i * 18, ToButtonId(6, i), $"Team {i + 1} ({part.Players.Count})");
						else if (_tournament.TournyType == TournyType.Faction)
						{
							switch (_tournament.ParticipantsPerMatch)
							{
								case 4:
								{
									string name = "(null)";

									switch (i)
									{
										case 0:
										{
											name = "Minax";
											break;
										}
										case 1:
										{
											name = "Council of Mages";
											break;
										}
										case 2:
										{
											name = "True Britannians";
											break;
										}
										case 3:
										{
											name = "Shadowlords";
											break;
										}
									}

									AddRightArrow(25, 113 + i * 18, ToButtonId(6, i), $"{name} ({part.Players.Count})");
									break;
								}
								case 2:
									AddRightArrow(25, 113 + i * 18, ToButtonId(6, i),
										$"{(i == 0 ? "Evil" : "Hero")} Team ({part.Players.Count})");
									break;
								default:
									AddRightArrow(25, 113 + i * 18, ToButtonId(6, i),
										$"Team {i + 1} ({part.Players.Count})");
									break;
							}
						}
						else
							AddRightArrow(25, 113 + i * 18, ToButtonId(6, i),
								$"Team {(i == 0 ? "Red" : "Blue")} ({part.Players.Count})");
					}
				}
				else if (_tournament.TournyType == TournyType.FreeForAll)
				{
					AddHtml(25, 113, 250, 20, "Free For All", false, false);
				}

				break;
			}
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{

		if (!FromButtonId(info.ButtonID, out int type, out int index))
			return;

		switch (type)
		{
			case 0:
			{
				switch (index)
				{
					case 0: _from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.Index, null, 0, null)); break;
					case 1: _from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.ParticipantList, null, 0, null)); break;
					case 2: _from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.RoundList, null, 0, null)); break;
					case 4: _from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.RulesInfo, null, 0, null)); break;
					case 3:
					{
						Mobile mob = _object as Mobile;

						for (int i = 0; i < _tournament.Participants.Count; ++i)
						{
							TournyParticipant part = (TournyParticipant)_tournament.Participants[i];

							if (part != null && part.Players.Contains(mob))
							{
								_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.ParticipantInfo, null, 0, part));
								break;
							}
						}

						break;
					}
					case 5:
					{
						if (_object is not TournyMatch match)
							break;

						for (int i = 0; i < _tournament.Pyramid.Levels.Count; ++i)
						{
							PyramidLevel level = (PyramidLevel)_tournament.Pyramid.Levels[i];

							if (level != null && level.Matches.Contains(match))
								_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.RoundInfo, null, 0, level));
						}

						break;
					}
				}

				break;
			}
			case 1:
			{
				switch (index)
				{
					case 0:
					{
						if (_list != null && _page > 0)
							_from.SendGump(new TournamentBracketGump(_from, _tournament, _type, _list, _page - 1, _object));

						break;
					}
					case 1:
					{
						if (_list != null && (_page + 1) * _perPage < _list.Count)
							_from.SendGump(new TournamentBracketGump(_from, _tournament, _type, _list, _page + 1, _object));

						break;
					}
				}

				break;
			}
			case 2:
			{
				if (_type != TournyBracketGumpType.ParticipantList)
					break;

				if (index >= 0 && index < _list.Count)
					_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.ParticipantInfo, null, 0, _list[index]));

				break;
			}
			case 3:
			{
				if (_type != TournyBracketGumpType.RoundList)
					break;

				if (index >= 0 && index < _list.Count)
					_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.RoundInfo, null, 0, _list[index]));

				break;
			}
			case 4:
			{
				if (_type != TournyBracketGumpType.ParticipantInfo)
					break;

				if (_object is TournyParticipant part && index >= 0 && index < part.Players.Count)
					_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.PlayerInfo, null, 0, part.Players[index]));

				break;
			}
			case 5:
			{
				if (_type != TournyBracketGumpType.RoundInfo)
					break;

				if (_object is not PyramidLevel level)
					break;

				switch (index)
				{
					case 0:
						_from.SendGump(level.FreeAdvance != null
							? new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.ParticipantInfo, null, 0,
								level.FreeAdvance)
							: new TournamentBracketGump(_from, _tournament, _type, _list, _page, _object));
						break;
					case >= 1 when index <= level.Matches.Count:
						_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.MatchInfo, null, 0, level.Matches[index - 1]));
						break;
				}

				break;
			}
			case 6:
			{
				if (_type != TournyBracketGumpType.MatchInfo)
					break;

				if (_object is TournyMatch match && index >= 0 && index < match.Participants.Count)
					_from.SendGump(new TournamentBracketGump(_from, _tournament, TournyBracketGumpType.ParticipantInfo, null, 0, match.Participants[index]));

				break;
			}
		}
	}
}

public class TournamentBracketItem : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public TournamentController Tournament { get; set; }

	public override string DefaultName => "tournament bracket";

	[Constructable]
	public TournamentBracketItem() : base(3774)
	{
		Movable = false;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 2))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
		}
		else
		{
			Tournament tourny = Tournament?.Tournament;

			if (tourny != null)
			{
				from.CloseGump(typeof(TournamentBracketGump));
				from.SendGump(new TournamentBracketGump(from, tourny, TournyBracketGumpType.Index, null, 0, null));
			}
		}
	}

	public TournamentBracketItem(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(Tournament);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		Tournament = version switch
		{
			0 => reader.ReadItem() as TournamentController,
			_ => Tournament
		};
	}
}
