using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Server.Mobiles;

namespace Server.Poker;

public class GameBackup //Provides a protection for players so that if server crashes, they will be refunded money
{
	public static List<PokerGame> PokerGames; //List of all poker games with players
}

public class PokerGame
{
	public static void Initialize()
	{
		GameBackup.PokerGames = new List<PokerGame>();
		EventSink.OnCrashed += EventSink_Crashed;
	}

	private static void EventSink_Crashed(CrashedEventArgs e)
	{
		foreach (PokerGame game in GameBackup.PokerGames)
		{
			List<PokerPlayer> toRemove = game.Players.Players.Where(player => player.Mobile != null).ToList();

			foreach (PokerPlayer player in toRemove)
			{
				player.SendMessage(0x22, "The server has crashed, and you are now being removed from the poker game and being refunded the money that you currently have.");

				game.RemovePlayer(player);
			}
		}
	}

	public bool NeedsGumpUpdate { get; set; }

	public int CommunityGold { get; set; }

	public int CurrentBet { get; set; }

	public Deck Deck { get; set; }

	public PokerGameState State { get; set; }

	public PokerDealer Dealer { get; set; }

	public PokerPlayer DealerButton { get; private set; }

	public PokerPlayer SmallBlind { get; private set; }

	public PokerPlayer BigBlind { get; private set; }

	public List<Card> CommunityCards { get; set; }

	public PokerGameTimer Timer { get; set; }

	public PlayerStructure Players { get; }

	public bool IsBettingRound => (int)State % 2 == 0;

	public PokerGame(PokerDealer dealer)
	{
		Dealer = dealer;
		NeedsGumpUpdate = false;
		CommunityCards = new List<Card>();
		State = PokerGameState.Inactive;
		Deck = new Deck();
		Timer = new PokerGameTimer(this);
		Players = new PlayerStructure(this);
	}

	public void PokerMessage(Mobile from, string message)
	{
		from.PublicOverheadMessage(Network.MessageType.Regular, 0x9A, true, message);

		for (int i = 0; i < Players.Count; ++i)
		{
			if (Players[i].Mobile != null)
			{
				Players[i].Mobile.SendMessage(0x9A, "[{0}]: {1}", from.Name, message);
			}
		}
	}

