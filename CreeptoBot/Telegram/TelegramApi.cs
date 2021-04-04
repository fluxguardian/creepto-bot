using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrategyTester.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTester
{
    public sealed class TelegramApi : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _botToken;
        private readonly string _groupId;
        private readonly ILogger<TelegramApi> _logger;
        private volatile bool _isReceiving;

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public TelegramApi(IOptions<TelegramApiSettings> settingsOption, ILogger<TelegramApi> logger)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.telegram.org/")
            };
            _botToken = settingsOption.Value?.Token;
            _groupId = settingsOption.Value?.GroupId;
            _logger = logger;
        }

        public void StartReceivingMessages(CancellationToken cancellationToken = default, EventHandler<MessageReceivedEventArgs> onMessage = null)
        {
            if (!_isReceiving)
            {
                var updatesTokenSource = new CancellationTokenSource();

                cancellationToken.Register(() => updatesTokenSource.Cancel());

                OnMessageReceived += onMessage;

                GetUpdatesAsync(updatesTokenSource.Token);
            }
        }

        public async Task GetUpdatesAsync(CancellationToken token)
        {
            _isReceiving = true;
            GetUpdatesRequest getUpdates = new GetUpdatesRequest()
            {
                Offset = 0,
                Timeout = (int)TimeSpan.FromMinutes(1).TotalSeconds,
                AllowedUpdates = new string[] { "message" }
            };

            while (!token.IsCancellationRequested)
            {
                using var response = await _client.PostJsonAsync($"/bot{_botToken}/getUpdates", getUpdates);
                var updates = await response.DeserializeFromJson<TelegramResponse<IReadOnlyList<Update>>>();
                _logger.LogDebug($"Telegram got {updates.Result?.Count} updates");
                if(updates.Result == null)
                {
                    continue;
                }

                foreach(var u in updates.Result)
                {
                    OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs()
                    {
                        Message = u.Message
                    });
                    getUpdates.Offset = u.UpdateId + 1;
                }
            }
            _isReceiving = false;
        }

        public async Task SendMessageAsync(string message)
            => await _client.GetAsync($"/bot{_botToken}/sendMessage?chat_id={_groupId}&text={message}");

        public void Dispose()
            => _client.Dispose();
    }

    internal class TelegramResponse<T>
    {
        [JsonPropertyName("result")]
        public T Result { get; set; }
    }

    internal record Update
    {
        [JsonPropertyName("update_id")]
        public int UpdateId { get; init; }

        [JsonPropertyName("message")]
        public Message Message { get; init; }
    }

    public record Message
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; }
    }

    internal class GetUpdatesRequest
    {
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 100;

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }

        [JsonPropertyName("allowed_updates")]
        public IEnumerable<string> AllowedUpdates { get; set; }
    }
}
