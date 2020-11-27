using TimesUp.Data;

namespace TimesUp.Context
{
	public class GameAddedMessage
	{
		public Game Game { get; set; }

		public GameAddedMessage(Game game)
		{
			Game = game;
		}
	}
}