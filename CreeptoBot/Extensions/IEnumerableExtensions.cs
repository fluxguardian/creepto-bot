using System.Collections.Generic;
using System.Linq;

namespace StrategyTester.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> TakeRange<T>(this IEnumerable<T> enumerable, int start, int end)
            => enumerable.Skip(start + 1).Take(end - start);
    }
}
