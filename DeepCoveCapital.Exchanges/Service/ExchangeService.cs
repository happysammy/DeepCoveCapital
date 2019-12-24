using DeepCoveCapital.Core;
using DeepCoveCapital.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeepCoveCapital.Exchanges.Service
{
    public class ExchangeService : ServiceBase
    {

        OrderFactory _orderFactory;
        IOrderRepository _orderRepository;

        public BrokerageService(IExchange exchangeClient) : base()
        {
            _client = exchangeClient;

            BrokerPosition = new Position(this);

            _orderFactory = new OrderFactory();

            _orderRepository = _mefLoader.OrderRepository;

            Mediator.Register(this);
        }

        #region Methods

        private void OpenPosition(OpenPositionData openPositionData)
        {

            try
            {
                BrokerPosition = new Position(this);
                BrokerPosition.Direction = openPositionData.Direction;

                //Create opening order
                Order openingOrder = _orderFactory.CreateOpeningOrder(openPositionData.Direction, KrakenOrderType.stop_loss, openPositionData.EnteringPrice, openPositionData.Volume, openPositionData.CandleStickId, openPositionData.ConfirmationId, validateOnly: openPositionData.ValidateOnly);
                UpdateOpeningOrder(openingOrder);
                //Place opening order and wait until closed or canceled
                Log(LogEntryImportance.Info, "Placing opening order...", true);
                PlaceOrderResult openingOrderResult = _client.PlaceOrder(openingOrder, true);
                openingOrder = openingOrderResult.Order;
                UpdateOpeningOrder(openingOrder);
                bool ok = false;

                #region Handle opening-order result

                switch (openingOrderResult.ResultType)
                {
                    case PlaceOrderResultType.error:
                        _client.HandleErrors(openingOrderResult.Errors);
                        break;
                    case PlaceOrderResultType.success:
                        Log(LogEntryImportance.Info, "Opening order filled", true);
                        UpdateEmergencyOrder(_orderFactory.CreateEmergencyExitOrder(openingOrder));
                        ok = true;
                        break;
                    case PlaceOrderResultType.partial:
                        Log(LogEntryImportance.Info, string.Format("Opening order partially filled: {0}/{1}", openingOrder.VolumeExecuted, openingOrder.Volume), true);
                        ok = true;
                        break;
                    case PlaceOrderResultType.txid_null:
                        Log(LogEntryImportance.Error, "An error occured while attempting to place an opening order: txid is null (unknown reason. Maybe the ValidateOnly argument was set to true)", true);
                        break;
                    case PlaceOrderResultType.canceled_not_partial:
                        Log(LogEntryImportance.Info, string.Format("The opening order was canceled: {0}", openingOrder.Reason), true);
                        break;
                    case PlaceOrderResultType.exception:
                        Log(LogEntryImportance.Error, string.Format("An error occured while attempting to place an opening order: {0}", openingOrderResult.Exception.Message), true);
                        break;
                    default:
                        Log(LogEntryImportance.Error, string.Format("Unknown PlaceOrderResultType {0}", openingOrderResult.ResultType), true);
                        break;
                }

                #endregion


                if (!ok) return;

                //if nothing went wrong, place exiting order
                Order closingOrder = _orderFactory.CreateStopLossOrder(openingOrder, openPositionData.ExitingPrice, openPositionData.ValidateOnly);
                UpdateClosingOrder(closingOrder);
                //Place closing order and wait until closed or canceled
                Log(LogEntryImportance.Info, "Placing closing order...", true);
                PlaceOrderResult closingOrderResult = _client.PlaceOrder(closingOrder, true);
                closingOrder = closingOrderResult.Order;
                UpdateClosingOrder(closingOrder);

                #region Handle closing-order result

                switch (closingOrderResult.ResultType)
                {
                    case PlaceOrderResultType.error:
                        _client.HandleErrors(closingOrderResult.Errors);
                        break;
                    case PlaceOrderResultType.success:
                        Log(LogEntryImportance.Info, "Closing order filled", true);
                        UpdateEmergencyOrder(null);
                        break;
                    case PlaceOrderResultType.partial:
                        Log(LogEntryImportance.Info, string.Format("Closing order partially filled: {0}/{1}", closingOrder.VolumeExecuted, closingOrder.Volume), true);
                        break;
                    case PlaceOrderResultType.txid_null:
                        Log(LogEntryImportance.Error, "An error occured while attempting to place a closing order: txid is null (unknown reason. Maybe the ValidateOnly argument was set to true)", true);
                        break;
                    case PlaceOrderResultType.canceled_not_partial:
                        Log(LogEntryImportance.Info, string.Format("The closing order was canceled: {0}", closingOrder.Reason), true);
                        break;
                    case PlaceOrderResultType.exception:
                        Log(LogEntryImportance.Error, string.Format("An error occured while attempting to place a closing order: {0}", closingOrderResult.Exception.Message), true);
                        break;
                    default:
                        Log(LogEntryImportance.Error, string.Format("Unknown PlaceOrderResultType {0}", closingOrderResult.ResultType), true);
                        break;
                }

                #endregion
            }
            catch (Exception ex)
            {
                Log(LogEntryImportance.Error, string.Format("An exception occured in OpenPosition at line {0}. {1} {2}", ex.LineNumber(), ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : "")), true);
            }

        }

        private bool CancelOpeningOrder()
        {
            bool ok = false;

            try
            {
                if (BrokerPosition == null)
                {
                    Log(LogEntryImportance.Info, "Position is null. No opening order to cancel", true);
                    ok = true;
                }
                else
                {


                    Order currentOpeningOrder = BrokerPosition.OpeningOrder;

                    if (currentOpeningOrder != null && (currentOpeningOrder.Status == "open" || currentOpeningOrder.Status == "pending" || currentOpeningOrder.Status == "not yet submitted"))
                    {

                        Log(LogEntryImportance.Info, "Canceling current opening order...", true);

                        bool keepSpinning = true;
                        while (keepSpinning)
                        {
                            CancelOrderResult cancelOrderResult = _client.CancelOrder(currentOpeningOrder);
                            switch (cancelOrderResult.ResultType)
                            {
                                case CancelOrderResultType.error:
                                    Log(LogEntryImportance.Error, string.Format("An error occured while trying to cancel the opening order.\nError List: {0}", String.Join(",\n", cancelOrderResult.Errors)), true);
                                    //if the error is handled, keep spinning
                                    _client.HandleErrors(cancelOrderResult.Errors);
                                    break;
                                case CancelOrderResultType.success:
                                    Log(LogEntryImportance.Info, "Succesfully canceled opening order", true);
                                    UpdateOpeningOrder(cancelOrderResult.Order);
                                    UpdateEmergencyOrder(null);
                                    keepSpinning = false;
                                    ok = true;
                                    break;
                                case CancelOrderResultType.exception:
                                    Log(LogEntryImportance.Error, string.Format("An error occured while attempting to cancel the current opening order: {0}", cancelOrderResult.Exception.Message), true);
                                    return ok;

                            }
                        }

                    }
                    else
                    {
                        Log(LogEntryImportance.Info, "No opening order to cancel.", true);
                        ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogEntryImportance.Error, string.Format("An unhandled exception occured in CancelOpeningOrder: {0} {1}", ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : "")), true);
            }

            return ok;
        }

        private bool CancelClosingOrder()
        {
            bool ok = false;

            try
            {
                if (BrokerPosition == null)
                {
                    Log(LogEntryImportance.Info, "Position is null. No closing order to cancel", true);
                    ok = true;
                }
                else
                {
                    Order currentClosingOrder = BrokerPosition.ClosingOrder;

                    //currentClosingOrder != null && (currentClosingOrder.Status == "open" || currentClosingOrder.Status == "pending")
                    if (currentClosingOrder != null && (currentClosingOrder.Status == "open" || currentClosingOrder.Status == "pending" || currentClosingOrder.Status == "not yet submitted"))
                    {

                        Log(LogEntryImportance.Info, "Canceling current closing order...", true);

                        bool keepSpinning = true;
                        while (keepSpinning)
                        {
                            CancelOrderResult cancelOrderResult = _client.CancelOrder(currentClosingOrder);
                            switch (cancelOrderResult.ResultType)
                            {
                                case CancelOrderResultType.error:
                                    Log(LogEntryImportance.Error, string.Format("An error occured while trying to cancel the current closing order.Error List: {0}", String.Join(",", cancelOrderResult.Errors)), true);
                                    //if the error is handled, keep spinning
                                    _client.HandleErrors(cancelOrderResult.Errors);
                                    break;
                                case CancelOrderResultType.success:
                                    Log(LogEntryImportance.Info, "Succesfully canceled closing order", true);
                                    UpdateClosingOrder(cancelOrderResult.Order);
                                    keepSpinning = false;
                                    ok = true;
                                    break;
                                case CancelOrderResultType.exception:
                                    Log(LogEntryImportance.Error, string.Format("An error occured while attempting to cancel the current closing order: {0}", cancelOrderResult.Exception.Message), true);
                                    return ok;

                            }
                        }

                    }
                    else
                    {
                        Log(LogEntryImportance.Info, "No closing order to cancel.", true);
                        ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogEntryImportance.Error, string.Format("An unhandled exception occured in CancelOpeningOrder: {0} {1}", ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : "")), true);
            }
            return ok;
        }

        private void ShiftPositionLimits(ShiftPositionLimitsData shiftPositionLimitsData)
        {

            Log(LogEntryImportance.Info, "In ShiftPositionLimits", true);

            try
            {
                //Cancel current stoploss order
                bool cancelClosingOrderRes = CancelClosingOrder();

                if (cancelClosingOrderRes)
                {
                    //create new stop loss order
                    Order newStopLossOrder = _orderFactory.CreateStopLossOrder(BrokerPosition.OpeningOrder, shiftPositionLimitsData.NewLimitPrice, shiftPositionLimitsData.ValidateOnly);
                    UpdateClosingOrder(newStopLossOrder);
                    //place order and wait
                    Log(LogEntryImportance.Info, "Placing new closing order...", true);
                    PlaceOrderResult placeOrderResult = _client.PlaceOrder(newStopLossOrder, true);
                    newStopLossOrder = placeOrderResult.Order;
                    UpdateClosingOrder(newStopLossOrder);

                    #region Handle place-order result

                    switch (placeOrderResult.ResultType)
                    {
                        case PlaceOrderResultType.error:
                            _client.HandleErrors(placeOrderResult.Errors);
                            break;
                        case PlaceOrderResultType.success:
                            Log(LogEntryImportance.Info, "Closing order filled", true);
                            UpdateEmergencyOrder(null);
                            break;
                        case PlaceOrderResultType.partial:
                            Log(LogEntryImportance.Info, string.Format("Closing order partially filled: {0}/{1}", newStopLossOrder.VolumeExecuted, newStopLossOrder.Volume), true);
                            break;
                        case PlaceOrderResultType.txid_null:
                            Log(LogEntryImportance.Error, "An error occured while attempting to place a closing order: txid is null (unknown reason. Maybe the ValidateOnly argument was set to true)", true);
                            break;
                        case PlaceOrderResultType.canceled_not_partial:
                            Log(LogEntryImportance.Info, string.Format("The closing order was canceled: {0}", newStopLossOrder.Reason), true);
                            break;
                        case PlaceOrderResultType.exception:
                            Log(LogEntryImportance.Error, string.Format("An error occured while attempting to place a closing order: {0}", placeOrderResult.Exception.Message), true);
                            break;
                        default:
                            Log(LogEntryImportance.Error, string.Format("Unknown PlaceOrderResultType {0}", placeOrderResult.ResultType), true);
                            break;
                    }

                    #endregion
                }
                else
                {
                    Log(LogEntryImportance.Error, "Unable to cancel current closing order. Cannot shift limits", true);
                }
            }
            catch (Exception ex)
            {
                Log(LogEntryImportance.Error, string.Format("An exception occured in ShiftPositionLimits at line {0}. {1} {2}", ex.LineNumber(), ex.Message, ((ex.InnerException != null) ? ex.InnerException.Message : "")), true);
            }

        }

        private void UpdateOpeningOrder(Order order)
        {
            //save the order
            var newOrder = _orderRepository.Save(order);

            //if the current order is more recent then the one we are trying to refresh with, do not update Position.
            //The instruction is coming from another thread that was still waiting for an order to be closed.
            if (BrokerPosition != null && BrokerPosition.OpeningOrder != null && BrokerPosition.OpeningOrder.CreateDate > order.CreateDate)
            {
                return;
            }
            else
            {
                BrokerPosition.OpeningOrder = newOrder;
            }
        }

        private void UpdateClosingOrder(Order order)
        {
            //save the order
            var newOrder = _orderRepository.Save(order);

            //if the current order is more recent then the one we are trying to refresh with, do not update Position.
            //The instruction is coming from another thread that was still waiting for an order to be closed.
            if (BrokerPosition != null && BrokerPosition.ClosingOrder != null && BrokerPosition.ClosingOrder.CreateDate > order.CreateDate)
            {
                return;
            }
            else
            {
                BrokerPosition.ClosingOrder = newOrder;
            }
        }

        private void UpdateEmergencyOrder(Order order)
        {

            var newOrder = order;

            //save the order to get ID
            if (order != null)
                newOrder = _orderRepository.Save(order);

            BrokerPosition.EmergencyExitOrder = newOrder;

        }

        #endregion

        #region Message Handlers
        [MediatorMessageSink(MediatorMessages.OpenPosition, ParameterType = typeof(OpenPositionData))]
        public void OpenPositionReceived(OpenPositionData openPositionData)
        {
            Task task = new Task(() =>
            {
                Log(LogEntryImportance.Info, "OpenPosition message received", true);
                if (openPositionData != null)
                {

                    //Cancel opening order
                    bool cancelOpeningOrderRes = CancelOpeningOrder();
                    if (!cancelOpeningOrderRes)
                    {
                        Log(LogEntryImportance.Error, "Unable to cancel current opening order. Cannot open new Position", true);
                    }
                    else
                    {
                        Log(LogEntryImportance.Info, "Opening Position...", true);
                        OpenPosition(openPositionData);
                    }
                }
                else
                {
                    Log(LogEntryImportance.Info, "OpenPositionData is null. Cannot open new Position...", true);
                }
            });
            task.Start();
        }

        [MediatorMessageSink(MediatorMessages.ShiftPositionLimits, ParameterType = typeof(ShiftPositionLimitsData))]
        public void ShiftPositionLimitsReceived(ShiftPositionLimitsData shiftPositionLimitsData)
        {
            Task task = new Task(() =>
            {
                Log(LogEntryImportance.Info, "Shifting Position limits...", true);
                ShiftPositionLimits(shiftPositionLimitsData);
            });
            task.Start();
        }

        [MediatorMessageSink(MediatorMessages.ClosePosition, ParameterType = typeof(string))]
        public void ClosePositionReceived(string message)
        {
            Task task = new Task(() =>
            {
                Log(LogEntryImportance.Info, "Closing Position...", true);

                //Cancel opening order
                bool cancelOpeningOrderRes = CancelOpeningOrder();

                //cancel closing order
                bool cancelClosingOrder = CancelClosingOrder();

                //execute emergency exit order
                if (BrokerPosition != null && BrokerPosition.EmergencyExitOrder != null)
                {
                    Order emergencyExitOrder = BrokerPosition.EmergencyExitOrder;

                    PlaceOrderResult emergencyExitOrderResult = _client.PlaceOrder(emergencyExitOrder, true);

                    emergencyExitOrder = emergencyExitOrderResult.Order;

                    //update PositionView and PanelView
                    UpdateEmergencyOrder(emergencyExitOrder);

                    #region Handle emergency order result

                    switch (emergencyExitOrderResult.ResultType)
                    {
                        case PlaceOrderResultType.error:
                            _client.HandleErrors(emergencyExitOrderResult.Errors);
                            break;
                        case PlaceOrderResultType.success:
                            Log(LogEntryImportance.Info, "Emergency exit order filled", true);
                            break;
                        case PlaceOrderResultType.partial:
                            Log(LogEntryImportance.Info, string.Format("Emergency exit order partially filled: {0}/{1}", emergencyExitOrder.VolumeExecuted, emergencyExitOrder.Volume), true);
                            break;
                        case PlaceOrderResultType.txid_null:
                            Log(LogEntryImportance.Error, "An error occured while attempting to place the emergency exit order: txid is null (unknown reason. Maybe the ValidateOnly argument was set to true)", true);
                            break;
                        case PlaceOrderResultType.canceled_not_partial:
                            Log(LogEntryImportance.Info, string.Format("The emergency exiting order was canceled: {0}", emergencyExitOrder.Reason), true);
                            break;
                        case PlaceOrderResultType.exception:
                            Log(LogEntryImportance.Error, string.Format("An error occured while attempting to place the emergency exit order: {0}", emergencyExitOrderResult.Exception.Message), true);
                            break;
                        default:
                            Log(LogEntryImportance.Error, string.Format("Unknown PlaceOrderResultType {0}", emergencyExitOrderResult.ResultType), true);
                            break;
                    }

                    #endregion

                }
                else
                {
                    Log(LogEntryImportance.Info, "No emergency order to execute.", true);
                }


                Log(LogEntryImportance.Info, "Position closed.", true);

                BrokerPosition = null;
            });
            task.Start();
        }
        #endregion

        #region Properties

        Position position;
        Position BrokerPosition
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                if (value == null)
                {
                    Mediator.NotifyColleagues<string>(MediatorMessages.BrokerPositionNull, "Position null");
                }

            }
        }

        IExchangeClient _client;
        public IExchangeClient Client
        {
            get
            {
                return _client;
            }
            set
            {
                _client = value;
            }

        }

        #endregion

        private class Position
        {

            BrokerageService _brokerageService;

            public Position(BrokerageService brokerageService)
            {
                _brokerageService = brokerageService;
            }

            public OrderType Direction { get; set; }

            Order openingOrder;
            public Order OpeningOrder
            {
                get
                {
                    return openingOrder;
                }
                set
                {
                    openingOrder = value;

                    if (value != null)
                        _brokerageService.Mediator.NotifyColleagues<Order>(MediatorMessages.UpdateOrder, openingOrder);

                    UpdateOngoingContracts(openingOrder);

                }
            }

            Order closingOrder;
            public Order ClosingOrder
            {
                get
                {
                    return closingOrder;
                }
                set
                {
                    closingOrder = value;
                    if (value != null)
                        _brokerageService.Mediator.NotifyColleagues<Order>(MediatorMessages.UpdateOrder, closingOrder);

                    UpdateOngoingContracts(closingOrder);
                }
            }

            Order emergencyExitOrder;
            public Order EmergencyExitOrder
            {
                get
                {
                    return emergencyExitOrder;
                }
                set
                {
                    emergencyExitOrder = value;

                    _brokerageService.Mediator.NotifyColleagues<Order>(MediatorMessages.UpdateEmergencyOrder, emergencyExitOrder);

                    UpdateOngoingContracts(emergencyExitOrder);
                }
            }

            private void UpdateOngoingContracts(Order order)
            {
                if (order != null && order.VolumeExecuted.HasValue)
                {

                    int sign = 0;
                    switch (order.Type)
                    {
                        case "buy":
                            //positive
                            sign = 1;
                            break;
                        case "sell":
                            //negative
                            sign = -1;
                            break;
                    }
                    decimal ongoingContractsIncrement = sign * order.VolumeExecuted.Value;

                    _brokerageService.Mediator.NotifyColleagues<decimal>(MediatorMessages.UpdateOngoingContracts, ongoingContractsIncrement);
                }
            }
        }

        #region Dispose

        ~BrokerageService()
        {
            // In case the client forgets to call
            // Dispose , destructor will be invoked for
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                BrokerPosition = null;
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
