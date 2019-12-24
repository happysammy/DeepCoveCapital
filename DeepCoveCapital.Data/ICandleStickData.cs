using DeepCoveCapital.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Data
{
    public interface ICandleStickData
    {
        IEnumerable<CandleStick> GetInstrumentCandleSticks(Instrument);
    }

    public class InMemoryCandleStickData : ICandleStickData
    {
        public IEnumerable<CandleStick> GetInstrumentCandleSticks(Instrument instrument)
        {
            return from c in GetCandleSticks
                   where c.Exchange == instrument.Exchange
                   where c.Symbol == instrument.Symbol
                   select c;
        }
    }
}
