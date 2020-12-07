using System;
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
        }

		private void SetupInMemoryDatabase()
		{
			// TODO
			TimesUpContext context = new TimesUpContext(new DbContextOptionsBuilder<TimesUpContext>()
				.UseInMemoryDatabase("InMemory")
				.Options);

			// Decks
			var deck1 = new Deck { Name = "Deck 1" };
			var deck2 = new Deck { Name = "Deck 2" };
			context.Add(deck1);
			context.Add(deck2);

			// Cards
			context.Add(new Card { Text1 = "Card 1 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 2 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 3 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 4 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 5 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 6 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 7 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 8 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 9 / Text 1", Deck = deck1 });
			context.Add(new Card { Text1 = "Card 10 / Text 1", Deck = deck1 });

			context.SaveChanges();
		}
    }
}
