using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using TimesUp.Data;

namespace TimesUp.Pages
{
	public class ResultsComponent : ComponentBase
	{
		#nullable disable
		[Inject]
		private NavigationManager _navigationManager { get; set; }
		#nullable enable
		
		[Parameter]
		public Guid? Guid { get; set; }
		[Parameter]
		public string? Nickname { get; set; }

		public Game? Game { get; set; }

		protected override void OnParametersSet()
		{
			if (!Guid.HasValue
				|| string.IsNullOrWhiteSpace(Nickname) )
			{
				_navigationManager.NavigateTo("/Search");
				return;
			}
			Game = ServerStatus.STATUS.GetGameById(Guid.Value);
			if (Game == null 
				|| !Game.Players.Any(player => string.Equals(player.Name, Nickname)))
			{
				_navigationManager.NavigateTo("/Search");
			}
		}
	}
}