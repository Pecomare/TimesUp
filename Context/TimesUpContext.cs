using Microsoft.EntityFrameworkCore;
using TimesUp.Data;

namespace TimesUp.Context
{
	public class TimesUpContext : DbContext
	{
		#nullable disable
		public DbSet<Deck> Decks { get; set; }
		public DbSet<Card> Cards { get; set; }
		#nullable enable

		public TimesUpContext(DbContextOptions<TimesUpContext> options) : base(options)
		{
			
		}
	}
}