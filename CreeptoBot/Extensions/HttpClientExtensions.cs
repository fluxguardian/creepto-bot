using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StrategyTester.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string requestUri, object body)
        {
            var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUri, content);
        }

        public static async Task<T> DeserializeFromJson<T>(this HttpResponseMessage message)
        {
            var json = await message.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
