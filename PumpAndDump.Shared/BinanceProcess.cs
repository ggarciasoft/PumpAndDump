using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace PumpAndDump.Shared
{
    public class BinanceProcess : BaseProcess, IProcess, IDisposable
    {
        WebsocketClient _client;

        public BinanceProcess()
        {
            Endpoint = "wss://stream.binance.com:9443/ws/!bookTicker";
        }

        public override async Task<bool> StartProcess()
        {
            await _client.Start();
            return _client.IsRunning;
        }
        public override async Task EndProcess()
        {
            await _client.Stop(System.Net.WebSockets.WebSocketCloseStatus.Empty, "");
        }

        public override async Task LoadProcess(Action<string> showLogAction)
        {
            ShowLogAction = showLogAction;
            var dataJson = $"Binance.json";
            var path = Path.Combine(AppContext.BaseDirectory, dataJson);
            var fileData = await File.ReadAllTextAsync(path);
            var deserialize = System.Text.Json.JsonSerializer.Deserialize<BinanceExchangeInformation>(fileData);
            var currentDate = DateTime.Now;
            foreach (var item in deserialize.Symbols.Where(o => o.IsMarginTradingAllowed && o.QuoteAsset == "BTC" && o.Status == "TRADING"))
                CompareMarketDic.TryAdd(item.Symbol, new CompareMarketInformation()
                {
                    Symbol = item.Symbol,
                    CheckingTime = currentDate
                });

            _client = new WebsocketClient(new Uri(Endpoint))
            {
                ReconnectTimeout = TimeSpan.FromSeconds(30)
            };

            _client.ReconnectionHappened.Subscribe(info => showLogAction($"\n Reconnection happened, type: {info.Type}"));

            _client
              .MessageReceived
              .Where(o => o.Text == "ping frame")
              .ObserveOn(TaskPoolScheduler.Default)
              .Subscribe(o => _client.Send("pong frame"));

            _client
                .MessageReceived
                .Where(o => o.Text.Contains("BTC\""))
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(o => ProcessMessage<BinanceMarketInformation>(o.Text));
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
