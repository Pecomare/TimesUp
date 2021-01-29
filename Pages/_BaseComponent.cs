using System.Threading.Tasks;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TimesUp.Context;

namespace TimesUp.Pages
{
	public class _BaseComponent : ComponentBase
	{
		#region Injections

#nullable disable
		[Inject]
		protected IEventAggregator _eventAggregator { get; set; }
		[Inject]
		protected NavigationManager _navigationManager { get; set; }
		[Inject]
		protected IDbContextFactory<TimesUpContext> DbFactory { get; set; }
#nullable restore

		#endregion

		protected async Task Refresh()
		{
			await InvokeAsync(() =>
			{
				StateHasChanged();
			});
		}
	}
}