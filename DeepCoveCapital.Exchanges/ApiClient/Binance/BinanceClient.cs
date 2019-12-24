using System;
using Binance.Net.Objects;
using binance = Binance.Net;
using System.Collections.Generic;
using CryptoExchange.Net.Authentication;
using System.Composition;
using System.Configuration;
using DeepCoveCapital.Exchanges;
using DeepCoveCapital.Core;
using core = DeepCoveCapital.Core;
using System.Linq;
using System.Threading.Tasks;

namespace DeepCoveCapital.Exchanges.Binance
{
    [Export(typeof(IExchange))]
    class BinanceClient : ExchangeClientBase, IExchange
    {
        private static string _key;
        private static string _secret;
        private static string _listenKey;
        private static binance.BinanceClient _binanceClient;

        #region Connections
        public BinanceClient() : base(ExchangeName.Binance)
        {
            _key = ConfigurationManager.AppSettings["BinanceKey"];
            _secret = ConfigurationManager.AppSettings["BinanceSecret"];
        }
        public void InitializeAPI()
        {
            binance.BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(_key, _secret)
            });

            binance.BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(_key, _secret),
            });

            _binanceClient = new binance.BinanceClient();

            var bin = _binanceClient.StartUserStream();
            if (bin.Success)
            {
                _listenKey = bin.Data;
            }
        }

        public List<Instrument> InitializeInstruments()
        {
            var bin = _binanceClient.GetExchangeInfo();
            if (bin.Success)
            {
                List<Instrument> ActiveInstruments = new List<Instrument>();
                var BinanceInstruments = bin.Data.Symbols.Where(x => x.Name.Contains("BNBUSDT")).OrderBy(a => a.Name).ToList();
                for (byte num = 0; num < BinanceInstruments.Count(); num++)
                {
                    if (ActiveInstruments.Where(x => x.Symbol == BinanceInstruments[num].Name).Count() == 0)
                    {
                        Instrument ins = new Instrument(BinanceInstruments[num].Name, Convert.ToDecimal((double)(BinanceInstruments[num].LotSizeFilter.StepSize)), Exchange.Binance, Active: chkBinance.IsChecked);
                        ActiveInstruments.Add(ins);
                        var t = _binanceClient.GetMyTrades(BinanceInstruments[num].Name);
                        if (t.Success)
                        {
                            List<BinanceTrade> myt = t.Data.OrderByDescending(a => a.Time).ToList();
                            decimal Tot = 0, p_tot = 0;
                            for (int n = 0; n < myt.Count(); n++)
                            {
                                if (myt[n].IsBuyer)
                                {
                                    Tot += myt[n].Quantity;
                                    p_tot += myt[n].Price * myt[n].Quantity;
                                }
                                else { break; }
                            }
                            SymbolData[ins].EntryPrice = Tot == 0 ? 0 : p_tot / Tot;
                        }
                    }
                }
                return ActiveInstruments;
            }
            else
            {
                throw new Exception("BinanceClient.InitializeInstruments() : Unable to initialize Instruments");
            }
        }

        #endregion

        #region Methods
        public CancelOrderResult CancelOrder(string symbol)
        {
            throw new NotImplementedException();
        }

        public EditOrderPriceResult EditOrderPrice(string symbol, string orderID, decimal price)
        {
            throw new NotImplementedException();
        }

        public EditOrderQuantityResult EditOrderQuantity(string symbol, string orderID, decimal quantity)
        {
            throw new NotImplementedException();
        }

        public IList<CandleStick> GetAllCandleHistories()
        {
            throw new NotImplementedException();
        }

        public Task<IList<CandleStick>> GetAllCandleHistoriesAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
        public void GetAPIValidity()
        {
            throw new NotImplementedException();
        }

        public IList<CandleStick> GetIndividualCandleHistory(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CandleStick>> GetIndividualCandleHistoryAsync(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public IList<Order> GetLiveOrders()
        {
            throw new NotImplementedException();
        }

        public Task<IList<Order>> GetLiveOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public IList<Position> GetLivePositions()
        {
            throw new NotImplementedException();
        }

        public Task<IList<Position>> GetLivePositionsAsync()
        {
            throw new NotImplementedException();
        }

        public void HandleErrors()
        {
            throw new NotImplementedException();
        }

        public void HandleExceptions()
        {
            throw new NotImplementedException();
        }


        public void InitializeWebsocket()
        {
            throw new NotImplementedException();
        }

        public PlaceCloseOrderResult PlaceCloseOrder(string symbol, OrderDirection direction, decimal price, decimal quantity, string note)
        {
            throw new NotImplementedException();
        }

        public PlaceOrderResult PlaceOrder(string symbol, OrderSide side, core.OrderType type, decimal quantity, string orderID, decimal price, TimeInForce timeInForce)
        {
            throw new NotImplementedException();
        }

        public PlacePostOnlyCloseOrderResult PlacePostOnlyCloseOrder(string symbol, OrderDirection direction, decimal price, decimal quantity, string note)
        {
            throw new NotImplementedException();
        }

        public void TradeWait()
        {
            throw new NotImplementedException();
        }

        public IList<CandleStick> UpdateAllCandleIndicators()
        {
            throw new NotImplementedException();
        }

        public Task<IList<CandleStick>> UpdateAllCandleIndicatorsAsync()
        {
            throw new NotImplementedException();
        }

        public IList<CandleStick> UpdateIndividualCandleIndicators(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CandleStick>> UpdateIndividualCandleIndicatorsAsync(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        }
    }
}
