using System.Collections.Generic;
using System.Threading.Tasks;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TimesUp.Context;
using TimesUp.Data;

namespace TimesUp.Pages
{
	public class SearchComponent : ComponentBase, IHandle<GameChangedMessage>, IHandle<GameAddedMessage>
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

		public string Nickname { get; set; } = string.Empty;
		public string SearchText { get; set; } = string.Empty;
		public List<Game> Results { get; private set; }
		
		protected override void OnInitialized()
		{
			_eventAggregator.Subscribe(this);
			Results = ServerStatus.STATUS.OpenGames;
		}

#region Events

		public void SearchGame()
		{
			Results = ServerStatus.STATUS.OpenGames.FindAll(game => game.Name.Contains(SearchText));
		}

		protected async void JoinGame(Game game)
		{
			if (string.IsNullOrWhiteSpace(Nickname))
			{
				return;
			}
			game.AddPlayer(Nickname);
			await _eventAggregator.PublishAsync(new GameChangedMessage(game));
			string page = game.State switch
			{
				GameState.NEW => "SetupGame",
				GameState.RUNNING => "Game",
				GameState.ENDED => "Results",
				_ => "Error"
			};
			_navigationManager.NavigateTo($"/{page}/{game.Guid}/{Nickname}");
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
			await Refresh();
		}

		public async Task HandleAsync(GameAddedMessage message)
		{
			await Refresh();
		}

#endregion
	}
}