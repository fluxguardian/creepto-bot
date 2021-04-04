using System.Text.Json;

namespace StrategyTester.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToJsonString(this object value)
            => JsonSerializer.Serialize(value);
    }
}
