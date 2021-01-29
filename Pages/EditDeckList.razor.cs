using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;
using TimesUp.Context;
using TimesUp.Data;

namespace TimesUp.Pages
{
	public class EditDeckListComponent : _BaseComponent, IHandle<DeckAddedMessage>
	{
		protected List<Deck> Decks { get; set; } = new();

		protected override async Task OnInitializedAsync()
		{
			using TimesUpContext context = DbFactory.CreateDbContext();
			Decks = await context.GetDecksAsync();
		}

		protected void CreateDeck()
		{
			_navigationManager.NavigateTo($"/EditDeck/{Guid.NewGuid()}");
			return;
		}

		public async Task HandleAsync(DeckAddedMessage message)
		{
			await Refresh();
		}
	}
}