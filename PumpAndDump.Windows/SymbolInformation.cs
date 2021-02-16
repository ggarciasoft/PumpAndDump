using System;
using System.Collections.Generic;
using System.Text;

namespace PumpAndDump
{
    public class SymbolInformation
    {
        [System.Text.Json.Serialization.JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("baseAsset")]
        public string BaseAsset { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("quoteAsset")]
        public string QuoteAsset { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isMarginTradingAllowed")]
        public bool IsMarginTradingAllowed { get; set; }
    }
}
