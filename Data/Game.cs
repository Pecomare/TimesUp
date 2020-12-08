using System;
using System.Collections.Generic;
using System.Linq;
using TimesUp.Context;
using TimesUp.Extensions;

namespace TimesUp.Data
{
	public class Game
	{
		public Guid Guid { get; } = Guid.NewGuid();
		public string Name { get; set; } = string.Empty;
		public GameState State { get; private set; } = GameState.NEW;
		public string Owner { get; private set; }
		public int MaximumPlayerCount { get; set; } = 10;
		public Dictionary<string, long> PlayerScores { get; } = new();
		public int MaximumRoundCount { get; set; } = 2;
		public int RoundNumber { get; private set; } = 1;
		public Deck? Deck { get; private set; }
		public List<Card>? CardsToPlay { get; set; }
		public Card? CardToPlay => State != GameState.RUNNING
			? null
			: CardsToPlay?.FirstOrDefault();
		public List<Card> FoundCards { get; } = new();
		public List<string>? PlayerOrder { get; set; }
		private int CurrentPlayerIndex { get; set; }
		public string? CurrentPlayer => PlayerOrder?[CurrentPlayerIndex];
		public int TimePerRound { get; set; } = 60;
		public int RemainingSeconds { get; private set; }
		public bool IsTimeTicking { get; set; } = false;
		public bool IsOnLastPlayer => CurrentPlayerIndex == PlayerOrder?.Count - 1;
		public bool IsOnLastRound => RoundNumber == MaximumRoundCount;
		public bool HasStillCardsToFind => CardsToPlay?.Any() ?? false;
		public DateTime ModifiedDateTime { get; private set; }

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

		public void ChangeDeck(Deck deck)
		{
			if (State != GameState.NEW)
			{
				throw new InvalidOperationException("Cannot change deck while game is running.");
			}
			Deck = deck;
			ModifiedDateTime = DateTime.Now;
		}

		public void SetOwner(string name)
		{
			Owner = name;
			ModifiedDateTime = DateTime.Now;
		}

		public void Start()
		{
			if (!PlayerScores.Any())
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
			PlayerOrder = PlayerScores.Keys.ToList().Shuffle();
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
			if (PlayerScores.ContainsKey(name))
			{
				return;
			}
			if (PlayerScores.Count == MaximumPlayerCount)
			{
				throw new InvalidOperationException("Cannot exceed player limit.");
			}
			PlayerScores.Add(name, 0);
			if (State == GameState.RUNNING)
			{
				PlayerOrder.Add(name);
			}
			ModifiedDateTime = DateTime.Now;
		}

		public bool RemovePlayer(string name)
		{
			CheckEnded();
			if (!PlayerScores.Any())
			{
				throw new InvalidOperationException("Cannot remove a player if there isn't any.");
			}
			bool isRemoved = PlayerScores.Remove(name);
			if (State == GameState.RUNNING)
			{
				isRemoved = isRemoved && PlayerOrder.Remove(name);
			}
			ModifiedDateTime = DateTime.Now;
			return isRemoved;
		}

		private void GivePointToCurrentPlayer()
		{
			PlayerScores[PlayerOrder[CurrentPlayerIndex]]++;
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
			if (CurrentPlayerIndex == PlayerScores?.Count - 1)
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