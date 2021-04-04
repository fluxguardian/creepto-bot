using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StrategyTester.Exchanges
{
    public class BinanceDecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => decimal.Parse(reader.GetString(), NumberStyles.Any, new CultureInfo("en-US"));
        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
        
    }
}
