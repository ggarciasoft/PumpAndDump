using System;
using System.Collections.Generic;
using System.Text;

namespace PumpAndDump
{
    public class BinanceExchangeInformation
    {
        [System.Text.Json.Serialization.JsonPropertyName("symbols")]
        public List<SymbolInformation> Symbols1 { get; set; }
    }
}
