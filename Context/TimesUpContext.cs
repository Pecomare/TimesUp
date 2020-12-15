using System.Collections.Generic;
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
	}
}