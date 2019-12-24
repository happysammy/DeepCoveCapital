using DeepCoveCapital.Exchanges.Model;
using DeepCoveCapital.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeepCoveCapital.Exchanges
{
    interface IExchange
    {//TODO
        #region Connections
        void InitializeAPI();
        List<Instrument> InitializeInstruments();
        void InitializeWebsocket();
        void GetAPIValidity();
        #endregion


        #region Methods
        IList<Order> GetLiveOrders();
        Task<IList<Order>> GetLiveOrdersAsync();
        IList<Position> GetLivePositions();
        Task<IList<Position>> GetLivePositionsAsync();

        CancelOrderResult CancelOrder(string symbol);
        PlaceOrderResult PlaceOrder(string symbol, OrderDirection direction, OrderType type, decimal quantity, string orderID, decimal price, TimeInForce timeInForce);
        EditOrderPriceResult EditOrderPrice(string symbol, string orderID, decimal price);
        EditOrderQuantityResult EditOrderQuantity(string symbol, string orderID, decimal quantity);
        PlacePostOnlyCloseOrderResult PlacePostOnlyCloseOrder(string symbol, OrderDirection direction, decimal price, decimal quantity, string note);
        PlaceCloseOrderResult PlaceCloseOrder(string symbol, OrderDirection direction, decimal price, decimal quantity, string note);

        IList<CandleStick> GetAllCandleHistories();
        Task<IList<CandleStick>> GetAllCandleHistoriesAsync();
        IList<CandleStick> GetIndividualCandleHistory(Instrument instrument);
        Task<IList<CandleStick>> GetIndividualCandleHistoryAsync(Instrument instrument);
        IList<CandleStick> UpdateAllCandleIndicators();
        Task<IList<CandleStick>> UpdateAllCandleIndicatorsAsync();
        IList<CandleStick> UpdateIndividualCandleIndicators(Instrument instrument);
        Task<IList<CandleStick>> UpdateIndividualCandleIndicatorsAsync(Instrument instrument);

        #endregion


        #region ErrorProcessing
        void HandleErrors();
        void HandleExceptions();
        void TradeWait();
        #endregion
    }
    
}
