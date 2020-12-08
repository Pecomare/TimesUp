using System;
using System.Collections.Generic;
using System.Linq;
using TimesUp.Data;

namespace TimesUp
{
	public class ServerStatus
	{
		public static ServerStatus STATUS { get; set; } = new ServerStatus();

		public List<Game> Games { get; private set; } = new List<Game>();

		public List<Game> OpenGames => Games.FindAll(Game => Game.State != GameState.ENDED);
		public List<Game> RunningGames => Games.FindAll(game => game.State == GameState.RUNNING);

		public Game? GetGameById(Guid guid) => Games.FirstOrDefault(game => game.Guid == guid);

		public Game CreateGame(string name)
		{
			Game game = new(name);
			Games.Add(game);
			return game;
		}

		public Game CreateGame(string name, string owner)
		{
			Game game = CreateGame(name);
			game.SetOwner(owner);
			game.AddPlayer(owner);
			return game;
		}
	}
}