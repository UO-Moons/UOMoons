using System;
using System.Collections.Generic;

namespace Server.Poker;

public class Deck
{
	private Stack<Card> _deck;
	private List<Card> _usedCards;

	public Deck()
	{
		InitDeck();
	}

	private void InitDeck()
	{
		_deck = new Stack<Card>(52);
		_usedCards = new List<Card>();

		foreach (Suit s in Enum.GetValues(typeof(Suit)))
		{
			foreach (Rank r in Enum.GetValues(typeof(Rank)))
			{
				_deck.Push(new Card(s, r));
			}
		}

		Shuffle(5);
	}

	public Card Pop() { _usedCards.Add( _deck.Peek() ); return _deck.Pop(); }

	public Card Peek() { return _deck.Peek(); }

	public void Shuffle(int count)
	{
		List<Card> deck = new(_deck.ToArray());

		for (int i = 0; i < count; ++i)
		{
			for (int j = 0; j < deck.Count; ++j)
			{
				int index = Utility.Random(deck.Count);

				(deck[index], deck[j]) = (deck[j], deck[index]);
			}
		}

		_deck = new Stack<Card>(deck);
	}
}
