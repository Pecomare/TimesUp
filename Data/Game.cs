using System;
using System.Collections.Generic;
using System.Linq;
using TimesUp.Context;
using TimesUp.Extensions;

namespace TimesUp.Data
{
	public class Game
	{
		private static readonly List<Card> EMPTY_CARD_LIST = new List<Card>(0);

		public Guid Guid { get; } = Guid.NewGuid();
		public string Name { get; set; } = string.Empty;
		public GameState State { get; private set; } = GameState.NEW;
		public string Owner { get; private set; } = string.Empty;
		public int MaximumPlayerCount { get; set; } = 10;
		public List<Player> Players { get; private set; } = new();
		public int MaximumRoundCount { get; set; } = 2;
		public int RoundNumber { get; private set; } = 1;
		public Deck? Deck { get; private set; }
		public List<Card> CardsToPlay { get; set; } = EMPTY_CARD_LIST;
		public DateTime ModifiedDateTime { get; private set; }
		public List<Card> FoundCards { get; } = new();
		private int CurrentPlayerIndex { get; set; }
		public int TimePerRound { get; set; } = 60;
		public int RemainingSeconds { get; private set; }
		public bool IsTimeTicking { get; set; } = false;

		public Player? CurrentPlayer => Players.ElementAtOrDefault(CurrentPlayerIndex);
		public bool IsOnLastPlayer => CurrentPlayerIndex == Players.Count - 1;
		public bool IsOnLastRound => RoundNumber == MaximumRoundCount;
		public bool HasStillCardsToFind => CardsToPlay.Any();
		public Card? CardToPlay => State != GameState.RUNNING
			? null
			: CardsToPlay?.FirstOrDefault();

		#region Constructors

		public Game(string name)
		{
			Name = name;
			ModifiedDateTime = DateTime.Now;
		}

		#endregion

		#region Methods

		public void Rename(string newName)
		{
			Name = newName;
			ModifiedDateTime = DateTime.Now;
		}

		public void SetDeck(Deck deck)
		{
			if (State != GameState.NEW)
			{
				throw new InvalidOperationException("Cannot change deck while game is running.");
			}
			Deck = deck;
			ModifiedDateTime = DateTime.Now;
		}

		public void SetDeckIfNull(Deck deck)
		{
			if (Deck == null)
			{
				SetDeck(deck);
			}
		}

		public void SetOwner(string name)
		{
			Owner = name;
			ModifiedDateTime = DateTime.Now;
		}

		public void Start()
		{
			if (!Players.Any())
			{
				throw new InvalidOperationException("Cannot start a game without players.");
			}
			if (Deck == null)
			{
				throw new InvalidOperationException("Cannot start a game without a deck.");
			}
			if (!Deck.Cards.Any())
			{
				throw new InvalidOperationException("Cannot start a game with an empty deck.");
			}
			if (State != GameState.NEW)
			{
				throw new InvalidOperationException("Cannot start a game twice.");
			}
			CardsToPlay = Deck.Cards.Shuffle();
			Players = Players.Shuffle();
			State = GameState.RUNNING;
			RemainingSeconds = TimePerRound;
			ModifiedDateTime = DateTime.Now;
		}

		public void End() 
		{
			if (State == GameState.ENDED)
			{
				throw new InvalidOperationException("Cannot end a game twice.");
			}
			State = GameState.ENDED;
			ModifiedDateTime = DateTime.Now;
		}

		private void CheckEnded()
		{
			if (State == GameState.ENDED)
			{
				throw new InvalidOperationException("Game has already ended.");
			}
		}

		public void AddPlayer(string name)
		{
			CheckEnded();
			if (Players.Any(player => string.Equals(player.Name, name)))
			{
				return;
			}
			if (Players.Count == MaximumPlayerCount)
			{
				throw new InvalidOperationException("Cannot exceed player limit.");
			}
			Players.Add(new Player(name));
			ModifiedDateTime = DateTime.Now;
		}

		public bool RemovePlayer(string name)
		{
			CheckEnded();
			if (!Players.Any())
			{
				throw new InvalidOperationException("Cannot remove a player if there isn't any.");
			}
			int removedCount = Players.RemoveAll(player => string.Equals(player.Name, name));
			ModifiedDateTime = DateTime.Now;
			return removedCount > 0;
		}

		public bool IsPlayerInGame(string name) => Players.Any(player => string.Equals(player.Name, name));

		private void GivePointToCurrentPlayer()
		{
			if (State != GameState.RUNNING)
			{
				throw new InvalidOperationException("Cannot give point while game is not running.");
			}
			if (CurrentPlayer == null)
			{
				throw new InvalidOperationException("Cannot give point without any player.");
			}
			CurrentPlayer.Score++;
			ModifiedDateTime = DateTime.Now;
		}

		public void AcceptCard()
		{
			if (State != GameState.RUNNING)
			{
				throw new InvalidOperationException("Cannot find a card if game is not running.");
			}
			if (!CardsToPlay.Any())
			{
				throw new InvalidOperationException("Cannot find a card in an emtpy list.");
			}
			GivePointToCurrentPlayer();
			Card card = CardsToPlay.First();
			CardsToPlay.Remove(card);
			FoundCards.Add(card);
			if (!CardsToPlay.Any() && IsOnLastRound)
			{
				End();
			}
			ModifiedDateTime = DateTime.Now;
		}

		public void SkipCard()
		{
			if (State != GameState.RUNNING)
			{
				throw new InvalidOperationException("Cannot skip a card if game is not running.");
			}
			if (!CardsToPlay.Any())
			{
				throw new InvalidOperationException("Cannot skip a card in an emtpy list.");
			}
			Card card = CardsToPlay.First();
			CardsToPlay.Remove(card);
			CardsToPlay.Add(card);
			ModifiedDateTime = DateTime.Now;
		}

		public void PerformAnotherRound()
		{
			if (State != GameState.RUNNING)
			{
				throw new InvalidOperationException("Cannot do another round in a non-running game.");
			}
			CurrentPlayerIndex = 0;
			RemainingSeconds = TimePerRound;
			ModifiedDateTime = DateTime.Now;
		}

		public void GoToNextPhase()
		{
			if (State != GameState.RUNNING)
			{
				throw new InvalidOperationException("Cannot go to next round if game is not running.");
			}
			if (RoundNumber == MaximumRoundCount)
			{
				throw new InvalidOperationException("Final phase already achieved.");
			}
			RoundNumber++;
			CurrentPlayerIndex = 0;
			CardsToPlay = FoundCards.Shuffle();
			FoundCards.Clear();
			RemainingSeconds = TimePerRound;
			ModifiedDateTime = DateTime.Now;
		}

		public void GoToNextPlayer()
		{
			if (State != GameState.RUNNING)
			{
				throw new InvalidOperationException("Cannot go to next player if game is not running.");
			}
			if (CurrentPlayerIndex == Players.Count - 1)
			{
				throw new InvalidOperationException("Last player already played.");
			}
			SkipCard();
			CurrentPlayerIndex++;
			RemainingSeconds = TimePerRound;
			ModifiedDateTime = DateTime.Now;
		}

		public void DecrementTimer()
		{
			RemainingSeconds--;
		}

		#endregion
	}
}