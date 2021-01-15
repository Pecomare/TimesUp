using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
				SetupDatabase(new TimesUpContext(builder.Options), Configuration.GetSection("Connection")["DatabaseContentFile"]);
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

		private static async void SetupDatabase(TimesUpContext context, string setupFilePath)
		{
			await context.Database.EnsureDeletedAsync();
			await context.Database.EnsureCreatedAsync();

			if ((await context.GetDecksAsync()).Any())
			{
				return;
			}

			string jsonText = File.ReadAllText(setupFilePath);
			List<Deck>? decks = JsonSerializer.Deserialize<List<Deck>>(jsonText);

			if (decks == null)
			{
				throw new InvalidDataException($"No data found in file {setupFilePath}.");
			}

			foreach (Deck deck in decks)
			{
				context.Add(deck);
			}

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
