using DeepCoveCapital.Core;

namespace DeepCoveCapital.Exchanges
{
    class ExchangeClientBase
    {
        public ExchangeClientBase(ExchangeName exchangeName)
        {
            this.Name = exchangeName;
        }
        public ExchangeName Name { get; set; }
    }
}
