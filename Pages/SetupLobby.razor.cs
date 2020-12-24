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
		#nullable restore

#endregion

		[Parameter]
		public Guid Guid { get; set; }
		[Parameter]
		public string? Nickname { get; set; }

		public Game? Game { get; set; }
		public List<Deck>? AvailableDecks { get; set; }
		
		protected override async Task OnInitializedAsync()
		{
			if (this.Guid == Guid.Empty || string.IsNullOrWhiteSpace(Nickname))
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			Game = ServerStatus.STATUS.GetGameById(Guid);
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
				AvailableDecks = await context.GetDecksAsync();
				Game.SetDeckIfNull(AvailableDecks.First());
			}
		}

#region Events

		protected async void ChangeDeck(ChangeEventArgs e)
		{
			if (Game == null || AvailableDecks == null || e.Value == null)
			{
				return;
			}
			#nullable disable
			Guid id = new(e.Value.ToString());
			#nullable restore
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
			if (Game == null || !int.TryParse(eventArgs.Value?.ToString(), out int capacity))
			{
				return;
			}
			Game.MaximumPlayerCount = capacity;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeRoundCount(ChangeEventArgs eventArgs)
		{
			if (Game == null || !int.TryParse(eventArgs.Value?.ToString(), out int maximumRoundCount))
			{
				return;
			}
			Game.MaximumRoundCount = maximumRoundCount;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void ChangeTimePerRound(ChangeEventArgs eventArgs)
		{
			if (Game == null || !int.TryParse(eventArgs.Value?.ToString(), out int timePerRound))
			{
				return;
			}
			Game.TimePerRound = timePerRound;
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		protected async void StartGame()
		{
			if (Game == null)
			{
				return;
			}
			Game.Start();
			_navigationManager.NavigateTo($"/Game/{Game.Guid}/{Nickname}");
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
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
			if (Game == null)
			{
				return;
			}
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