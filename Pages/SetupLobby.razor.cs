using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TimesUp.Context;
using TimesUp.Data;

namespace TimesUp.Pages
{
	public class SetupLobbyComponent : ComponentBase, IHandle<GameChangedMessage>
	{

#region Injections

		#nullable disable
		[Inject]
		private IEventAggregator _eventAggregator { get; set; }
		[Inject]
		private NavigationManager _navigationManager { get; set; }
		[Inject]
		private IDbContextFactory<TimesUpContext> DbFactory { get; set; }
		#nullable enable

#endregion

		[Parameter]
		public Guid? Guid { get; set; }
		[Parameter]
		public string? Nickname { get; set; }

		public Game? Game { get; set; }
		public List<Deck>? AvailableDecks { get; set; }
		
		protected override void OnInitialized()
		{
			if (Guid == null || string.IsNullOrWhiteSpace(Nickname))
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			Game = ServerStatus.STATUS.GetGameById(Guid.Value);
			if (Game == null)
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			if (!Game.IsPlayerInGame(Nickname))
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			switch (Game.State)
			{
				case GameState.RUNNING:
					_navigationManager.NavigateTo($"/Game/{Game.Guid}/{Nickname}");
					return;

				case GameState.ENDED:
					_navigationManager.NavigateTo($"/Results/{Game.Guid}/{Nickname}");
					return;
			}

			_eventAggregator.Subscribe(this);
			
			if (!string.Equals(Nickname, Game.Owner))
			{
				return;
			}
			using (TimesUpContext context = DbFactory.CreateDbContext())
			{
				AvailableDecks = context.Decks.Include(deck => deck.Cards).ToList();
				Game.SetDeckIfNull(AvailableDecks.First());
			}
		}

#region Events

		protected async void ChangeDeck(ChangeEventArgs e)
		{
			if (!int.TryParse(e.Value?.ToString(), out int id))
			{
				return;
			}
			Deck? deck= AvailableDecks.Find(deck => deck.Id == id);
			if (deck == null)
			{
				return;
			}
			Game.SetDeck(deck);
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangePlayerCapacity(ChangeEventArgs eventArgs)
		{
			if (!int.TryParse(eventArgs.Value?.ToString(), out int capacity))
			{
				return;
			}
			Game.MaximumPlayerCount = capacity;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeRoundCount(ChangeEventArgs eventArgs)
		{
			if (!int.TryParse(eventArgs.Value?.ToString(), out int maximumRoundCount))
			{
				return;
			}
			Game.MaximumRoundCount = maximumRoundCount;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeTimePerRound(ChangeEventArgs eventArgs)
		{
			if (!int.TryParse(eventArgs.Value?.ToString(), out int timePerRound))
			{
				return;
			}
			Game.TimePerRound = timePerRound;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void StartGame()
		{
			Game.Start();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
			_navigationManager.NavigateTo($"/Game/{Game.Guid}/{Nickname}");
		}

		public async Task Refresh()
		{
			await InvokeAsync(() => 
			{
				StateHasChanged();
			});
		}

		public async Task HandleAsync(GameChangedMessage message)
		{
			if (Game.State == GameState.RUNNING)
			{
				_navigationManager.NavigateTo($"/Game/{Game.Guid}/{Nickname}");
				return;
			}
			await Refresh();
		}

#endregion
	}
}