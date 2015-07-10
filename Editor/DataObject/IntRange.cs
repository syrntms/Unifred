using System;
using System.Linq;
using System.Collections.Generic;

namespace Unifred
{
	public class IntRange
	{
		public int from = 0;
		public int to = 0;

		public static IEnumerable<int> Split(IEnumerable<IntRange> range_list)
		{
			List<int> result = new List<int>();
			range_list.ForEach(
				range => {
					int min = Math.Min(range.from, range.to);
					int max = Math.Max(range.from, range.to);
					Enumerable.Range(min, max - min + 1).ForEach(num => result.Add(num));
				}
			);
			return result.Distinct();
		}

		public static IEnumerable<IntRange> Build(IEnumerable<int> source)
		{
			IEnumerable<int> sorted = source.OrderBy((item) => {return item;});
			List<IntRange> result = new List<IntRange>();
			IntRange latest = null;

			foreach (var num in sorted) {
				if (latest != null) {
					if (latest.to + 1 == num) {
						latest.to = num;
						continue;
					}
				}

				latest = new IntRange();
				latest.from = latest.to = num;
				result.Add(latest);
			}
			return result;
		}
	}
}
