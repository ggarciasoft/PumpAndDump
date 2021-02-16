using System;
using System.Collections.Generic;
using System.Text;

namespace PumpAndDump
{
    public class CompareMarketInformation
    {
        public string Symbol { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal CheckingValue { get; set; }
        public DateTime CheckingTime { get; set; }
        public decimal Difference
        {
            get
            {
                var max = Math.Max(CurrentValue, CheckingValue);
                var min = Math.Min(CurrentValue, CheckingValue);
                return max - min;
            }
        }
        public decimal DifferencePercentage
        {
            get
            {
                var min = Math.Min(CurrentValue, CheckingValue);
                return Math.Round(min == 0 ? 0.00m : Difference / min, 2);
            }
        }
    }
}
