using System;
using DeepCoveCapital.Core;
using log4net;

namespace DeepCoveCapital.Infrastructure
{
    public class ServiceBase : ObservableObject, IDisposable
    {

        protected ILog _logger;
        protected string _name;
        protected MEFLoader _mefLoader;

        public ServiceBase()
        {
            _name = this.GetType().Name;
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _mefLoader = new MEFLoader();
        }

        protected void Log(LogEntryImportance importance, string message, bool logToConsole = false)
        {
            //log to dynamic log viewer 
            if (logToConsole)
                Mediator.NotifyColleagues<LogEntry>(MediatorMessages.LogMessage, new LogEntry(importance, DateTime.UtcNow, _name, message));

            //log to file
            switch (importance)
            {
                case LogEntryImportance.Info:
                    _logger.Info(message);
                    break;
                case LogEntryImportance.Debug:
                    _logger.Debug(message);
                    break;
                case LogEntryImportance.Error:
                    _logger.Error(message);
                    break;
            }

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
