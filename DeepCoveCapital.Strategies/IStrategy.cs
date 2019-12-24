using DeepCoveCapital.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Strategies
{
    interface IStrategy
    {
        #region Methods
        public IndicatorUpdateEvent IndicatorUpdate();

        public Setup DetermineSetup();
        #endregion

        #region Message Processing

        [MediatorMessageSink(MediatorMessages.TimeIntervalTypeChanged, ParameterType = typeof(TimeIntervals))]
        public void SetTimeIntervalType(TimeIntervals timeIntervalType);

        [MediatorMessageSink(MediatorMessages.TimeIntervalValueChanged, ParameterType = typeof(int))]
        public void SetTimeIntervalValue(int timeIntervalValue);
        
        [MediatorMessageSink(MediatorMessages.EnableOrdersChanged, ParameterType = typeof(bool))]
        public void SetEnableOrders(bool enableOrders)
        {
            EnableOrders = enableOrders;
        }

        [MediatorMessageSink(MediatorMessages.PositionOpeningCostChanged, ParameterType = typeof(decimal))]
        public void SetPositionOpeningCost(decimal openingCost)
        {
            PositionOpeningCost = openingCost;
        }

        [MediatorMessageSink(MediatorMessages.NewPriceData, ParameterType = typeof(PriceData))]
        public void NewPriceDataReceived(PriceData priceData);

        [MediatorMessageSink(MediatorMessages.Reset, ParameterType = typeof(string))]
        public void ResetMessageReceived(string message);

        [MediatorMessageSink(MediatorMessages.UpdateOngoingContracts, ParameterType = typeof(decimal))]
        public void UpdateOngoingContracts(decimal ongoingContractsIncrement);

        [MediatorMessageSink(MediatorMessages.StopStrategyService, ParameterType = typeof(string))]
        public void StopStrategyService(string message);

        #endregion
    }
}
