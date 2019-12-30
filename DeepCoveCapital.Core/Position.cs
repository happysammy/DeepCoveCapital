using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Core
{
    public class Position
    {
        public Position(string symbol, ExchangeName exchange, decimal quantity)
        {
            Symbol = symbol;
            Exchange = exchange;
            Quantity = quantity;
        }
        public Position(string symbol, ExchangeName exchange, decimal quantity, decimal averageEntryPrice)
        {
            Symbol = symbol;
            Exchange = exchange;
            Quantity = quantity;
            AverageEntryPrice = averageEntryPrice;
        }
        public string Symbol;
        public ExchangeName Exchange;
        public decimal? AverageEntryPrice;
        public decimal Quantity;
    }
}
