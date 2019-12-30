using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Core
{
    public class Order
    {//TODO create open order items - maybe separate into placed order and open order
        public Order(string symbol, ExchangeName exchange, OrderDirection direction, decimal quantity, decimal price, OrderType type, TimeInForce timeInForce)
        {
            Symbol = symbol;
            Exchange = exchange;
            Direction = direction;
            Quantity = quantity;
            Price = price;
            Type = type;
            TimeInForce = timeInForce;
        }

        public Order(string symbol, ExchangeName exchange, OrderDirection direction, decimal quantity, decimal price, OrderType type, TimeInForce timeInForce, decimal stopPrice, string clientID)
        {
            Symbol = symbol;
            Exchange = exchange;
            Direction = direction;
            Quantity = quantity;
            Price = price;
            Type = type;
            TimeInForce = timeInForce;
            StopPrice = stopPrice;
            ClientID = clientID;
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
        public string ClientID = "";
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
