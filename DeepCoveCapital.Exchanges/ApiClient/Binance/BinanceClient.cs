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
        #region Client Settings
        private static string _key;
        private static string _secret;
        private static string _listenKey;
        private static binance.BinanceClient _binanceClient;
        private static binance.BinanceSocketClient _binanceSocketClient;
        #endregion

        #region Trade Settings
        private static KlineInterval _timeframe = KlineInterval.OneHour;
        private static List<Instrument> _activeInstruments = new List<Instrument>();

        #endregion

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
                        //TODO edit if this instrument is active or not
                        Instrument ins = ConvertInstrument(BinanceInstruments[num]);
                        ActiveInstruments.Add(ins);
                    }
                }
                return ActiveInstruments;
            }
            else
            {
                throw new Exception("BinanceClient.InitializeInstruments() : Unable to initialize Instruments");
            }
        }

        public void InitializeWebsocket()
        {
            _binanceSocketClient = new binance.BinanceSocketClient();
            _binanceSocketClient.SubscribeToAllSymbolTickerUpdates((data) =>
            {
                //TODO new tick event
                //TODO other items to register to this event - Trade Execution
                List<string> tickers = ActiveInstruments.Where(a => a.Exchange == ExchangeName.Binance).Select(x => x.Symbol).ToList();
                var BinanceTicks = data.Where(s => tickers.Contains(s.Symbol)).ToList();
                for (byte num = 0; num < BinanceTicks.Count(); num++)
                {
                    if (ins != null)
                    {
                        Prices[ins] = BinanceTicks[num].CurrentDayClosePrice;
                        askQuotes[ins] = BinanceTicks[num].BestAskPrice;
                        bidQuotes[ins] = BinanceTicks[num].BestBidPrice;
                    }
                }
            });
            if (_listenKey == null)
            {
                var bin = _binanceClient.StartUserStream();
                if (bin.Success)
                {
                    _listenKey = bin.Data;
                }
                else
                {
                    HandleErrors(bin.Error);
                }
            }
            _binanceSocketClient.SubscribeToUserDataUpdates(_listenKey,
            (accountInfoUpdate) =>
            {
                //TODO new position event
                var BinanceStreamAccount = accountInfoUpdate;
                var BinanceStreamPositions = accountInfoUpdate.Balances.Where(a => a.Total > 0).ToList();
                List<Position> ballist = new List<Position>();
                for (byte n = 0; n < BinanceStreamPositions.Count(); n++)
                {
                    Position bal = new Position(
                        BinanceStreamPositions[n].Asset,
                        ExchangeName.Binance,
                        BinanceStreamPositions[n].Asset == "BNB" ? Math.Min(0, BinanceStreamPositions[n].Total - 1) : BinanceStreamPositions[n].Total
                        );
                    ballist.Add(bal);
                }
                BinanceLivePositions = ballist.Where(a => a.Total > 0 || a.Asset == "BNB").ToList();
            },
            (orderInfoUpdate) =>
            {
                //TODO new update order event

                var BinanceStreamOrders = orderInfoUpdate;
                Order binanceOrder = ConvertOrder(BinanceStreamOrders);
                if (BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).Any())
                {
                    //UpdateOrderEvent
                    if (BinanceStreamOrders.Status == Binance.Net.Objects.OrderStatus.Canceled || BinanceStreamOrders.Status == Binance.Net.Objects.OrderStatus.Filled)
                    {
                        for (byte n = 0; n < BinanceLiveOrders.Count(); n++)
                        {
                            if (BinanceLiveOrders[n].ClientOrderId == BinanceStreamOrders.OriginalClientOrderId.ToString())
                            {
                                BinanceLiveOrders.RemoveAt(n);
                                if (BinanceStreamOrders.Status == OrderStatus.Filled && ActiveInstruments.Where(a => a.Symbol.Contains(BinanceStreamOrders.Symbol)).Any())
                                {
                                    if (BinanceStreamOrders.Side == OrderSide.Buy)
                                    {
                                        SymbolData[ActiveInstruments.Single(a => a.Symbol.Contains(BinanceStreamOrders.Symbol))].EntryPrice = BinanceStreamOrders.Price;
                                    }
                                    else
                                    {
                                        SymbolData[ActiveInstruments.Single(a => a.Symbol.Contains(BinanceStreamOrders.Symbol))].EntryPrice = 0;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        //NewOrderEvent
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().ClientOrderId = BinanceStreamOrders.ClientOrderId;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().Time = BinanceStreamOrders.Time;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().IcebergQuantity = BinanceStreamOrders.IcebergQuantity;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().StopPrice = BinanceStreamOrders.StopPrice;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().Side = BinanceStreamOrders.Side;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().Type = BinanceStreamOrders.Type;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().TimeInForce = BinanceStreamOrders.TimeInForce;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().UpdateTime = BinanceStreamOrders.EventTime;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().Status = BinanceStreamOrders.Status;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().ExecutedQuantity = BinanceStreamOrders.AccumulatedQuantityOfFilledTrades;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().OriginalQuantity = BinanceStreamOrders.CummulativeQuoteQuantity;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().Price = BinanceStreamOrders.Price;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().Symbol = BinanceStreamOrders.Symbol;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().CummulativeQuoteQuantity = BinanceStreamOrders.Quantity;
                        BinanceLiveOrders.Where(a => a.OrderId == BinanceStreamOrders.OrderId).First().IsWorking = BinanceStreamOrders.IsWorking;
                    }
                }
            },
            (ocoOrderUpdate) =>
            {
            },
            (accountPositionUpdate) =>
            {

            },
            (accountBalanceUpdate) =>
            {

            });
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
        public decimal GetAccountBalanceInUSD()
        {
            try
            {
                decimal WalletBalance_Binance = 0;
                // Binance Balance
                if (_tradeSettings.TestMode == false || _tradeSettings.IsBinanceActive())
                {
                    for (byte n = 0; n < BinanceLivePositions.Count(); n++)
                    {
                        if (BinanceLivePositions[n].Asset.Contains("BTC"))
                        {
                            WalletBalance_Binance += Convert.ToDouble(BinanceLivePositions[n].Total);
                        }
                        else
                        {
                            if (Startup)
                            {
                                if (BinanceLivePositions[n].Asset == "USDT")
                                {
                                    var price = binance.GetPrice("BTCUSDT");
                                    if (price.Success)
                                    {
                                        WalletBalance_Binance += Convert.ToDouble(BinanceLivePositions[n].Total / price.Data.Price);
                                    }
                                }
                                else
                                {
                                    var price = binance.GetPrice(BinanceLivePositions[n].Asset + "BTC");
                                    if (price.Success)
                                    {
                                        WalletBalance_Binance += Convert.ToDouble(BinanceLivePositions[n].Total * price.Data.Price);
                                    }
                                }
                            }
                        }
                    }
                }

                if (WalletBalance_BitMEX >= 0 || _tradeSettings.TestMode || WalletBalance_Futures >= 0)
                {
                    _statusBar.APIValid = true;

                    Instrument ins = ActiveInstruments.First(a => a.Symbol == "XBTUSD" || a.Symbol.Contains("BTCUSD"));

                    return decimal convertedBalance = Prices[ins] * WalletBalance_Binance;
                }
                else
                {
                    //TODO throw ApiInvalidError
                    return 0;
                }
            }
            catch (Exception ex)
            {
                //TODO throw ApiInvalidError
                return 0;
            }
        }

        public IList<CandleStick> GetIndividualCandleHistory(Instrument instrument) => GetIndividualCandleHistoryAsync(instrument).Result;

        public Task<IList<CandleStick>> GetIndividualCandleHistoryAsync(Instrument instrument)
        {
            string s = instrument.Symbol;
            bool active = false;
            if (_tradeSettings.IsBinanceActive())
            {
                active = true;
                List<CandleStick> tempc = new List<CandleStick>();
                var t = await _binanceClient.GetKlinesAsync(s, _timeframe).ConfigureAwait(false);
                if (t.Success)
                {
                    Klines[s] = t.Data.ToList();
                }
                Klines[s] = Klines[s].OrderByDescending(a => a.OpenTime).Take(trendmaPeriod).ToList();
                if (timeframe == "1m")
                {
                    if (Klines[s][0].OpenTime.Minute > DateTime.UtcNow.Minute && Klines[s][0].OpenTime.Hour >= DateTime.UtcNow.Hour && Klines[s][0].OpenTime.DayOfYear >= DateTime.UtcNow.DayOfYear)
                    {
                        Klines[s].RemoveAt(0);
                    }
                }
                else if (timeframe == "1h")
                {
                    if (Klines[s][0].OpenTime.Hour >= DateTime.UtcNow.Hour && Klines[s][0].OpenTime.DayOfYear >= DateTime.UtcNow.DayOfYear)
                    {
                        Klines[s].RemoveAt(0);
                    }
                }

                for (byte n = 0; n < Klines[s].Count(); n++)
                {
                    Candle c = new Candle(
                        Klines[s][n].OpenTime.AddHours(1),
                        (double?)Klines[s][n].Open,
                        (double?)Klines[s][n].Close,
                        (double?)Klines[s][n].High,
                        (double?)Klines[s][n].Low,
                        (double?)Klines[s][n].Volume,
                        Klines[s][n].TradeCount
                        );
                    c.PCC = Klines[s].Where(a => a.OpenTime < c.TimeStamp).Count();
                    tempc.Add(c);
                }
                Candles[instrument] = tempc;
            }
            Console.WriteLine(instrument.Symbol + " : Candle Histories Retreived, GetAllCandleHistoriesAsync");

            if (IndvCandles.ContainsKey(instrument))
            {
                if (IndvCandles[instrument] != null) while (IndvCandles[instrument].IsCompleted == false) { }
            }
            IndvCandles[instrument] = UpdateInstrumentCandlesAsync(instrument);

            return true;
        }

        public IList<CandleStick> UpdateAllCandleIndicators()
        {
            throw new NotImplementedException();
        }

        public async Task<IList<CandleStick>> UpdateAllCandleIndicatorsAsync()
        {
            throw new NotImplementedException();
        }

        public IList<CandleStick> UpdateIndividualCandleIndicators(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<CandleStick>> UpdateIndividualCandleIndicatorsAsync(Instrument instrument)
        {
            throw new NotImplementedException();
        }


        public IList<Order> GetLiveOrders() => GetLiveOrdersAsync.Result;

        public async Task<IList<Order>> GetLiveOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public IList<Position> GetLivePositions() => GetLivePositionsAsync.Result;

        public async Task<IList<Position>> GetLivePositionsAsync()
        {
            throw new NotImplementedException();
        }

        public PlaceCloseOrderResult PlaceCloseOrder(string symbol, OrderDirection direction, decimal price, decimal quantity, string note)
        {
            throw new NotImplementedException();
        }

        public PlaceOrderResult PlaceOrder(string symbol, OrderSide side, core.OrderType type, decimal quantity, string orderID, decimal price, core.TimeInForce timeInForce)
        {
            throw new NotImplementedException();
        }

        public PlacePostOnlyCloseOrderResult PlacePostOnlyCloseOrder(string symbol, OrderDirection direction, decimal price, decimal quantity, string note)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Error Handling
        public void HandleErrors()
        {
            throw new NotImplementedException();
        }

        public void HandleExceptions()
        {
            throw new NotImplementedException();
        }

        public void TradeWait()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Converters
        public static Instrument ConvertInstrument(BinanceSymbol instrument)
        {
            return new Instrument(instrument.Name, 
                Convert.ToDecimal((double)(instrument.LotSizeFilter.StepSize)), 
                ExchangeName.Binance, 
                active: true);
        }

        public static Order ConvertOrder(BinanceOrder order)
        {
            return new Order(
                       order.Symbol,
                       ExchangeName.Binance,
                       ConvertOrderDirection(order.Side),
                       order.OriginalQuantity,
                       order.Price,
                       ConvertOrderType(order.Type),
                       ConvertTimeInForce(order.TimeInForce),
                       order.StopPrice,
                       order.OrderId.ToString());
        }

        public static Order ConvertOrder(BinanceStreamOrderUpdate order)
        {
            return new Order(
                       order.Symbol,
                       ExchangeName.Binance,
                       ConvertOrderDirection(order.Side),
                       order.Quantity,
                       order.Price,
                       ConvertOrderType(order.Type),
                       ConvertTimeInForce(order.TimeInForce),
                       order.StopPrice,
                       order.OrderId.ToString());
        }

        public static Position ConvertPositions()
        {
            throw new NotImplementedException();
        }
        
        public static OrderDirection ConvertOrderDirection(OrderSide direction)
        {
            throw new NotImplementedException();
        }

        public static core.OrderType ConvertOrderType(binance.Objects.OrderType orderType)
        {
            throw new NotImplementedException();
        }

        public static core.TimeInForce ConvertTimeInForce(binance.Objects.TimeInForce timeInForce)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
}
