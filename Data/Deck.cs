using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesUp.Data
{
	[Table("Deck")]
	public class Deck
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; } = string.Empty;
		public ICollection<Card> Cards { get; set; } = new List<Card>();

		public Deck()
		{
			
		}

		public Deck(string name)
		{
			Name = name;
		}
	}
}