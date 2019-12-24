using System;

namespace DeepCoveCapital.Core
{
    public class CandleStick
    {
        public string Symbol { get; set; }
        public ExchangeName Exchange { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}