	public void PokerGame_PlayerMadeDecision(PokerPlayer player)
	{
		if (Players.Peek() == player)
		{
			if (player.Mobile == null)
			{
				return;
			}

			bool resetTurns = false;

			switch (player.Action)
			{
				case PlayerAction.None: break;
				case PlayerAction.Bet:
				{
					PokerMessage(player.Mobile, $"I bet {player.Bet}.");
					CurrentBet = player.Bet;
					player.RoundBet = player.Bet;
					player.Gold -= player.Bet;
					player.RoundGold += player.Bet;
					CommunityGold += player.Bet;
					resetTurns = true;

					break;
				}
				case PlayerAction.Raise:
				{
					PokerMessage(player.Mobile, $"I raise by {player.Bet}.");
					CurrentBet += player.Bet;
					int diff = CurrentBet - player.RoundBet;
					player.Gold -= diff;
					player.RoundGold += diff;
					player.RoundBet += diff;
					CommunityGold += diff;
					player.Bet = diff;
					resetTurns = true;

					break;
				}
				case PlayerAction.Call:
				{
					PokerMessage(player.Mobile, "I call.");

					int diff = CurrentBet - player.RoundBet; //how much they owe in the pot
					player.Bet = diff;
					player.Gold -= diff;
					player.RoundGold += diff;
					player.RoundBet += diff;
					CommunityGold += diff;

					break;
				}
				case PlayerAction.Check:
				{
					if (!player.LonePlayer)
					{
						PokerMessage(player.Mobile, "Check.");
					}

					break;
				}
				case PlayerAction.Fold:
				{
					PokerMessage(player.Mobile, "I fold.");

					if (Players.Round.Contains(player))
					{
						Players.Round.Remove(player);
					}

					if ( Players.Turn.Contains(player))
					{
						Players.Turn.Remove(player);
					}

					if (Players.Round.Count == 1)
					{
						DoShowdown(true);
						return;
					}

					break;
				}
				case PlayerAction.AllIn:
				{
					if (!player.IsAllIn)
					{
						PokerMessage(player.Mobile, player.Forced ? "I call: all-in." : "All in.");

						int diff = player.Gold - CurrentBet;

						if (diff > 0)
						{
							CurrentBet += diff;
						}

						player.Bet = player.Gold;
						player.RoundGold += player.Gold;
						player.RoundBet += player.Gold;
						CommunityGold += player.Gold;
						player.Gold = 0;

						//We need to check to see if this is a follow up action, or a first call
						//before we reset the turns
						if (Players.Prev() != null)
						{
							resetTurns = Players.Prev().Action == PlayerAction.Check;

							PokerPlayer prev = Players.Prev();

							if (prev.Action == PlayerAction.Check || prev.Action == PlayerAction.Bet && prev.Bet < player.Bet ||
							    prev.Action == PlayerAction.AllIn && prev.Bet < player.Bet || prev.Action == PlayerAction.Call && prev.Bet < player.Bet ||
							    prev.Action == PlayerAction.Raise && prev.Bet < player.Bet)
							{
								resetTurns = true;
							}
						}
						else
						{
							resetTurns = true;
						}

						player.IsAllIn = true;
						player.Forced = false;
					}

					break;
				}
			}

			if (resetTurns)
			{
				Players.Turn.Clear();
				Players.Push(player);
			}

			Timer.LastPlayer = null;
			Timer.HasWarned = false;

			if (Players.Turn.Count == Players.Round.Count)
			{
				State = (PokerGameState)((int)State + 1);
			}
			else
			{
				AssignNextTurn();
			}

			NeedsGumpUpdate = true;
		}
	}

