using System;
using System.Collections.Generic;
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
            switch (Configuration.GetSection("Connection")["DatabaseType"])
            {
				case "InMemory":
					services.AddDbContextFactory<TimesUpContext>(options =>
						options.UseInMemoryDatabase("InMemory"));
					SetupInMemoryDatabase();
					break;
                case "SqlServer":
                    services.AddDbContextFactory<TimesUpContext>(options =>
				        options.UseSqlServer(Configuration.GetConnectionString("SqlServerDatabase")));
                    break;

                case "Postgres":
                    services.AddDbContextFactory<TimesUpContext>(options =>
				        options.UseNpgsql(Configuration.GetConnectionString("PostgresDatabase")));
                    break;
                
                case "Sqlite":
                    services.AddDbContextFactory<TimesUpContext>(options =>
				        options.UseSqlite(Configuration.GetConnectionString("SqliteDatabase")));
                    break;
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

		private void SetupInMemoryDatabase()
		{
			TimesUpContext context = new TimesUpContext(new DbContextOptionsBuilder<TimesUpContext>()
				.UseInMemoryDatabase("InMemory")
				.Options);

			// Decks
			var deckAnime = new Deck("Anime");
            var deckJdg = new Deck("Joueur du Grenier");
			context.AddRange(deckAnime, deckJdg);

			// Cards / Anime
			context.Add(new Card(deckAnime, "Toaru Kagaku no Railgun (A Certain Scientific Railgun)"));
            context.Add(new Card(deckAnime, "Toaru Majutsu no Index (A Certain Magical Index)"));
            context.Add(new Card(deckAnime, "BanG Dream!"));
            context.Add(new Card(deckAnime, "Love Live!"));
            context.Add(new Card(deckAnime, "The iDOLM@STER"));
            context.Add(new Card(deckAnime, "Gochuumon wa Usagi desuka ? (Is the Order a Rabbit ?)"));
            context.Add(new Card(deckAnime, "Lucky Star"));
            context.Add(new Card(deckAnime, "CLANNAD"));
            context.Add(new Card(deckAnime, "Angel Beats"));
            context.Add(new Card(deckAnime, "Charlotte"));
            context.Add(new Card(deckAnime, "Kamisama ni Natta Hi (The Day I Became a God)"));

            // Cards / Joueur du Grenier
            context.Add(new Card(deckJdg, "Fisti"));
            context.Add(new Card(deckJdg, "Pepito"));
            context.Add(new Card(deckJdg, "David Goodenough"));
            context.Add(new Card(deckJdg, "Alpha V"));
            context.Add(new Card(deckJdg, "Jean-Michel Bruitage"));
            context.Add(new Card(deckJdg, "Albus HumbleBundleDore"));
            context.Add(new Card(deckJdg, "Henri PotDeBeurre"));
            context.Add(new Card(deckJdg, "Georges Tusaisqui"));
            context.Add(new Card(deckJdg, "Frangipanus"));
            context.Add(new Card(deckJdg, "Enfant de juron"));
            context.Add(new Card(deckJdg, "Jean-Michel Hardfist"));
            context.Add(new Card(deckJdg, "Mondo Cinematic Universe"));
            context.Add(new Card(deckJdg, "Gelganox"));
            context.Add(new Card(deckJdg, "Shinwa"));
            context.Add(new Card(deckJdg, "Archibald von Grenier"));

			context.SaveChanges();
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
