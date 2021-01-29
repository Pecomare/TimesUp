using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TimesUp.Context;
using TimesUp.Data;

namespace TimesUp.Pages
{
	public class EditDeckComponent : _BaseComponent
	{
		#nullable disable
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

		protected void SaveDeck()
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

			_eventAggregator.PublishAsync(new DeckAddedMessage(Deck));
		}

		protected void Cancel()
		{
			_navigationManager.NavigateTo("/EditDeckList");
		}
	}
}