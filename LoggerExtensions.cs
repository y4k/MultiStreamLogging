using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Logging
{
    public static class LoggerExtensions
    {
        public static LoggerConfiguration WithIdentity(this LoggerEnrichmentConfiguration config, string identity) => config.With(IdentityEnricher.Create(identity));

        public static LoggerConfiguration WithIdentity(this LoggerConfiguration loggerConfig, string identity) => loggerConfig.Enrich.WithProperty("Identity", identity);

        public static LogEventLevel Disable(this LoggingLevelSwitch levelSwitch) => levelSwitch.MinimumLevel = LogEventLevel.Fatal + 1;
    }
}