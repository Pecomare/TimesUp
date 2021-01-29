using TimesUp.Data;

namespace TimesUp.Context
{
	public class DeckAddedMessage
	{
		public Deck Deck { get; set; }

		public DeckAddedMessage(Deck deck)
		{
			Deck = deck;
		}
	}
}