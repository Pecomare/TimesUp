using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesUp.Data
{
	[Table("Deck")]
	public class Deck
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public List<Card> Cards { get; set; } = new();

		public Deck()
		{
			
		}

		public Deck(string name)
		{
			Name = name;
		}
	}
}