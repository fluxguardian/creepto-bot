using System;

namespace StrategyTester.Extensions
{
    public static class MathExtensions
    {
        public static decimal PercentageDifference(decimal value, decimal value2)
            => Math.Round((value2 - value) / value * 100, 2);
    }
}
