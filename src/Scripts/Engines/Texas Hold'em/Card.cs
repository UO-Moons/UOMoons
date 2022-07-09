using System;

namespace Server.Poker;

public class Card : IComparable
{
	public const int Red = 0x26;
	public const int Black = 0x00;

	public Suit Suit { get; }

	public Rank Rank { get; }

	public string Name => $"{Rank} of {Suit}".ToLower();
	public string RankString => Rank.ToString().ToLower();

	public Card(Suit suit, Rank rank)
	{
		Suit = suit;
		Rank = rank;
	}

	public string GetRankLetter()
	{
		if ((int)Rank < 11)
		{
			return ((int)Rank).ToString();
		}

		return Rank switch
		{
			Rank.Jack => "J",
			Rank.Queen => "Q",
			Rank.King => "K",
			Rank.Ace => "A",
			_ => "?"
		};
	}

	public int GetSuitColor() { return (int)Suit < 3 ? Red : Black; }

	public string GetSuitString()
	{
		return (int) Suit switch
		{
			1 => "\u25C6",
			2 => "\u2665",
			3 => "\u2663",
			4 => "\u2660",
			_ => "?"
		};
	}

	public override string ToString()
	{
		return $"{Rank} of {Suit}";
	}

	#region IComparable Members
	public int CompareTo(object obj)
	{
		if (obj is Card card)
		{
			if (Rank < card.Rank)
			{
				return 1;
			}

			if (Rank > card.Rank)
			{
				return -1;
			}
		}

		return 0;
	}
	#endregion
}
