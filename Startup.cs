using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TimesUp.Context;
using TimesUp.Data;

namespace TimesUp
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
			DbContextOptionsBuilder<TimesUpContext> builder = new();
            switch (Configuration.GetSection("Connection")["DatabaseType"])
            {
				case "InMemory":
					services.AddDbContextFactory<TimesUpContext>(options =>
						options.UseInMemoryDatabase("InMemory"));
					builder.UseInMemoryDatabase("InMemory");
					break;
                case "SqlServer":
                    services.AddDbContextFactory<TimesUpContext>(options =>
				        options.UseSqlServer(Configuration.GetConnectionString("SqlServerDatabase")));
					builder.UseSqlServer(Configuration.GetConnectionString("SqlServerDatabase"));
                    break;

                case "Postgres":
                    services.AddDbContextFactory<TimesUpContext>(options =>
				        options.UseNpgsql(Configuration.GetConnectionString("PostgresDatabase")));
					builder.UseNpgsql(Configuration.GetConnectionString("PostgresDatabase"));
                    break;
                
                case "Sqlite":
                    services.AddDbContextFactory<TimesUpContext>(options =>
				        options.UseSqlite(Configuration.GetConnectionString("SqliteDatabase")));
					builder.UseSqlite(Configuration.GetConnectionString("SqliteDatabase"));
                    break;

				case "CosmosDb":
					services.AddDbContextFactory<TimesUpContext>(options =>
						options.UseCosmos(
							Configuration.GetConnectionString("CosmosDbEndpoint")
							, Configuration.GetConnectionString("CosmosDbAccountKey")
							, Configuration.GetConnectionString("CosmosDbDatabase")));
					builder.UseCosmos(
						Configuration.GetConnectionString("CosmosDbEndpoint")
						, Configuration.GetConnectionString("CosmosDbAccountKey")
						, Configuration.GetConnectionString("CosmosDbDatabase"));
					break;

				default:
					throw new ArgumentOutOfRangeException(
						"DatabaseType invalid. Please specify one of these: InMemory, SqlServer, Postgres, Sqlite, CosmosDb");
            }

			if (bool.TryParse(Configuration.GetSection("Connection")["ResetDatabase"], out bool reset) && reset)
			{
				SetupDatabase(new TimesUpContext(builder.Options));
			}
			
			services.AddEventAggregator();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

			ConfigureGameCleaning();
        }

		private async void SetupDatabase(TimesUpContext context)
		{
			await context.Database.EnsureDeletedAsync();
			await context.Database.EnsureCreatedAsync();

			if ((await context.GetDecksAsync()).Any())
			{
				return;
			}

			// Decks
			var deckAnime = new Deck("Anime");
            var deckJdg = new Deck("Joueur du Grenier");

			deckAnime.Cards = new List<Card>
			{
				new Card("Toaru Kagaku no Railgun (A Certain Scientific Railgun)")
				, new Card("Toaru Majutsu no Index (A Certain Magical Index)")
				, new Card("BanG Dream!")
				, new Card("Love Live!")
				, new Card("The iDOLM@STER")
				, new Card("Gochuumon wa Usagi desuka ? (Is the Order a Rabbit ?)")
				, new Card("Lucky Star")
				, new Card("CLANNAD")
				, new Card("Angel Beats")
				, new Card("Charlotte")
				, new Card("Kamisama ni Natta Hi (The Day I Became a God)")
				, new Card("D4DJ: First Mix")
			};
			deckJdg.Cards = new List<Card>
			{
				new Card("Fisti")
				, new Card("Pepito")
				, new Card("David Goodenough")
				, new Card("Alpha V")
				, new Card("Jean-Michel Bruitage")
				, new Card("Albus HumbleBundleDore")
				, new Card("Henri PotDeBeurre")
				, new Card("Georges Tusaisqui")
				, new Card("Frangipanus")
				, new Card("Enfant de juron")
				, new Card("Jean-Michel Hardfist")
				, new Card("Mondo Cinematic Universe")
				, new Card("Gelganox")
				, new Card("Shinwa")
				, new Card("Archibald von Grenier")
			};

			context.AddRange(deckAnime, deckJdg);

			await context.SaveChangesAsync();
		}

		private void ConfigureGameCleaning()
		{
			Task.Run(async () => 
			{
				ServerStatus status = ServerStatus.STATUS;
				int removedGameCount;
				int interval = Configuration.GetValue<int>("GameCleaningInterval");
				int delayBeforeCleanup = Configuration.GetValue<int>("GameDelayBeforeCleanup");
				DateTime loopStart;
				int loopDuration;
				while (true)
				{
					loopStart = DateTime.Now;
					Console.WriteLine("Removing inactive games.");
					removedGameCount = status.Games.RemoveAll(game => 
						(loopStart - game.ModifiedDateTime).TotalMilliseconds >= delayBeforeCleanup);
					Console.WriteLine($"Removed {removedGameCount} inactive games.");
					loopDuration = (int)(DateTime.Now - loopStart).TotalMilliseconds;
					await Task.Delay(interval - loopDuration);
				}
			});
		}
    }
}