	public void Begin()
	{
		while (true)
		{
			Players.Clear();
			CurrentBet = 0;

			List<PokerPlayer> dispose = Players.Players.Where(player => player.RequestLeave || !player.IsOnline()).ToList();

			foreach (var player in dispose.Where(player => Players.Contains(player)))
			{
				RemovePlayer(player);
			}

			foreach (PokerPlayer player in Players.Players)
			{
				player.ClearGame();
				player.Game = this;

				if (player.Gold >= Dealer.BigBlind && player.IsOnline())
				{
					Players.Round.Add(player);
				}
			}

			if (DealerButton == null) //First round / more player
			{
				switch (Players.Round.Count)
				{
					//Only use dealer button and small blind
					case 2:
						DealerButton = Players.Round[0];
						SmallBlind = Players.Round[1];
						BigBlind = null;
						break;
					case > 2:
						DealerButton = Players.Round[0];
						SmallBlind = Players.Round[1];
						BigBlind = Players.Round[2];
						break;
					default:
						return;
				}
			}
			else
			{
				switch (Players.Round.Count)
				{
					//Only use dealer button and small blind
					case 2:
					{
						if (DealerButton == Players.Round[0])
						{
							DealerButton = Players.Round[1];
							SmallBlind = Players.Round[0];
						}
						else
						{
							DealerButton = Players.Round[0];
							SmallBlind = Players.Round[1];
						}

						BigBlind = null;
						break;
					}
					case > 2:
					{
						int index = Players.Round.IndexOf(DealerButton);

						if (index == -1) //Old dealer button was lost :(
						{
							DealerButton = null;
							continue;
						}

						if (index == Players.Round.Count - 1)
						{
							DealerButton = Players.Round[0];
							SmallBlind = Players.Round[1];
							BigBlind = Players.Round[2];
						}
						else if (index == Players.Round.Count - 2)
						{
							DealerButton = Players.Round[^1];
							SmallBlind = Players.Round[0];
							BigBlind = Players.Round[1];
						}
						else if (index == Players.Round.Count - 3)
						{
							DealerButton = Players.Round[^2];
							SmallBlind = Players.Round[^1];
							BigBlind = Players.Round[0];
						}
						else
						{
							DealerButton = Players.Round[index + 1];
							SmallBlind = Players.Round[index + 2];
							BigBlind = Players.Round[index + 3];
						}

						break;
					}
					default:
						return;
				}
			}

			CommunityCards.Clear();
			Deck = new Deck();

			State = PokerGameState.DealHoleCards;

			if (BigBlind != null)
			{
				BigBlind.Gold -= Dealer.BigBlind;
				CommunityGold += Dealer.BigBlind;
				BigBlind.RoundGold = Dealer.BigBlind;
				BigBlind.RoundBet = Dealer.BigBlind;
				BigBlind.Bet = Dealer.BigBlind;
			}

			SmallBlind.Gold -= BigBlind == null ? Dealer.BigBlind : Dealer.SmallBlind;
			CommunityGold += BigBlind == null ? Dealer.BigBlind : Dealer.SmallBlind;
			SmallBlind.RoundGold = BigBlind == null ? Dealer.BigBlind : Dealer.SmallBlind;
			SmallBlind.RoundBet = BigBlind == null ? Dealer.BigBlind : Dealer.SmallBlind;
			SmallBlind.Bet = BigBlind == null ? Dealer.BigBlind : Dealer.SmallBlind;

			if (BigBlind != null)
			{
				//m_Players.Push(m_BigBlind);
				BigBlind.SetBbAction();
				CurrentBet = Dealer.BigBlind;
			}
			else
			{
				//m_Players.Push(m_SmallBlind);
				SmallBlind.SetBbAction();
				CurrentBet = Dealer.BigBlind;
			}

			if (Players.Next() == null)
			{
				return;
			}

			NeedsGumpUpdate = true;
			Timer = new PokerGameTimer(this);
			Timer.Start();
			break;
		}
	}

	public void End()
	{
		State = PokerGameState.Inactive;

		foreach (PokerPlayer player in Players.Players)
		{
			player.CloseGump(typeof(PokerTableGump));
			player.SendGump(new PokerTableGump(this, player));
		}

		if (Timer.Running)
		{
			Timer.Stop();
		}
	}

	public void DealHoleCards()
	{
		for (int i = 0; i < 2; ++i) //Simulate passing one card out at a time, going around the circle of players 2 times
		{
			foreach (PokerPlayer player in Players.Round)
			{
				player.AddCard(Deck.Pop());
			}
		}
	}

	public PokerPlayer AssignNextTurn()
	{
		PokerPlayer nextTurn = Players.Next();

		if (nextTurn == null)
		{
			return null;
		}

		if (nextTurn.RequestLeave)
		{
			Players.Push(nextTurn);
			nextTurn.BetStart = DateTime.Now;
			nextTurn.Action = PlayerAction.Fold;
			return nextTurn;
		}

		if (nextTurn.IsAllIn)
		{
			Players.Push(nextTurn);
			nextTurn.BetStart = DateTime.Now;
			nextTurn.Action = PlayerAction.AllIn;
			return nextTurn;
		}

		if (nextTurn.LonePlayer)
		{
			Players.Push(nextTurn);
			nextTurn.BetStart = DateTime.Now;
			nextTurn.Action = PlayerAction.Check;
			return nextTurn;
		}

		bool canCall = false;

		PokerPlayer currentTurn = Players.Peek();

		if (currentTurn != null && currentTurn.Action != PlayerAction.Check && currentTurn.Action != PlayerAction.Fold)
		{
			canCall = true;
		}

		if (currentTurn == null && State == PokerGameState.PreFlop)
		{
			canCall = true;
		}

		Players.Push(nextTurn);
		nextTurn.BetStart = DateTime.Now;

		ResultEntry entry = new(nextTurn)
		{
			Rank = nextTurn.GetBestHand(CommunityCards, out var bestCards),
			BestCards = bestCards
		};

		nextTurn.SendMessage(0x22, $"You have {HandRanker.RankString(entry)}.");
		nextTurn.CloseGump(typeof(PokerBetGump));
		nextTurn.SendGump(new PokerBetGump(this, nextTurn, canCall));

		NeedsGumpUpdate = true;

		return nextTurn;
	}

