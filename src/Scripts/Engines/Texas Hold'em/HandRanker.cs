using System;
using System.Collections.Generic;

namespace Server.Poker;

public class HandRanker
{
	public static string RankString(HandRank rank)
	{
		return rank switch
		{
			HandRank.None => "high card",
			HandRank.OnePair => "one pair",
			HandRank.TwoPairs => "two pairs",
			HandRank.ThreeOfAKind => "three of a kind",
			HandRank.Straight => "a straight",
			HandRank.Flush => "a flush",
			HandRank.FullHouse => "a full house",
			HandRank.FourOfAKind => "four of a kind",
			HandRank.StraightFlush => "a straight flush",
			HandRank.RoyalFlush => "a royal flush",
			_ => string.Empty
		};
	}

	public static string RankString(ResultEntry entry)
	{
		return entry.Rank switch
		{
			HandRank.None => $"high card {entry.BestCards[0].RankString}",
			HandRank.OnePair => $"a pair of {entry.BestCards[0].RankString}s",
			HandRank.TwoPairs => $"two pairs: {entry.BestCards[0].RankString}s and {entry.BestCards[2].RankString}s",
			HandRank.ThreeOfAKind => $"three {entry.BestCards[0].RankString}s",
			HandRank.Straight =>
				$"a straight: high {entry.BestCards[0].RankString} to low {entry.BestCards[4].RankString}",
			HandRank.Flush => $"a flush: high card {entry.BestCards[0].RankString}",
			HandRank.FullHouse =>
				$"a full house: 3 {entry.BestCards[0].RankString}s and 2 {entry.BestCards[3].RankString}s",
			HandRank.FourOfAKind => $"four {entry.BestCards[0].RankString}s",
			HandRank.StraightFlush => $"a straight flush: {entry.BestCards[0].Name} to {entry.BestCards[4].Name}",
			HandRank.RoyalFlush => "a royal flush",
			_ => string.Empty
		};
	}

	public static bool UsesKicker(HandRank rank)
	{
		if (rank is < HandRank.Straight or HandRank.FourOfAKind)
		{
			return true;
		}

		return false;
	}

	/*public static int GetKicker( List<ResultEntry> entries )
	{
		int startIndex = 0;

		switch ( entries[0].Rank )
		{
			case HandRank.None: startIndex = 1; break;
			case HandRank.OnePair: startIndex = 2; break;
			case HandRank.ThreeOfAKind: startIndex = 3; break;
			case HandRank.FourOfAKind:
			case HandRank.TwoPairs: return 4;
		}

		for ( int i = startIndex; i < 4; ++i )
		{
			foreach ( ResultEntry entry in entries )
				if ( entry.BestCards[i].Rank != entries[0].BestCards[i].Rank )
					startIndex = i;
		}

		return startIndex;
	}*/

	/// <summary>
	/// Returns whether the left Result entry is better than the right
	/// </summary>
	public static RankResult IsBetterThan(ResultEntry left, ResultEntry right)
	{
		if (left.Rank > right.Rank)
		{
			return RankResult.Better;
		}

		if (left.Rank < right.Rank)
		{
			return RankResult.Worse;
		}

		//Ranks are the same
		if (left.Rank != HandRank.RoyalFlush)
		{
			for (int i = 0; i < left.BestCards.Count; ++i)
			{
				if (left.BestCards[i].Rank > right.BestCards[i].Rank)
				{
					return RankResult.Better;
				}

				if (left.BestCards[i].Rank < right.BestCards[i].Rank)
				{
					return RankResult.Worse;
				}
			}
		}

		return RankResult.Same;
	}

