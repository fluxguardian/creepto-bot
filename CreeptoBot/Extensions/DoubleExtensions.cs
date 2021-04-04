using System;

namespace StrategyTester.Extensions
{
    public static class DoubleExtensions
    {
        public static decimal Round(this decimal value, int decimals)
            => Math.Round(value, decimals);
    }
}
