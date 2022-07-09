using System;
using Server.Gumps;

namespace Server.Poker;

public class PokerTableGump : Gump
{
	private const int CardX = 300;
	private const int CardY = 270;

	private const int ColorGold = 0xFFD700;
	private const int ColorGreen = 0x00FF00;
	private const int ColorOffWhite = 0xFFFACD;
	private const int ColorPink = 0xFF0099;

	private readonly PokerGame _game;
	private readonly PokerPlayer _player;

	public PokerTableGump(PokerGame game, PokerPlayer player)
		: base(0, 0)
	{
		_game = game;
		_player = player;

		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);

		if (_game.State > PokerGameState.PreFlop)
		{
			DrawCards();
		}

		DrawPlayers();

		if (_game.State > PokerGameState.Inactive)
		{
			AddLabel(350, 340, 148, "Pot: " + _game.CommunityGold.ToString("#,###"));
		}
	}

	private void DrawPlayers()
	{
		const int radius = 240;
		int centerX = CardX + (_game.State < PokerGameState.Turn ? 15 : _game.State < PokerGameState.River ? 30 : 45);
		const int centerY = CardY + radius;

		if (_game.State > PokerGameState.DealHoleCards)
		{
			int lastX = centerX;
			const int lastY = centerY - 85;

			for (int i = 0; i < _player.HoleCards.Count; ++i)
			{
				AddBackground(lastX, lastY, 71, 95, 9350);
				AddLabelCropped(lastX + 10, lastY + 5, 80, 60, _player.HoleCards[i].GetSuitColor(), _player.HoleCards[i].GetRankLetter());
				AddLabelCropped(lastX + 6, lastY + 25, 75, 60, _player.HoleCards[i].GetSuitColor(), _player.HoleCards[i].GetSuitString());

				lastX += 30;
			}
		}

		int playerIndex = _game.GetIndexFor(_player.Mobile);
		int counter = _game.Players.Count - 1;

		for (double i = playerIndex + 1; counter >= 0; ++i)
		{
			if (i == _game.Players.Count)
			{
				i = 0;
			}

			PokerPlayer current = _game.Players[(int)i];

			double xdist = radius * Math.Sin(counter * 2.0 * Math.PI / _game.Players.Count);
			double ydist = radius * Math.Cos(counter * 2.0 * Math.PI / _game.Players.Count);

			int x = centerX + (int)xdist;
			int y = CardY + (int)ydist;

			AddBackground(x, y, 101, 65, 9270); //This is the gump that shows your name and gold left.

			if (current.HasBlindBet || current.HasDealerButton)
			{
				AddHtml(x, y - 15, 101, 45, Color(Center(current.HasBigBlind ? "(Big Blind)" : current.HasSmallBlind ? "(Small Blind)" : "(Dealer Button)"), ColorGreen), false, false); 
			}

			AddHtml(x, y + 5, 101, 45, Color(Center(current.Mobile.Name), _game.Players.Peek() == current ? ColorGreen : !_game.Players.Round.Contains(current) ? ColorOffWhite : ColorPink), false, false);
			AddHtml(x + 2, y + 24, 101, 45, Color(Center("(" + current.Gold.ToString("#,###") + ")"), ColorGold), false, false);

			--counter;
		}
	}

	private void DrawCards()
	{
		int lastX = CardX;

		for (int i = 0; i < _game.CommunityCards.Count; ++i)
		{
			AddBackground(lastX, CardY, 71, 95, 9350);
			AddLabelCropped(lastX + 10, CardY + 5, 80, 60, _game.CommunityCards[i].GetSuitColor(), _game.CommunityCards[i].GetRankLetter());
			AddLabelCropped(lastX + 6, CardY + 25, 75, 60, _game.CommunityCards[i].GetSuitColor(), _game.CommunityCards[i].GetSuitString());

			lastX += 30;
		}
	}

	private new static string Center(string text)
	{
		return $"<CENTER>{text}</CENTER>";
	}

	private new static string Color(string text, int color)
	{
		return $"<BASEFONT COLOR=#{color:X6}>{text}</BASEFONT>";
	}
}
