using TimesUp.Data;

namespace TimesUp.Context
{
	public class GameChangedMessage
	{
		public Game Game { get; set; }

		public GameChangedMessage(Game game)
		{
			Game = game;
		}
	}
}