using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using TimesUp.Context;
using TimesUp.Data;

namespace TimesUp.Pages
{
	public class LobbyComponent : ComponentBase, IHandle<GameChangedMessage>, IHandle<GameAddedMessage>
	{
		private static readonly List<Deck> EmptyDeckList = new();
		// [Parameter]
		// public string Action { get; set; } = string.Empty;
		[Parameter]
		public Guid? Guid { get; set; }
		[Parameter]
		public string? Nickname { get; set; }
		public Game? Game { get; set; }
		[Inject]
		private IEventAggregator _eventAggregator { get; set; }
		[Inject]
		private NavigationManager _navigationManager { get; set; }
		[Inject]
		private IDbContextFactory<TimesUpContext> DbFactory { get; set; }
		public string Name { get; set; } = string.Empty;
		public string GameName { get; set; } = string.Empty;
		public string ErrorMessage { get; set; } = string.Empty;
		public ServerStatus ServerStatus => ServerStatus.GetServerStatus();
		public List<Deck> AvailableDecks { get; set; } = EmptyDeckList;
		public bool IsTimeTicking { get; set; } = false;
		private Log? _log;
		public Log Log => _log ?? (_log = new Log());

		#region Initialization

		protected override void OnInitialized()
		{
			_eventAggregator.Subscribe(this);
		}

		protected override async Task OnParametersSetAsync()
		{
			if (!Guid.HasValue)
			{
				Game = null;
				AvailableDecks = new();
				return;
			}
			Game = ServerStatus.GetGameById(Guid.Value);
			if (Game == null)
			{
				ErrorMessage = $"La partie d'identifiant {Guid} n'existe pas.";
				AvailableDecks = new();
				return;
			}
			ErrorMessage = "";
			if (!string.IsNullOrWhiteSpace(Nickname))
			{
				Game.AddPlayer(Nickname);
				await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
				if (Game.State == GameState.NEW)
				{
					TimesUpContext context = DbFactory.CreateDbContext();
					AvailableDecks = await context.Decks.ToListAsync();
				}
			}
		}

		#endregion

		public async Task Refresh()
		{
			await InvokeAsync(() => 
			{
				StateHasChanged();
			});
		}

		protected async void CreateGame()
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				ErrorMessage = "Le nom ne peut pas être vide.";
				return;
			}
			ErrorMessage = "";
			TimesUpContext context = DbFactory.CreateDbContext();
			AvailableDecks = await context.Decks.Include(deck => deck.Cards).ToListAsync();
			Game game = ServerStatus.GetServerStatus().CreateGame(GameName);
			game.SetOwner(Name);
			game.AddPlayer(Name);
			game.ChangeDeck(AvailableDecks.First());
			Game = game;
			_navigationManager.NavigateTo($"/Game/{game.Guid}/{Name}");
			await _eventAggregator.PublishAsync(new GameAddedMessage(game));
		}

		protected async void JoinGame(Game game)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				ErrorMessage = "Le pseudo ne peut pas être vide";
				return;
			}
			ErrorMessage = "";
			Game = game;
			game.AddPlayer(Name);
			Guid = game.Guid;
			_navigationManager.NavigateTo($"/Game/{Guid}/{Name}");
			await _eventAggregator.PublishAsync(new GameChangedMessage(game));
		}

		protected async void StartGame()
		{
			Game.Start();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeDeck(ChangeEventArgs e)
		{
			if (!int.TryParse(e.Value?.ToString(), out int id))
			{
				ErrorMessage = "Could not parse value";
				return;
			}
			Deck? deck= AvailableDecks.Find(deck => deck.Id == id);
			if (deck == null)
			{
				ErrorMessage = $"Cannot find deck with id {id}.";
				return;
			}
			ErrorMessage = "";
			Game.ChangeDeck(deck);
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void CardFound()
		{
			Game.AcceptCard();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void CardSkipped()
		{
			Game.SkipCard();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void StartTimer()
		{
			Game.IsTimeTicking = true;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
			while (Game.RemainingSeconds > 0)
			{
				await Task.Delay(1000);
				Game.DecrementTimer();
				await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
			}
			Game.IsTimeTicking = false;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void GoToNextPlayer()
		{
			Game.GoToNextPlayer();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void GoToNextPhase()
		{
			Game.GoToNextPhase();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void PerformAnotherRound()
		{
			Game.PerformAnotherRound();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangePlayerCapacity(ChangeEventArgs eventArgs)
		{
			if (!int.TryParse(eventArgs.Value?.ToString(), out int capacity))
			{
				Log.Error("Cannot convert specified value into a number.");
			}
			Game.MaximumPlayerCount = capacity;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeRoundCount(ChangeEventArgs eventArgs)
		{
			if (!int.TryParse(eventArgs.Value?.ToString(), out int maximumRoundCount))
			{
				Log.Error("Cannot convert specified value into a number.");
			}
			Game.MaximumRoundCount = maximumRoundCount;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeTimePerRound(ChangeEventArgs eventArgs)
		{
			if (!int.TryParse(eventArgs.Value?.ToString(), out int timePerRound))
			{
				Log.Error("Cannot convert specified value into a number.");
			}
			Game.TimePerRound = timePerRound;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ConcludeGame()
		{
			Game.End();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		#region Message Handling

		public async Task HandleAsync(GameChangedMessage message)
		{
			await Refresh();
		}

		public async Task HandleAsync(GameAddedMessage message)
		{
			await Refresh();
		}

		#endregion
	}
}