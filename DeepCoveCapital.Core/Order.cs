using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Core
{
    public class Order
    {
        public Order(string symbol, ExchangeName exchange, OrderDirection direction, decimal quantity, decimal price, OrderType type, TimeInForce timeInForce)
        {
            this.Symbol = symbol;
            this.Exchange = exchange;
            this.Direction = direction;
            this.Quantity = quantity;
            this.Price = price;
            this.Type = type;
            this.TimeInForce = timeInForce;
        }
        public int OrderID;
        public string Symbol;
        public ExchangeName Exchange;
        public OrderDirection Direction;
        public decimal Quantity;
        public decimal Price;
        public OrderType Type;
        public TimeInForce TimeInForce;
        public decimal? StopPrice = null;
    }

    public enum OrderDirection
    {
        Buy,
        Sell,
        Hold
    }

    public enum OrderType
    {
        Limit,
        Market,
        StopLimit,
        StopMarket
    }

    public enum TimeInForce
    {
        GoodTillClose,
        EndOfDay
    }
}