	public List<PokerPlayer> GetWinners(bool silent)
	{
		List<ResultEntry> results = new();

		for (int i = 0; i < Players.Round.Count; ++i)
		{
			ResultEntry entry = new(Players.Round[i]);

			entry.Rank = HandRanker.GetBestHand(entry.Player.GetAllCards(CommunityCards), out var bestCards);
			entry.BestCards = bestCards;

			results.Add(entry);

			/*if ( !silent )
			{
				//Check if kickers needed
				PokerMessage( entry.Player.Mobile, String.Format( "I have {0}.", HandRanker.RankString( entry ) ) );
			}*/
		}

		results.Sort();

		if (results.Count < 1)
		{
			return null;
		}

		List<PokerPlayer> winners = new();

		for (int i = 0; i < results.Count; ++i)
		{
			if (HandRanker.IsBetterThan(results[i], results[0]) == RankResult.Same)
			{
				winners.Add(results[i].Player);
			}
		}

		//IF NOT SILENT
		if (!silent)
		{
			//Only hands that have made it past the showdown may be considered for the jackpot
			for (int i = 0; i < results.Count; ++i)
			{
				if (winners.Contains(results[i].Player))
				{
					if (PokerDealer.JackpotWinners != null)
					{
						if (HandRanker.IsBetterThan(results[i], PokerDealer.JackpotWinners.Hand) == RankResult.Better)
						{
							PokerDealer.JackpotWinners = null;
							PokerDealer.JackpotWinners = new PokerDealer.JackpotInfo(winners, results[i], DateTime.Now);

							break;
						}
					}
					else
					{
						PokerDealer.JackpotWinners = new PokerDealer.JackpotInfo(winners, results[i], DateTime.Now);
						break;
					}
				}
			}

			results.Reverse();

			foreach (ResultEntry entry in results)
			{
				//if ( !winners.Contains( entry.Player ) )
				PokerMessage( entry.Player.Mobile, $"I have {HandRanker.RankString(entry)}.");
				/*else
				{
					if ( !HandRanker.UsesKicker( entry.Rank ) )
						PokerMessage( entry.Player, String.Format( "I have {0}.", HandRanker.RankString( entry ) ) );
					else //Hand rank uses a kicker
					{
						switch ( entry.Rank )
						{
						}
					}
				}*/
			}
		}

		return winners;
	}

	public void AwardPotToWinners(List<PokerPlayer> winners, bool silent)
	{
		//** Casino Rake - Will take a percentage of each pot awarded and place it towards
		//**				the casino jackpot for the highest ranked hand.

		if (!silent) //Only rake pots that have made it past the showdown.
		{
			int rake = Math.Min((int)(CommunityGold * Dealer.Rake), Dealer.RakeMax);

			if (rake > 0)
			{
				CommunityGold -= rake;
				PokerDealer.Jackpot += rake;
			}
		}
            
		int lowestBet = 0;

		foreach (PokerPlayer player in winners)
		{
			if (player.RoundGold < lowestBet || lowestBet == 0)
			{
				lowestBet = player.RoundGold;
			}
		}

		foreach (PokerPlayer player in Players.Round)
		{
			int diff = player.RoundGold - lowestBet;

			if (diff > 0)
			{
				player.Gold += diff;
				CommunityGold -= diff;
				PokerMessage(Dealer, $"{diff}gp has been returned to {player.Mobile.Name}.");
			}
		}

		int splitPot = CommunityGold / winners.Count;

		foreach (PokerPlayer player in winners)
		{
			player.Gold += splitPot;
			PokerMessage(Dealer, $"{player.Mobile.Name} has won {splitPot}gp.");
		}

		CommunityGold = 0;
	}

