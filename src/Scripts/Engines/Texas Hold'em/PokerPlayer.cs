using System;
using System.Collections.Generic;
using Server.Gumps;

namespace Server.Poker;

public class PokerPlayer
{
	private PlayerAction _action;

	public int Gold { get; set; }

	public int Bet { get; set; }

	public int RoundGold { get; set; }

	public int RoundBet { get; set; }

	public bool RequestLeave { get; set; }

	public bool IsAllIn { get; set; }

	public bool Forced { get; set; }

	public bool LonePlayer { get; set; }

	public Mobile Mobile { get; set; }

	public PokerGame Game { get; set; }

	public Point3D Seat { get; set; }

	public DateTime BetStart { get; set; }

	public List<Card> HoleCards { get; }

	public PlayerAction Action
	{
		get => _action;
		set
		{
			_action = value;

			switch (_action)
			{
				case PlayerAction.None: break;
				case PlayerAction.Bet:
				case PlayerAction.Raise:
				case PlayerAction.Call:
				case PlayerAction.Check:
				case PlayerAction.Fold:
				case PlayerAction.AllIn:
				default:
					Game?.PokerGame_PlayerMadeDecision(this);
					break;
			}
		}
	}

	public bool HasDealerButton => Game.DealerButton == this;
	public bool HasSmallBlind => Game.SmallBlind == this;
	public bool HasBigBlind => Game.BigBlind == this;
	public bool HasBlindBet => Game.SmallBlind == this || Game.BigBlind == this;

	public PokerPlayer(Mobile from)
	{
		Mobile = from;
		HoleCards = new List<Card>();
	}

	public void ClearGame()
	{
		Bet = 0;
		RoundGold = 0;
		RoundBet = 0;
		HoleCards.Clear();
		Game = null;
		CloseAllGumps();
		_action = PlayerAction.None;
		IsAllIn = false;
		Forced = false;
		LonePlayer = false;
	}

	public void AddCard(Card card)
	{
		HoleCards.Add(card);
	}

	public void SetBbAction()
	{
		_action = PlayerAction.Bet;
	}

	public HandRank GetBestHand(List<Card> communityCards, out List<Card> bestCards)
	{
		return HandRanker.GetBestHand(GetAllCards(communityCards), out bestCards);
	}

	public List<Card> GetAllCards(List<Card> communityCards)
	{
		List<Card> hand = new(communityCards);

		hand.AddRange(HoleCards);
		hand.Sort();

		return hand;
	}

	public void CloseAllGumps()
	{
		CloseGump(typeof(PokerTableGump));
		CloseGump(typeof(PokerLeaveGump));
		CloseGump(typeof(PokerJoinGump));
		CloseGump(typeof(PokerBetGump));
	}

	public void CloseGump(Type type)
	{
		if (Mobile != null)
		{
			Mobile.CloseGump(type);
		}
	}

	public void SendGump(Gump toSend)
	{
		if (Mobile != null)
		{
			Mobile.SendGump(toSend);
		}
	}

	public void SendMessage(string message)
	{
		if (Mobile != null)
		{
			Mobile.SendMessage(message);
		}
	}

	public void SendMessage(int hue, string message)
	{
		if (Mobile != null)
		{
			Mobile.SendMessage(hue, message);
		}
	}

	public void TeleportToSeat()
	{
		if (Mobile != null && Seat != Point3D.Zero)
		{
			Mobile.Location = Seat;
		}
	}

	public bool IsOnline()
	{
		return Mobile is {NetState.Socket.Connected: true};
	}
}
