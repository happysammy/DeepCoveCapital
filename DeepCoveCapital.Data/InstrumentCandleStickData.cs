using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepCoveCapital.Core;

namespace DeepCoveCapital.Data
{
    public class InstrumentCandleStickData : InMemoryDataBase, IJob
    {
        private List<Instrument> _activeInstruments;
        private Dictionary<Instrument, List<CandleStick>> _instrumentCandles = new Dictionary<Instrument, List<CandleStick>>();
        public void AddInstrument(Instrument ins)
        {
            if (!_activeInstruments.Contains(ins))
            {
                _activeInstruments.Add(ins);
                RaisePropertyChanged(nameof(ActiveInstruments));
            }
            else
            {
                //TODO log Instrument already active
            }
        }
        public void RemoveInstrument(Instrument ins)
        {
            if (_activeInstruments.Contains(ins))
            {
                _activeInstruments.Remove(ins);
                RaisePropertyChanged(nameof(ActiveInstruments));
            }
            else
            {
                //TODO log Instrument not active
            }
        }
        public List<Instrument> ActiveInstruments
        {
            get
            {
                return _activeInstruments;
            }
        }

        public void UpdateInstrumentCandles(Instrument instrument, List<CandleStick> candles)
        {
            if (candles != _instrumentCandles[instrument])
            {
                _instrumentCandles[instrument] = candles;
                RaisePropertyChanged(nameof(Candles));
            }
        }
        public Dictionary<Instrument, List<CandleStick>> Candles
        {
            get
            {
                return _instrumentCandles;
            }
        }


        public Task Execute(IJobExecutionContext context)
        {
            foreach (Instrument ins in _activeInstruments)
            {
                _instrumentCandles[ins].Add(new CandleStick());
            }
            return Task.CompletedTask;
        }
    }
}
