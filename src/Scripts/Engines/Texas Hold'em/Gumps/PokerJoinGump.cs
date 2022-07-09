using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Poker;

public class PokerJoinGump : Gump
{
	private readonly PokerGame _game;

	public PokerJoinGump(Mobile from, PokerGame game)
		: base(50, 50)
	{
		_game = game;

		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);

		AddBackground(0, 0, 385, 393, 9270);
		AddImageTiled( 18, 15, 350, 320, 9274);
			
		AddLabel(125, 10, 28, "The Shard's Texas Hold-em");
		AddLabel(133, 25, 28, " Join Poker Table");
		AddImageTiled(42, 47, 301, 3, 96);
		AddLabel(65, 62, 68, "You are about to join a game of Poker.");
		AddImage(33, 38, 95, 68);
		AddImage(342, 38, 97, 68);
		AddLabel(52, 80, 68, "All bets involve real gold and no refunds will be");
		AddLabel(54, 98, 68, "given. If you feel uncomfortable losing gold or");
		AddLabel(40, 116, 68, "are unfamiliar with the rules of Texas Hold'em, you");
		AddLabel(100, 134, 68, "are advised against proceeding.");

		AddLabel(122, 161, 1149, "Small Blind:");
		AddLabel(129, 181, 1149, "Big Blind:");
		AddLabel(123, 201, 1149, "Min Buy-In:");
		AddLabel(120, 221, 1149, "Max Buy-In:");
		AddLabel(110, 241, 1149, "Bank Balance:");
		AddLabel(101, 261, 1149, "Buy-In Amount:");

		AddLabel(200, 161, 148, _game.Dealer.SmallBlind.ToString("#,###") + "gp");
		AddLabel(200, 181, 148, _game.Dealer.BigBlind.ToString("#,###") + "gp");
		AddLabel(200, 201, 148, _game.Dealer.MinBuyIn.ToString("#,###") + "gp");
		AddLabel(200, 221, 148, _game.Dealer.MaxBuyIn.ToString("#,###") + "gp");

		int balance = Banker.GetBalance(from);
		int balanceHue = 28;
		int layout = 0;

		if (balance >= _game.Dealer.MinBuyIn)
		{
			balanceHue = 266;
			layout = 1;
		}

		AddLabel(200, 241, balanceHue, balance.ToString("#,###") + "gp");

		if (layout == 0)
		{
			AddLabel(200, 261, 1149, "(not enough gold)");
			AddButton(163, 292, 242, 241, (int)Handlers.BtnCancel, GumpButtonType.Reply, 0);
		}
		else
		{
			AddImageTiled(200, 261, 80, 19, 0xBBC);
			AddAlphaRegion(200, 261, 80, 19);
			AddTextEntry(203, 261, 77, 19, 68, (int)Handlers.TxtBuyInAmount, _game.Dealer.MinBuyIn.ToString());
			AddButton(123, 292, 247, 248, (int)Handlers.BtnOkay, GumpButtonType.Reply, 0);
			AddButton(200, 292, 242, 241, (int)Handlers.BtnCancel, GumpButtonType.Reply, 0);
		}
	}

	public enum Handlers
	{
		BtnOkay = 1,
		BtnCancel,
		TxtBuyInAmount
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		Mobile from = sender.Mobile;

		if (info.ButtonID == 1)
		{
			int balance = Banker.GetBalance(from);

			if (balance >= _game.Dealer.MinBuyIn)
			{
				int buyInAmount;
				try
				{
					buyInAmount = Convert.ToInt32(info.TextEntries[0].Text);
				}
				catch
				{
					from.SendMessage(0x22, "Use numbers without commas to input your buy-in amount (ie 25000)");
					return;
				}

				if (buyInAmount <= balance && buyInAmount >= _game.Dealer.MinBuyIn && buyInAmount <= _game.Dealer.MaxBuyIn)
				{
					PokerPlayer player = new(from)
					{
						Gold = buyInAmount
					};

					_game.AddPlayer( player );
				}
				else
				{
					from.SendMessage(0x22, "You may not join with that amount of gold. Minimum buy-in: " + Convert.ToString(_game.Dealer.MinBuyIn) + ", Maximum buy-in: " + Convert.ToString(_game.Dealer.MaxBuyIn));
				}
			}
			else
			{
				from.SendMessage(0x22, "You may not join with that amount of gold. Minimum buy-in: " + Convert.ToString(_game.Dealer.MinBuyIn) + ", Maximum buy-in: " + Convert.ToString(_game.Dealer.MaxBuyIn));
			}
		}
		else if (info.ButtonID == 2)
		{
		}
	}
}
