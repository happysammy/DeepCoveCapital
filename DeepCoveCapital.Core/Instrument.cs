using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Core
{
    public class Instrument
    {
        public Instrument(string symbol, decimal tickSize, ExchangeName exchange, bool active = false)
        {
            Symbol = symbol;
            Exchange = exchange;
            TickSize = tickSize;
            Active = active;
        }
        public string Symbol { get; }
        public ExchangeName Exchange { get; }
        public decimal TickSize { get; }
        public decimal DecimalPlacesInTickSize
        {
            get
            {//TODO edit for decimal places
                return TickSize;
            }
        }
        public bool Active { get; set; }
    }
}
