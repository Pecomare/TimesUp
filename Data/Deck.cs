using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;

namespace TimesUp.Data
{
	/// <summary>
	/// Represents a named list of cards.
	/// </summary>
	[Table("Deck")]
	public class Deck
	{
		/// <summary>
		/// Globally unique ID.
		/// </summary>
		public Guid Id { get; set; } = Guid.NewGuid();
		/// <summary>
		/// Name of the deck.
		/// </summary>
		public string Name { get; set; } = string.Empty;
		/// <summary>
		/// List of cards in the deck.
		/// </summary>
		public ICollection<Card> Cards { get; set; } = new List<Card>();

		/// <summary>
		/// Number of cards in the deck.
		/// </summary>
		public int Count => Cards.Count;

		/// <summary>
		/// Creates an unnamed Deck.
		/// </summary>
		public Deck()
		{
			
		}

		/// <summary>
		/// Creates a Deck.
		/// </summary>
		/// <param name="name">Name of the new deck</param>
		public Deck(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Adds a card to the deck.
		/// </summary>
		/// <param name="card">The <see cref="Card" /> to add to the deck</param>
		public void AddCard(Card card)
		{
			Cards.Add(card);
		}

		/// <summary>
		/// Removes a card from the deck.
		/// </summary>
		/// <param name="card">The <see cref="Card" /> to remove from the deck</param>
		/// <returns><c>true</c> if the card has been removed, otherwise <c>false</c></returns>
		public bool RemoveCard(Card card)
		{
			return Cards.Remove(card);
		}
	}
}