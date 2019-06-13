using System;
using Serilog;
using Serilog.Core;

namespace Logging
{
    public class UniqueLogger : IDisposable
    {
        public string Id { get; }
        public ILogger Logger => _logger;
        private readonly Logger _logger;
        public LoggingLevelSwitch Switch { get; }

        public UniqueLogger(string id, Logger logger, LoggingLevelSwitch logSwitch)
        {
            Id = id;
            Switch = logSwitch;
            _logger = logger;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() => Dispose(true);

        #endregion
    }
}