	public void DoShowdown(bool silent)
	{
		List<PokerPlayer> winners = GetWinners(silent);

		if (winners is {Count: > 0})
		{
			AwardPotToWinners(winners, silent);
		}

		End();

		Begin();
	}

	public void DoRoundAction() //Happens once State is changed (once per state)
	{
		if (State == PokerGameState.Showdown)
		{
			DoShowdown(false);
		}
		else if (State == PokerGameState.DealHoleCards)
		{
			DealHoleCards();
			State = PokerGameState.PreFlop;
			NeedsGumpUpdate = true;
		}
		else if (!IsBettingRound)
		{
			int numberOfCards = 0;
			string round = string.Empty;

			switch (State)
			{
				case PokerGameState.Flop: numberOfCards += 3; round = "flop"; State = PokerGameState.PreTurn; break;
				case PokerGameState.Turn: ++numberOfCards; round = "turn"; State = PokerGameState.PreRiver; break;
				case PokerGameState.River: ++numberOfCards; round = "river"; State = PokerGameState.PreShowdown; break;
			}

			if (numberOfCards != 0) //Pop the appropriate number of cards from the top of the deck
			{
				StringBuilder sb = new();

				sb.Append("The " + round + " shows: ");

				for (int i = 0; i < numberOfCards; ++i)
				{
					Card popped = Deck.Pop();
					if (i == 2 || numberOfCards == 1)
					{
						sb.Append(popped.Name + ".");
					}
					else
					{
						sb.Append(popped.Name + ", ");
					}

					CommunityCards.Add(popped);
				}

				PokerMessage(Dealer, sb.ToString());
				Players.Turn.Clear();
				//AssignNextTurn();
				NeedsGumpUpdate = true;
			}
		}
		else
		{
			if (Players.Turn.Count == Players.Round.Count)
			{
				State = State switch
				{
					PokerGameState.PreFlop => PokerGameState.Flop,
					PokerGameState.PreTurn => PokerGameState.Turn,
					PokerGameState.PreRiver => PokerGameState.River,
					PokerGameState.PreShowdown => PokerGameState.Showdown,
					_ => State
				};

				//m_Players.Turn.Clear();
			}
			else switch (Players.Turn.Count)
			{
				//We need to initiate betting for this round
				case 0 when State != PokerGameState.PreFlop:
					ResetPlayerActions();
					CheckLonePlayer();
					AssignNextTurn();
					break;
				case 0 when State == PokerGameState.PreFlop:
					CheckLonePlayer();
					AssignNextTurn();
					break;
			}
		}
	}

	public void CheckLonePlayer()
	{
		int allInCount = Players.Round.Count(t => t.IsAllIn);

		PokerPlayer loner = null;

		if (allInCount == Players.Round.Count - 1)
		{
			for (int i = 0; i < Players.Round.Count; ++i)
			{
				if (!Players.Round[i].IsAllIn)
				{
					loner = Players.Round[i];
				}
			}
		}

		if (loner != null)
		{
			loner.LonePlayer = true;
		}
	}

	public void ResetPlayerActions()
	{
		for (int i = 0; i < Players.Count; ++i)
		{
			Players[i].Action = PlayerAction.None;
			Players[i].RoundBet = 0;
		}
	}

	public int GetIndexFor(Mobile from)
	{
		for (int i = 0; i < Players.Count; ++i)
		{
			if (Players[i].Mobile != null && from != null && Players[i].Mobile.Serial == from.Serial)
			{
				return i;
			}
		}

		return -1;
	}

