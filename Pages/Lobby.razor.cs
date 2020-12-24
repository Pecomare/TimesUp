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
	public class LobbyComponent : ComponentBase, IHandle<GameChangedMessage>
	{
		private static readonly List<Deck> EmptyDeckList = new();

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

		public Game Game { get; set; } = null!;

		#region Initialization

		protected override void OnInitialized()
		{
			if (!Guid.HasValue || string.IsNullOrWhiteSpace(Nickname) )
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			Game? game = ServerStatus.STATUS.GetGameById(Guid.Value);
			if (game == null)
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			Game = game;
			if (!Game.IsPlayerInGame(Nickname))
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			switch (Game.State)
			{
				case GameState.NEW:
					_navigationManager.NavigateTo($"/SetupGame/{Game.Guid}/{Nickname}");
					break;

				case GameState.ENDED:
					_navigationManager.NavigateTo($"/Results/{Game.Guid}/{Nickname}");
					break;
			}
			
			_eventAggregator.Subscribe(this);
		}

		#endregion

		public async Task Refresh()
		{
			await InvokeAsync(() => 
			{
				StateHasChanged();
			});
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

		protected async void ConcludeGame()
		{
			Game.End();
			await _eventAggregator.PublishAsync(new GameChangedMessage(Game));
		}

		#region Message Handling

		public async Task HandleAsync(GameChangedMessage message)
		{
			if (Game.State == GameState.ENDED)
			{
				_navigationManager.NavigateTo($"/Results/{Game.Guid}/{Nickname}");
				return;
			}
			await Refresh();
		}

		#endregion
	}
}