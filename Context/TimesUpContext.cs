using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimesUp.Data;

namespace TimesUp.Context
{
	public class TimesUpContext : DbContext
	{
		#nullable disable
		private DbSet<Deck> Decks { get; set; }
		private DbSet<Card> Cards { get; set; }
		#nullable enable

		public TimesUpContext(DbContextOptions<TimesUpContext> options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Deck>().ToContainer("TimesUpContext");
			modelBuilder.Entity<Deck>().OwnsMany(deck => deck.Cards);
			base.OnModelCreating(modelBuilder);
		}

		public List<Deck> GetDecks()
		{
			return Database.IsCosmos()
				? Decks.ToListAsync().Result
				: Decks.Include(deck => deck.Cards).ToListAsync().Result;
		}

		public async Task<List<Deck>> GetDecksAsync()
		{
			return await (Database.IsCosmos()
				? Decks.ToListAsync()
				: Decks.Include(deck => deck.Cards).ToListAsync());
		}

		public Deck? GetDeck(Guid guid)
		{
			return Database.IsCosmos()
				? Decks.FirstOrDefault(deck => deck.Id == guid)
				: Decks.Include(deck => deck.Cards).FirstOrDefault(deck => deck.Id == guid);
		}

		public async Task<Deck> GetDeckAsync(Guid guid)
		{
			return await (Database.IsCosmos()
				? Decks.FirstOrDefaultAsync(deck => deck.Id == guid)
				: Decks.Include(deck => deck.Cards).FirstOrDefaultAsync(deck => deck.Id == guid));
		}
	}
}