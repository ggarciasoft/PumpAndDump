using System;
using System.Collections.Generic;
using System.Text;

namespace PumpAndDump
{
    public class BinanceMarketInformation
    {
        [System.Text.Json.Serialization.JsonPropertyName("s")]
        public string Symbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("b")]
        public string BidPrice { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("B")]
        public string BidQuantity { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("a")]
        public string AskPrice { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("A")]
        public string AskQuantity { get; set; }
    }
}
