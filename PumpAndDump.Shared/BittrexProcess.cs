using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace PumpAndDump.Shared
{
    public class BittrexProcess : BaseProcess, IProcess, IDisposable
    {
        const string API_KEY = "";
        const string API_SECRET = "";
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;

        public BittrexProcess()
        {
            Endpoint = "https://socket-v3.bittrex.com/signalr";
        }

        public override async Task<bool> StartProcess()
        {
            await _hubConnection.Start();

            var channels = new string[] {
                "heartbeat",
                "market_summaries" };
            var response = await Subscribe(channels);
            for (int i = 0; i < channels.Length; i++)
            {
                ShowLogAction(response[i].Success ? $"{channels[i]}: Success" : $"{channels[i]}: {response[i].ErrorCode}");
            }
            return _hubConnection.State == ConnectionState.Connected;
        }
        public override async Task EndProcess()
        {
            _hubConnection.Stop();
        }

        public override async Task LoadProcess(Action<string> showLogAction)
        {
            ShowLogAction = showLogAction;
            var dataJson = $"Bittrex.json";
            var path = Path.Combine(AppContext.BaseDirectory, dataJson);
            var fileData = await File.ReadAllTextAsync(path);
            var deserialize = System.Text.Json.JsonSerializer.Deserialize<BittrexSymbolInformation[]>(fileData);
            var currentDate = DateTime.Now;
            foreach (var item in deserialize.Where(o => o.QuoteCurrencySymbol == "BTC" && o.Status == "ONLINE"))
                CompareMarketDic.TryAdd(item.Symbol, new CompareMarketInformation()
                {
                    Symbol = item.Symbol,
                    CheckingTime = currentDate
                });

            _hubConnection = new HubConnection(Endpoint);
            _hubProxy = _hubConnection.CreateHubProxy("c3");
            SetHeartbeatHandler(() => showLogAction("<heartbeat>"));
            AddMessageHandler<string>("market_summaries", msg =>
            {
                ProcessMessage<BinanceMarketInformation>(msg);
            });
        }

        private async Task<SocketResponse> Authenticate(string apiKey, string apiKeySecret)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var randomContent = $"{ Guid.NewGuid() }";
            var content = string.Join("", timestamp, randomContent);
            var signedContent = CreateSignature(apiKeySecret, content);
            var result = await _hubProxy.Invoke<SocketResponse>(
                "Authenticate",
                apiKey,
                timestamp,
                randomContent,
                signedContent);
            return result;
        }

        private IDisposable AddMessageHandler<Tmessage>(string messageName, Action<Tmessage> handler)
        {
            return _hubProxy.On(messageName, message =>
            {
                var decoded = DataConverter.Decode<Tmessage>(message);
                handler(decoded);
            });
        }

        private void SetHeartbeatHandler(Action handler)
        {
            _hubProxy.On("heartbeat", handler);
        }

        private void SetAuthExpiringHandler(Action handler)
        {
            _hubProxy.On("authenticationExpiring", handler);
        }

        private string CreateSignature(string apiSecret, string data)
        {
            var hmacSha512 = new HMACSHA512(Encoding.ASCII.GetBytes(apiSecret));
            var hash = hmacSha512.ComputeHash(Encoding.ASCII.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private async Task<List<SocketResponse>> Subscribe(string[] channels)
        {
            return await _hubProxy.Invoke<List<SocketResponse>>("Subscribe", (object)channels);
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
        }

        private class SocketResponse
        {
            public bool Success { get; set; }
            public string ErrorCode { get; set; }
        }


        private static class DataConverter
        {
            private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                FloatParseHandling = FloatParseHandling.Decimal,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
            }
            };

            public static T Decode<T>(string wireData)
            {
                // Step 1: Base64 decode the wire data into a gzip blob
                byte[] gzipData = Convert.FromBase64String(wireData);

                // Step 2: Decompress gzip blob into JSON
                string json = null;

                using (var decompressedStream = new MemoryStream())
                using (var compressedStream = new MemoryStream(gzipData))
                using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressedStream);
                    decompressedStream.Position = 0;
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        json = streamReader.ReadToEnd();
                    }
                }

                // Step 3: Deserialize the JSON string into a strongly-typed object
                return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
            }
        }
    }
}
