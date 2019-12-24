using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCoveCapital.Infrastructure
{
    public static class MediatorMessages
    {
        public const string AccountCapitalizationChanged = "AccountCapitalizationChanged";
        public const string TimeIntervalTypeChanged = "TimeIntervalTypeChanded";
        public const string TimeIntervalValueChanged = "TimeIntervalValueChanged";
        public const string WmaPeriodChanged = "WmaPeriodChanged";
        public const string NNIntervalChanged = "NNIntervalChanged";
        public const string LeverageChanged = "LeverageChanged";
        public const string PositionOpeningCostChanged = "PositionOpeningCostChanged";
        public const string EnableOrdersChanged = "EnableOrdersChanged";
        public const string CatchUpWithConnector = "CatchUpWithConnector";
        public const string DoneCatchingUpWithConnector = "DoneCatchingUpWithConnector";
        public const string StartQuoteBot = "StartQuoteBot";
        public const string StopQuoteBot = "StopQuoteBot";
        public const string StopStrategyService = "StopStrategyService";

        public const string InitializeGraphViewModel = "InitializeGraphViewModel";
        public const string SetGraphDataSource = "SetGraphDataSource";

        public const string NewPriceData = "NewPriceData";
        public const string LogMessage = "LogMessage";
        public const string UpdateHighBound = "UpdateHighBound";
        public const string UpdateLowBound = "UpdateLowBound";
        public const string Reset = "Reset";

        public const string NewCandleStick = "NewCandleStick";
        public const string NewSetup = "NewSetup";
        public const string NewConfirmation = "NewConfirmation";


        public const string OpenPosition = "OpenPosition";
        public const string ShiftPositionLimits = "ShiftPositionLimits";
        public const string ClosePosition = "ClosePosition";

        public const string UpdateOngoingContracts = "UpdateOngoingContracts";
        public const string UpdateOrder = "UpdateOrder";
        public const string UpdateEmergencyOrder = "UpdateEmergencyOrder";
        public const string BrokerPositionNull = "BrokerPositionNull";

    }
}
