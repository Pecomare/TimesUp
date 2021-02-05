using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TimesUp.Context;
using TimesUp.Data;
using TimesUp.Shared;

namespace TimesUp.Pages
{
	public class EditDeckComponent : _BaseComponent
	{
#nullable disable
		protected Popup SavePopup { get; set; }
		private TimesUpContext Context { get; set; }
#nullable restore
		protected Deck? Deck { get; set; }

		[Parameter]
		public Guid Guid { get; set; }

		protected override async Task OnInitializedAsync()
		{
			Context = DbFactory.CreateDbContext();
			Deck = await Context.GetDeckAsync(Guid);
			if (Deck == null)
			{
				Deck = new Deck();
			}
		}

		protected void RemoveCard(Card card)
		{
			if (Deck == null)
			{
				return;
			}
			Deck.RemoveCard(card);
		}

		protected void CreateCard()
		{
			if (Deck == null)
			{
				return;
			}
			Card card = new();
			Deck.AddCard(card);
		}

		protected async void SaveDeck()
		{
			if (Deck == null)
			{
				return;
			}
			
			if (Context.GetDeck(Deck.Id) == null)
			{
				Context.Add(Deck);
			}
			else
			{
				Context.Entry(Deck).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				Context.Update(Deck);
			}
			Context.SaveChanges();

			SavePopup.ShowPopup("", Popup.PopupSeverity.Success);

			await _eventAggregator.PublishAsync(new DeckAddedMessage(Deck));
		}

		protected void Quit()
		{
			_navigationManager.NavigateTo("/EditDeckList");
		}
	}
}