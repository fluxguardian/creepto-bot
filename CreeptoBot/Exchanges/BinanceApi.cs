using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrategyTester.Extensions;
using StrategyTester.TechnicalAnalysis;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyTester.Exchanges
{
    public class BinanceApi
    {
        private readonly string _apiAddress;
        private readonly string _apiSecret;
        private readonly HttpClient _httpClient;
        private readonly ClientWebSocket _webSocketClient;
        private readonly ILogger<BinanceApi> _logger;
        private readonly IDictionary<BinanceKLineSubscription, EventHandler<BinanceKLineEventArgs>> _eventDictionary;
        //public event EventHandler<BinanceKLineEventArgs> OnKline;

        public BinanceApi(IOptions<BinanceSettings> options, ILogger<BinanceApi> logger)
        {
            _apiAddress = options.Value.ApiAddress;
            _apiSecret = options.Value.ApiSecret;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_apiAddress)
            };
            _webSocketClient = new ClientWebSocket();
            _logger = logger;
            _eventDictionary = new ConcurrentDictionary<BinanceKLineSubscription, EventHandler<BinanceKLineEventArgs>>();
        }

        public async Task ConnectToWebSockets(BinanceKLineSubscription subscription, EventHandler<BinanceKLineEventArgs> eventHandler, CancellationToken cancellationToken)
        {
            await _webSocketClient.ConnectAsync(new Uri($"wss://stream.binance.com:9443/ws/{subscription}"), cancellationToken);
            //var request = new BinanceWebSocketRequest()
            //{
            //    Id = 1,
            //    Parameters = new string[]
            //    {
            //        subscription.ToString()
            //    },
            //    Method = "SUBSCRIBE"
            //}.ToJsonString();
            _eventDictionary[subscription] = eventHandler;

            ReceiveAsync(cancellationToken);

            //await _webSocketClient.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(request)), WebSocketMessageType.Text, false, cancellationToken);
        }

        public async Task UnsubscribeToKLineWebSocket(BinanceKLineSubscription subscription)
        {

        }

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            using IMemoryOwner<byte> memory = MemoryPool<byte>.Shared.Rent(1024 * 4);

            var jsonOpts = new JsonSerializerOptions();
            jsonOpts.Converters.Add(new BinanceDecimalJsonConverter());

            while (_webSocketClient.State != WebSocketState.Closed && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"\nWaiting websockets messages");

                var receiveResult = await _webSocketClient.ReceiveAsync(memory.Memory, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    if (_webSocketClient.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation($"\nAcknowledging Close frame received from server");
                        await _webSocketClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                    }

                    if (_webSocketClient.State == WebSocketState.Open && receiveResult.MessageType != WebSocketMessageType.Close)
                    {
                        _logger.LogDebug("Kline received");

                        var kLine = JsonSerializer.Deserialize<BinanceKlineResponse>(memory.Memory.Span.Slice(0, receiveResult.Count), jsonOpts);
                        var subscription = new BinanceKLineSubscription()
                        {
                            Interval = kLine.KLine.Internal,
                            Symbol = kLine.Symbol.ToLowerInvariant()
                        };

                        var eventHandler = _eventDictionary[subscription];
                        eventHandler.Invoke(this, new BinanceKLineEventArgs()
                        {
                            KLine = kLine
                        });
                    }
                }
            }
        }

        public async Task<IEnumerable<Candle>> GetCandles(string market, string interval, int limit = 1000)
            => await GetCandles(market, interval, null, null, limit);

        public async Task<IEnumerable<Candle>> GetCandles(string market, string interval, DateTime? startTime = null, DateTime? endTime = null, int limit = 1000)
        {
            var url = $"/api/v3/klines?symbol={market.ToUpperInvariant()}&interval={interval}&limit={limit}";

            // build url
            url += startTime.HasValue ? $"startTime={startTime.Value.ToEpochTime()}" : string.Empty;
            url += endTime.HasValue ? $"endTime={endTime.Value.ToEpochTime()}" : string.Empty;

            var response = await _httpClient.GetAsync(url);

            var json = JsonSerializer.Deserialize<JsonElement[][]>(await response.Content.ReadAsStringAsync());

            return json.Select(s => new Candle()
            {
                OpenTime = (long)s[0].GetDouble(),
                Open = decimal.Parse(s[1].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                High = decimal.Parse(s[2].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                Low = decimal.Parse(s[3].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                Close = decimal.Parse(s[4].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                Volume = decimal.Parse(s[5].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                CloseTime = (long)s[6].GetDouble(),
                QuoteAssetVolume = decimal.Parse(s[7].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                NumberOfTrades = s[8].GetInt32(),
                TakerBuyBaseAssetVolume = decimal.Parse(s[9].GetString(), NumberStyles.Any, new CultureInfo("en-US")),
                TakerBuyQuoteAssetVolume = decimal.Parse(s[10].GetString(), NumberStyles.Any, new CultureInfo("en-US"))
            }).ToArray();
        }
    }
}
