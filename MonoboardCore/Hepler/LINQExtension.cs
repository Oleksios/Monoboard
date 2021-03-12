using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoboardCore.Hepler
{
	public static class LINQExtension
	{
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new();
			foreach (var element in source)
				if (seenKeys.Add(keySelector(element)))
					yield return element;
		}
	}
}
