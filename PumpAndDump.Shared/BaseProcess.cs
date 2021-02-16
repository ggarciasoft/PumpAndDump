using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PumpAndDump.Shared
{
    public abstract class BaseProcess : IProcess
    {
        protected string Endpoint { get; set; }
        protected string SelectedSymbol { get; set; }
        protected int ThresholdCompareSecond { get; set; } = 10;
        protected decimal ThresholdComparePercent { get; set; } = 0.1m;
        protected decimal ThresholdAskPercent { get; set; } = 0.4m;
        protected Action<string> ShowLogAction;
        public ConcurrentDictionary<string, CompareMarketInformation> CompareMarketDic { get; set; } = new ConcurrentDictionary<string, CompareMarketInformation>();

        protected T ConvertToMarketInformation<T>(string text) where T : class?
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(text);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public abstract Task EndProcess();

        public abstract Task LoadProcess(Action<string> showLogAction);

        public abstract Task<bool> StartProcess();

        public void ProcessMessage<T>(string message)
        {
            var marketInformation = ConvertToMarketInformation<BinanceMarketInformation>(message);
            if (marketInformation != null && CompareMarketDic.TryGetValue(marketInformation.Symbol, out var compareMarketInformation))
            {
                compareMarketInformation.CurrentValue = decimal.Parse(marketInformation.AskPrice);
                var currentDate = DateTime.Now;
                var differenceSecond = (currentDate - compareMarketInformation.CheckingTime).Seconds;
                if (compareMarketInformation.CheckingValue == default)
                {
                    compareMarketInformation.CheckingValue = decimal.Parse(marketInformation.AskPrice);
                    return;
                }
                else if (
                differenceSecond <= ThresholdCompareSecond &&
                compareMarketInformation.CurrentValue > compareMarketInformation.CheckingValue &&
                compareMarketInformation.DifferencePercentage >= ThresholdComparePercent)
                {
                    ShowLogAction($"FOUND: symbol: {marketInformation.Symbol} current date: {currentDate.ToLongDateString()} checking price: {compareMarketInformation.CheckingValue} current price: {compareMarketInformation.CurrentValue}");
                }
                else if (differenceSecond > ThresholdCompareSecond)
                {
                    compareMarketInformation.CheckingValue = decimal.Parse(marketInformation.AskPrice);
                    compareMarketInformation.CheckingTime = currentDate;
                }
            }
        }
    }
}
