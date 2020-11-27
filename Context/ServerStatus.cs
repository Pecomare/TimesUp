using System;
using System.Collections.Generic;
using System.Linq;
using TimesUp.Data;

namespace TimesUp
{
	public class ServerStatus
	{
		private static ServerStatus? STATUS { get; set; }
		public static ServerStatus GetServerStatus() => STATUS ?? (STATUS = new ServerStatus());

		private List<Game> Games { get; set; } = new List<Game>();

		public List<Game> OpenGames => Games.FindAll(Game => Game.State != GameState.ENDED);
		public List<Game> RunningGames => Games.FindAll(game => game.State == GameState.RUNNING);

		public Game? GetGameById(Guid guid) => Games.FirstOrDefault(game => game.Guid == guid);

		public Game CreateGame(string name)
		{
			Game game = new(name);
			Games.Add(game);
			return game;
		}

		public bool RemoveGameById(Guid guid) => RemoveAllGamesById(guid) > 0;

		public int RemoveAllGamesById(Guid guid) => Games.RemoveAll(game => game.Guid == guid);
	}
}