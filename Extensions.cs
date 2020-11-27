using System;
using System.Collections.Generic;
using System.Linq;

namespace TimesUp.Extensions
{
	public static class Extensions
	{
		public static List<T> Shuffle<T>(this List<T> list)
		{
			Random random = new();
			return list.OrderBy(c => random.Next()).ToList();
		}
	}
}