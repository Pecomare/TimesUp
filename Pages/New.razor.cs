using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TimesUp.Context;

namespace TimesUp.Pages
{
	public class NewComponent : ComponentBase
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
	
		public string Nickname { get; set; }
		public string GameName { get; set; }

		protected override void OnInitialized()
		{
			_eventAggregator.Subscribe(this);
		}

		protected async void CreateGame()
		{
			if (string.IsNullOrWhiteSpace(Nickname) || string.IsNullOrWhiteSpace(GameName))
			{
				return;
			}
			Data.Game game = ServerStatus.STATUS.CreateGame(GameName, Nickname);
			_navigationManager.NavigateTo($"/SetupGame/{game.Guid}/{Nickname}");
			await _eventAggregator.PublishAsync(new GameAddedMessage(game));
		}
	}
}