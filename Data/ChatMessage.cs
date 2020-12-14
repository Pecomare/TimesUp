using System;

namespace TimesUp.Data
{
	public record ChatMessage (DateTime PostDateTime, string Owner, string Message);
}