	public PokerPlayer GetPlayer(Mobile from)
	{
		return GetIndexFor(from) == -1 ? null : Players[GetIndexFor(from)];
	}

	public int GetIndexForPlayerInRound(Mobile from)
	{
		for (int i = 0; i < Players.Round.Count; ++i)
		{
			if (Players.Round[i].Mobile != null && from != null && Players.Round[i].Mobile.Serial == from.Serial)
			{
				return i;
			}
		}

		return -1;
	}

	public void AddPlayer(PokerPlayer player)
	{
		Mobile from = player.Mobile;

		if (from == null)
		{
			return;
		}

		if (!Dealer.InRange(from.Location, 8))
		{
			from.PrivateOverheadMessage(Network.MessageType.Regular, 0x22, true, "I am too far away to do that", from.NetState);
		}
		else if (GetIndexFor(from) != -1)
		{
			from.SendMessage(0x22, "You are already seated at this table");
		}
		else if (Players.Count >= Dealer.MaxPlayers)
		{
			from.SendMessage(0x22, "Sorry, that table is full");
		}
		/*else if ( TournamentSystem.TournamentCore.SignedUpTeam( from ) != null || TournamentSystem.TournamentCore.FindTeam( from ) != null )
			from.SendMessage( 0x22, "You may not join a poker game while signed up for a tournament." );*/
		else if (Banker.Withdraw(from, player.Gold))
		{
			Point3D seat = Point3D.Zero;

			foreach (var seats in Dealer.Seats.Where(seats => !Dealer.SeatTaken(seats)))
			{
				seat = seats;
				break;
			}

			if (seat == Point3D.Zero)
			{
				from.SendMessage(0x22, "Sorry, that table is full.");
				return;
			}

			player.Game = this;
			player.Seat = seat;
			player.TeleportToSeat();
			Players.Players.Add(player);

			((PlayerMobile)from).PokerGame = this;
			from.SendMessage(0x22, "You have been seated at the table");

			if (Players.Count == 1 && !GameBackup.PokerGames.Contains(this))
			{
				GameBackup.PokerGames.Add(this);
			}
			else switch (State)
			{
				case PokerGameState.Inactive when Players.Count > 1 && !Dealer.TournamentMode:
					Begin();
					break;
				case PokerGameState.Inactive when Players.Count >= Dealer.MaxPlayers && Dealer.TournamentMode:
					Dealer.TournamentMode = false;
					Begin();
					break;
			}

			player.CloseGump(typeof(PokerTableGump));
			player.SendGump(new PokerTableGump(this, player));
			NeedsGumpUpdate = true;
		}
		else
		{
			from.SendMessage(0x22, "Your bank box lacks the funds to join this poker table");
		}
	}

