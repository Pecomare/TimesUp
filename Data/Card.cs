using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimesUp.Data
{
	[Table("Card")]
	public class Card
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Text { get; set; } = string.Empty;

		public Card()
		{
			
		}
		public Card(string text)
		{
			Text = text;
		}
	}
}