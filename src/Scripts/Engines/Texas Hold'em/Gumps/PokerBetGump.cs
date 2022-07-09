using System;
using Server.Gumps;
using Server.Network;

namespace Server.Poker;

public class PokerBetGump : Gump
{
	private const int ColorWhite = 0xFFFFFF;
	private const int ColorGreen = 0x00FF00;

	private readonly bool _canCall;
	private readonly PokerGame _game;
	private readonly PokerPlayer _player;

	public PokerBetGump(PokerGame game, PokerPlayer player, bool canCall)
		: base(460, 400)
	{
		_canCall = canCall;
		_game = game;
		_player = player;

		Closable = false;
		Disposable = false;
		Dragable = true;
		Resizable = false;
		AddPage( 0 );

		AddBackground(0, 0, 160, 155, 9270);

		AddRadio(14, 10, 9727, 9730, true, canCall ? (int)Buttons.Call : (int)Buttons.Check);
		AddRadio(14, 40, 9727, 9730, false, (int)Buttons.Fold);
		AddRadio(14, 70, 9727, 9730, false, (int)Buttons.AllIn);
		AddRadio(14, 100, 9727, 9730, false, canCall ? (int)Buttons.Raise : (int)Buttons.Bet);

		AddHtml(45, 14, 60, 45, Color(canCall ? "Call" : "Check", ColorWhite), false, false);

		if (canCall)
		{
			AddHtml(75, 14, 60, 22, Color(Center(_game.CurrentBet - player.RoundBet >= player.Gold ? "all-in" : $"{_game.CurrentBet - _player.RoundBet:#,###}"), ColorGreen), false, false);
		}

		AddHtml(45, 44, 60, 45, Color("Fold", ColorWhite), false, false);
		AddHtml(45, 74, 60, 45, Color("All In", ColorWhite), false, false);
		AddHtml(45, 104, 60, 45, Color(canCall ? "Raise" : "Bet", ColorWhite), false, false);

		AddTextEntry(85, 104, 60, 22, 455, (int)Buttons.TxtBet, game.Dealer.BigBlind.ToString());

		AddButton(95, 132, 247, 248, (int)Buttons.Okay, GumpButtonType.Reply, 0);
	}

	public enum Buttons
	{
		None,
		Check,
		Call,
		Fold,
		Bet,
		Raise,
		AllIn,
		TxtBet,
		Okay
	}

	public new static string Center(string text)
	{
		return $"<CENTER>{text}</CENTER>";
	}

	public new static string Color(string text, int color)
	{
		return $"<BASEFONT COLOR=#{color:X6}>{text}</BASEFONT>";
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		Mobile from = state.Mobile;

		if (from == null)
		{
			return;
		}

		if (_game.Players.Peek() != _player)
		{
			return;
		}

		if (info.ButtonID == 8) //Okay
		{
			if (info.IsSwitched((int)Buttons.Check))
			{
				_player.Action = PlayerAction.Check;
			}
			else if (info.IsSwitched((int)Buttons.Call))
			{
				if (_game.CurrentBet >= _player.Gold)
				{
					_player.Forced = true;
					_player.Action = PlayerAction.AllIn;
				}
				else
				{
					_player.Bet = _game.CurrentBet - _player.RoundBet;
					_player.Action = PlayerAction.Call;
				}

			}
			else if (info.IsSwitched((int)Buttons.Fold))
			{
				_player.Action = PlayerAction.Fold;
			}
			else if (info.IsSwitched((int)Buttons.Bet))
			{
				int bet = 0;

				info.GetTextEntry((int)Buttons.TxtBet);

				try { bet = Convert.ToInt32(info.GetTextEntry((int)Buttons.TxtBet).Text); }
				catch
				{
					// ignored
				}

				if (bet < _game.Dealer.BigBlind)
				{
					from.SendMessage(0x22, "Your must bet at least {0}gp.", _game.BigBlind);

					from.CloseGump(typeof(PokerBetGump));
					from.SendGump(new PokerBetGump(_game, _player, _canCall));
				}
				else if (bet > _player.Gold)
				{
					from.SendMessage(0x22, "You cannot bet more gold than you currently have!");

					from.CloseGump(typeof(PokerBetGump));
					from.SendGump(new PokerBetGump(_game, _player, _canCall));
				}
				else if (bet == _player.Gold)
				{
					_player.Action = PlayerAction.AllIn;
				}
				else
				{
					_player.Bet = bet;
					_player.Action = PlayerAction.Bet;
				}
			}
			else if (info.IsSwitched((int)Buttons.Raise)) //Same as bet, but add value to current bet
			{
				int bet = 0;

				info.GetTextEntry((int)Buttons.TxtBet);

				try { bet = Convert.ToInt32(info.GetTextEntry((int)Buttons.TxtBet).Text); }
				catch
				{
					// ignored
				}

				if (bet < 100)
				{
					from.SendMessage(0x22, "If you are going to raise a bet, it needs to be by at least 100gp.");

					from.CloseGump(typeof(PokerBetGump));
					from.SendGump(new PokerBetGump(_game, _player, _canCall));
				}
				else if (bet + _game.CurrentBet > _player.Gold)
				{
					from.SendMessage(0x22, "You do not have enough gold to raise by that much.");

					from.CloseGump(typeof(PokerBetGump));
					from.SendGump(new PokerBetGump(_game, _player, _canCall));
				}
				else if (bet + _game.CurrentBet == _player.Gold)
				{
					_player.Action = PlayerAction.AllIn;
				}
				else
				{
					_player.Bet = bet;
					_player.Action = PlayerAction.Raise;
				}
			}
			else if (info.IsSwitched((int)Buttons.AllIn))
			{
				_player.Action = PlayerAction.AllIn;
			}
		}
	}
}
