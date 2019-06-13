using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Logging
{
    public sealed class LoggingProvider
    {
        public static LoggingProvider Instance => _instance ?? (_instance = new LoggingProvider ());
        private LoggingProvider ()
        {
            Log.Logger = _defaultLogger = new LoggerConfiguration ()
                .MinimumLevel.ControlledBy (_masterSwitch)
                // .WriteTo.Console (outputTemplate:"[{Timestamp:HH:mm:ss} {Level:u3} {Identity}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File (
                    "logs/main-log_.log",
                    outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Identity} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    fileSizeLimitBytes : 100_000,
                    rollOnFileSizeLimit : true,
                    rollingInterval : RollingInterval.Hour,
                    retainedFileCountLimit : 100,
                    buffered : true,
                    flushToDiskInterval: TimeSpan.FromSeconds(30)
                )
                .CreateLogger ();
        }

        public void SetMasterLogLevel (LogEventLevel level)
        {
            lock (_lock)
            {
                _masterSwitch.MinimumLevel = level;
            }
        }

        public UniqueLogger GetLogger (string uniqueId)
        {
            lock (_lock)
            {
                if (!_loggers.TryGetValue (uniqueId, out var logger))
                {
                    logger = CreateNewLogger (uniqueId);
                    _loggers.Add (uniqueId, logger);
                }
                return logger;
            }
        }

        private UniqueLogger CreateNewLogger (string uniqueId)
        {
            var loggingSwitch = new LoggingLevelSwitch ();
            var logger = new LoggerConfiguration ()
                .WithIdentity (uniqueId)
                .MinimumLevel.Verbose ()
                .WriteTo.Logger (subLogger =>
                    subLogger.MinimumLevel.ControlledBy (loggingSwitch)
                    // .WriteTo.Console (outputTemplate:"[{Timestamp:HH:mm:ss} {Level:u3} {Identity}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File (
                        $"logs/plugin_{uniqueId}_.log",
                        outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Identity} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes : 100_000,
                        rollOnFileSizeLimit : true,
                        rollingInterval : RollingInterval.Hour,
                        retainedFileCountLimit : 50,
                        buffered : true,
                        flushToDiskInterval: TimeSpan.FromSeconds(30)
                    )
                )
                .WriteTo.Logger (_defaultLogger)
                .CreateLogger ();

            return new UniqueLogger (uniqueId, logger, loggingSwitch);
        }

        private void DisposeLoggers ()
        {
            lock (_lock)
            {
                foreach (var logger in _loggers)
                {
                    logger.Value?.Dispose ();
                }
            }
        }

        private readonly Dictionary<string, UniqueLogger> _loggers = new Dictionary<string, UniqueLogger> ();
        private readonly ILogger _defaultLogger;
        private static LoggingProvider _instance;
        private readonly object _lock = new object ();
        private readonly LoggingLevelSwitch _masterSwitch = new LoggingLevelSwitch ();
    }
}