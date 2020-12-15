using System;
using System.Collections.Generic;
using System.Linq;

namespace TimesUp.Extensions
{
	public static class Extensions
	{
		public static ICollection<T> Shuffle<T>(this ICollection<T> collection)
		{
			Random random = new();
			return collection.OrderBy(c => random.Next()).ToList();
		}
	}
}