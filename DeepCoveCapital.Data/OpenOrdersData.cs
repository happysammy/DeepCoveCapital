using System.Linq;
using System.Collections.Generic;
using DeepCoveCapital.Core;

namespace DeepCoveCapital.Data
{
    public class OpenOrdersData : InMemoryDataBase
    {
        private List<Order> _openOrders = new List<Order>();
        public List<Order> GetOpenOrders()
        {
            return _openOrders;
        }
        public List<Order> GetOpenOrders(Instrument instrument)
        {
            return _openOrders.Where(a => a.Symbol == instrument.Symbol && a.Exchange == instrument.Exchange).ToList();
        }
        public void AddOrUpdateOrder(Order Order)
        {
            if (_openOrders.Where(a => a.Symbol == Order.Symbol && a.Exchange == Order.Exchange).Any())
            {
                int location = _openOrders.Where(a => a.Symbol == Order.Symbol && a.Exchange == Order.Exchange).GetHashCode();
                if (Order.Quantity == 0)
                {
                    _openOrders.RemoveAt(location);
                }
                else
                {
                    _openOrders[location] = Order;
                }
                RaisePropertyChanged(nameof(OpenOrders));
            }
            else
            {
                _openOrders.Add(Order);
                RaisePropertyChanged(nameof(OpenOrders));
            }
        }
        public List<Order> OpenOrders
        {
            get
            {
                return _openOrders;
            }
        }
    }
}
