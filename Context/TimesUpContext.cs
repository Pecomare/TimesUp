using Microsoft.EntityFrameworkCore;
using TimesUp.Data;

namespace TimesUp.Context
{
	public class TimesUpContext : DbContext
	{
		public DbSet<Deck> Decks { get; set; }
		public DbSet<Card> Cards { get; set; }

		public TimesUpContext(DbContextOptions<TimesUpContext> options) : base(options)
		{
			
		}
	}
}