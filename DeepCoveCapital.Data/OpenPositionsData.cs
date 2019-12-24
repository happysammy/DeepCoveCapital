using System.Linq;
using System.Collections.Generic;
using DeepCoveCapital.Core;

namespace DeepCoveCapital.Data
{
    public class OpenPositionsData : InMemoryDataBase
    {
        private List<Position> _openPositions = new List<Position>();
        public List<Position> GetOpenPositions()
        {
            return _openPositions;
        }
        public List<Position> GetOpenPositions(Instrument instrument)
        {
            return _openPositions.Where(a => a.Symbol == instrument.Symbol && a.Exchange == instrument.Exchange).ToList();
        }
        public void AddOrUpdatePosition(Position position)
        {
            if (_openPositions.Where(a => a.Symbol == position.Symbol && a.Exchange == position.Exchange).Any())
            {
                int location = _openPositions.Where(a => a.Symbol == position.Symbol && a.Exchange == position.Exchange).GetHashCode();
                if (position.Quantity == 0)
                {
                    _openPositions.RemoveAt(location);
                }
                else
                {
                    _openPositions[location] = position;
                }
                RaisePropertyChanged(nameof(OpenPositions));
            }
            else
            {
                _openPositions.Add(position);
                RaisePropertyChanged(nameof(OpenPositions));
            }
        }
        public List<Position> OpenPositions
        {
            get
            {
                return _openPositions;
            }
        }
    }
}
