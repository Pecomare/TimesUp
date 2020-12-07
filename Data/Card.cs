using System.ComponentModel.DataAnnotations.Schema;

namespace TimesUp.Data
{
	[Table("Card")]
	public class Card
	{
		public int Id { get; set; }
		public string Text1 { get; set; }
		public string Text2 { get; set; }
		public Deck Deck { get; set; }
	}
}