	public static HandRank GetBestHand(List<Card> sortedCards, out List<Card> bestCards)
	{
		_ = new List<Card>();

		if ( HasRoyalFlush(sortedCards, out bestCards))
		{
			return HandRank.RoyalFlush;
		}

		if (HasStraightFlush(sortedCards, out bestCards))
		{
			return HandRank.StraightFlush;
		}

		if (Has4OfAKind(sortedCards, out bestCards))
		{
			AddHighestCards(1, sortedCards, bestCards, ref bestCards);
			return HandRank.FourOfAKind;
		}

		if (HasFullHouse(sortedCards, out bestCards))
		{
			return HandRank.FullHouse;
		}

		if (HasFlush(sortedCards, out bestCards))
		{
			return HandRank.Flush;
		}

		if (HasStraight(sortedCards, out bestCards))
		{
			return HandRank.Straight;
		}

		if (Has3OfAKind(sortedCards, out bestCards))
		{
			AddHighestCards(2, sortedCards, bestCards, ref bestCards);
			return HandRank.ThreeOfAKind;
		}

		if (Has2Pairs(sortedCards, out bestCards))
		{
			AddHighestCards(1, sortedCards, bestCards, ref bestCards);
			return HandRank.TwoPairs;
		}

		if (Has1Pair(sortedCards, out bestCards))
		{
			AddHighestCards(3, sortedCards, bestCards, ref bestCards);
			return HandRank.OnePair;
		}

		AddHighestCards(5, sortedCards, bestCards, ref bestCards);
		return HandRank.None;
	}

	private static void AddHighestCards(int numberOfCardsToAdd, List<Card> sortedSourceCards, List<Card> excludeCards, ref List<Card> targetCards)
	{
		int cardsAdded = 0;

		foreach (Card card in sortedSourceCards)
		{
			if (!excludeCards.Contains(card))
			{
				++cardsAdded;

				targetCards.Add(card);

				if (cardsAdded == numberOfCardsToAdd)
				{
					return;
				}
			}
		}
	}

	public static bool HasRoyalFlush(List<Card> sortedCards, out List<Card> royalFlushCards)
	{
		royalFlushCards = new List<Card>();

		if (sortedCards.Count < 5)
		{
			return false;
		}

		if (!HasFlush(sortedCards, out var flushCards))
		{
			return false;
		}

		//since the cards are sorted,
		//we need only to check the first and fifth card
		//if they are an Ace and 10 respectively
		if (flushCards[0].Rank == Rank.Ace && flushCards[4].Rank == Rank.Ten)
		{
			for (int i = 0; i < 5; ++i)
			{
				royalFlushCards.Add( flushCards[i] );
			}

			return true;
		}

		return false;
	}

	public static bool HasStraightFlush(List<Card> sortedCards, out List<Card> straightFlushCards)
	{
		straightFlushCards = new List<Card>();

		if (sortedCards.Count < 5)
		{
			return false;
		}

		if (!HasFlush(sortedCards, out var flushCards))
		{
			return false;
		}

		if (HasStraight(flushCards, out straightFlushCards))
		{
			return true;
		}

		return false;
	}

	public static bool Has4OfAKind(List<Card> sortedCards, out List<Card> fourOfAKindCards)
	{
		fourOfAKindCards = new List<Card>();

		if (sortedCards.Count < 4)
		{
			return false;
		}

		for (int i = 0; i < sortedCards.Count - 3; ++i)
		{
			if (sortedCards[i].Rank == sortedCards[i + 1].Rank && sortedCards[i].Rank == sortedCards[i + 2].Rank && sortedCards[i].Rank == sortedCards[i + 3].Rank)
			{
				fourOfAKindCards.Add(sortedCards[i]);
				fourOfAKindCards.Add(sortedCards[i + 1]);
				fourOfAKindCards.Add(sortedCards[i + 2]);
				fourOfAKindCards.Add(sortedCards[i + 3]);

				return true;
			}
		}

		return false;
	}

	public static bool HasFullHouse(List<Card> sortedCards, out List<Card> fullHouseCards)
	{
		fullHouseCards = new List<Card>();

		if (sortedCards.Count < 5)
		{
			return false;
		}

		//check for 3 of a kind
		if (!Has3OfAKind(sortedCards, out var threeCards))
		{
			return false;
		}

		foreach (Card card in threeCards)
		{
			fullHouseCards.Add(card);
		}

		//check for a pair, excluding the 3 cards
		List<Card> remainingCards = new();
		foreach (Card card in sortedCards)
		{
			if (!fullHouseCards.Contains(card))
			{
				remainingCards.Add(card);
			}
		}

		if (Has1Pair(remainingCards, out var twoCards))
		{
			fullHouseCards.AddRange(twoCards);

			return true;
		}

		fullHouseCards.Clear();

		return false;
	}

