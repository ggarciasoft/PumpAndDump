using System;
using System.Collections.Generic;
using System.Text;

namespace PumpAndDump
{
    public class BittrexSymbolInformation
    {
        [System.Text.Json.Serialization.JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("baseCurrencySymbol")]
        public string BaseCurrencySymbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("quoteCurrencySymbol")]
        public string QuoteCurrencySymbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isMarginTradingAllowed")]
        public bool IsMarginTradingAllowed { get; set; }
    }
}