	public void RemovePlayer(PokerPlayer player)
	{
		Mobile from = player.Mobile;

		if (from != null && Players.Contains(player))
		{
			Players.Players.Remove(player);

			if (Players.Peek() == player) //It is currently their turn, fold them.
			{
				player.CloseGump(typeof( PokerBetGump));
				Timer.LastPlayer = null;
				player.Action = PlayerAction.Fold;
			}

			if (Players.Round.Contains(player))
			{
				Players.Round.Remove(player);
			}

			if (Players.Turn.Contains(player))
			{
				Players.Turn.Remove(player);
			}

			if (Players.Round.Count == 0)
			{
				player.Gold += CommunityGold;
				CommunityGold = 0;

				if (GameBackup.PokerGames.Contains(this))
				{
					GameBackup.PokerGames.Remove(this);
				}
			}

			if (player.Gold > 0)
			{
				if (from.BankBox == null) //Should NEVER happen, but JUST IN CASE!
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("WARNING: Player \"{0}\" with account \"{1}\" had null bank box while trying to deposit {2} gold. Player will NOT receive their gold.", from.Name, from.Account == null ? "(-null-)" : from.Account.Username, player.Gold);
					Utility.PopColor();

					try
					{
						using StreamWriter op = new("poker_error.log", true);
						op.WriteLine("WARNING: Player \"{0}\" with account \"{1}\" had null bank box while poker script was trying to deposit {2} gold. Player will NOT receive their gold.", from.Name, from.Account == null ? "(-null-)" : from.Account.Username, player.Gold);
					}
					catch
					{
						// ignored
					}

					from.SendMessage(0x22, "WARNING: Could not find your bank box. All of your poker money has been lost in this error. Please contact a Game Master to resolve this issue.");
				}
				else
				{
					Banker.Deposit(from.BankBox, player.Gold);
					from.SendMessage(0x22, "{0}gp has been deposited into your bank box.", player.Gold);
				}
			}

			player.CloseAllGumps();
			((PlayerMobile)from).PokerGame = null;
			from.Location = Dealer.ExitLocation;
			from.Map = Dealer.ExitMap;
			from.SendMessage(0x22, "You have left the table");

			NeedsGumpUpdate = true;
		}
	}
}

public class ResultEntry : IComparable
{
	public PokerPlayer Player { get; }

	public List<Card> BestCards { get; set; }

	public HandRank Rank { get; set; }

	public ResultEntry(PokerPlayer player)
	{
		Player = player;
	}

	#region IComparable Members
	public int CompareTo(object obj)
	{
		if (obj is ResultEntry entry)
		{
			RankResult result = HandRanker.IsBetterThan(this, entry);

			switch (result)
			{
				case RankResult.Better:
					return -1;
				case RankResult.Worse:
					return 1;
			}
		}

		return 0;
	}
	#endregion
}

public class PokerGameTimer : Timer
{
	readonly PokerGame _game;
	PokerGameState _lastState;

	public PokerPlayer LastPlayer;
	public bool HasWarned;

	public PokerGameTimer(PokerGame game)
		: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
	{
		_game = game;
		_lastState = PokerGameState.Inactive;
		LastPlayer = null;
	}

	protected override void OnTick()
	{
		if (_game.State != PokerGameState.Inactive && _game.Players.Count < 2)
		{
			_game.End();
		}

		for (int i = 0; i < _game.Players.Count; ++i)
		{
			if (!_game.Players.Round.Contains( _game.Players[i] ) && _game.Players[i].RequestLeave)
			{
				_game.RemovePlayer(_game.Players[i]);
			}
		}

		if (_game.NeedsGumpUpdate)
		{
			foreach (PokerPlayer player in _game.Players.Players)
			{
				player.CloseGump(typeof(PokerTableGump));
				player.SendGump(new PokerTableGump(_game, player));
			}

			_game.NeedsGumpUpdate = false;
		}

		if (_game.State != _lastState && _game.Players.Round.Count > 1)
		{
			_lastState = _game.State;
			_game.DoRoundAction();
			LastPlayer = null;
		}

		if (_game.Players.Peek() != null)
		{
			LastPlayer ??= _game.Players.Peek();

			if (LastPlayer.BetStart.AddSeconds(45.0) <= DateTime.Now /*&& m_LastPlayer.Mobile.HasGump(typeof( PokerBetGump))*/ && !HasWarned)
			{
				LastPlayer.SendMessage(0x22, "You have 15 seconds left to make a choice. (You will automatically fold if no choice is made)");
				HasWarned = true;
			}
			else if (LastPlayer.BetStart.AddSeconds(60.0) <= DateTime.Now /*&& m_LastPlayer.Mobile.HasGump(typeof(PokerBetGump))*/)
			{
				PokerPlayer temp = LastPlayer;
				LastPlayer = null;

				temp.CloseGump(typeof(PokerBetGump));
				temp.Action = PlayerAction.Fold;
				HasWarned = false;
			}
		}
	}
}
