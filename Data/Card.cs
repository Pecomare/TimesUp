using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesUp.Data
{
	[Table("Card")]
	public class Card
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Text1 { get; set; } = string.Empty;
		public string Text2 { get; set; } = string.Empty;
		public Deck? Deck { get; set; }

		public Card()
		{
			
		}

		public Card(Deck? deck, string text)
		{
			Deck = deck;
			Text1 = text;
			Text2 = string.Empty;
		}
	}
}