	public static bool HasFlush(List<Card> sortedCards, out List<Card> flushCards)
	{
		flushCards = new List<Card>();

		int cardCount = sortedCards.Count;

		if (cardCount < 5)
		{
			return false;
		}

		int errThreshhold = cardCount - 5;

		foreach (Suit suit in Enum.GetValues(typeof(Suit)))
		{
			var errs = 0;
			flushCards.Clear();

			foreach (Card card in sortedCards)
			{
				if (card.Suit == suit)
				{
					flushCards.Add(card);

					if (flushCards.Count == 5)
					{
						return true;
					}
				}
				else
				{
					errs++;

					if (errs > errThreshhold)
					{
						break;
					}
				}
			}
		}

		flushCards.Clear();

		return false;
	}

	public static bool HasStraight(List<Card> sortedCards, out List<Card> straightCards)
	{
		straightCards = new List<Card>();

		if (sortedCards.Count < 5)
		{
			return false;
		}

		int sequenceCardCount = 0;

		foreach (Card card in sortedCards)
		{
			if (sortedCards.IndexOf(card) == 0)
			{
				straightCards.Add(card);
				sequenceCardCount++;
			}
			else
			{
				//current card is in sequence with previous
				if (straightCards[^1].Rank == card.Rank + 1)
				{
					straightCards.Add(card);
					sequenceCardCount++;
				}
				//current card is the same value as previous, ignore
				else if (straightCards[^1].Rank == card.Rank)
				{
				}
				//current card is not in sequence with previous so reset
				else
				{
					straightCards.Clear();
					straightCards.Add(card);
					sequenceCardCount = 1;
				}
			}

			if (sequenceCardCount == 5)
			{
				break;
			}
		}

		if (sequenceCardCount == 5)
		{
			return true;
		}

		//special case, if the straight is 5, 4, 3, 2, 
		//check for the ace which should be the first card in sortedCards
		if (sequenceCardCount == 4 && straightCards[^1].Rank == Rank.Two && sortedCards[0].Rank == Rank.Ace)
		{
			straightCards.Add( sortedCards[0] );

			return true;
		}

		return false;
	}

	public static bool Has3OfAKind(List<Card> sortedCards, out List<Card> threeOfAKindCards)
	{
		threeOfAKindCards = new List<Card>();

		if (sortedCards.Count < 3)
		{
			return false;
		}

		for (int i = 0; i < sortedCards.Count - 2; ++i)
		{
			if (sortedCards[i].Rank == sortedCards[i + 1].Rank && sortedCards[i].Rank == sortedCards[i + 2].Rank)
			{
				threeOfAKindCards.Add(sortedCards[i]);
				threeOfAKindCards.Add(sortedCards[i + 1]);
				threeOfAKindCards.Add(sortedCards[i + 2]);

				return true;
			}
		}

		return false;
	}

	public static bool Has2Pairs(List<Card> sortedCards, out List<Card> twoPairsCards)
	{
		twoPairsCards = new List<Card>();

		if (sortedCards.Count < 4)
		{
			return false;
		}

		int pairCount = 0;

		for (int i = 0; i < sortedCards.Count - 1; ++i)
		{
			if (sortedCards[i].Rank == sortedCards[i + 1].Rank)
			{
				twoPairsCards.Add(sortedCards[i]);
				twoPairsCards.Add(sortedCards[i + 1]);

				++pairCount;
					
				if (pairCount == 2)
				{
					return true;
				}

				//skip the next card
				++i;
			}
		}

		return false;
	}

	public static bool Has1Pair(List<Card> sortedCards, out List<Card> onePairCards)
	{
		onePairCards = new List<Card>();

		if (sortedCards.Count < 2)
		{
			return false;
		}

		for (int i = 0; i < sortedCards.Count - 1; ++i)
		{
			if (sortedCards[i].Rank == sortedCards[i + 1].Rank)
			{
				onePairCards.Add(sortedCards[i]);
				onePairCards.Add(sortedCards[i + 1]);

				return true;
			}
		}

		return false;
	}
}
