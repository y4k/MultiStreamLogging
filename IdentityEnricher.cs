using Serilog.Core;
using Serilog.Events;

namespace Logging
{
    public class IdentityEnricher: ILogEventEnricher
    {
        public static IdentityEnricher Create(string identity) => new IdentityEnricher(identity);

        private readonly string _identity;

        public IdentityEnricher(string identity)
        {
            _identity = identity;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Identity", _identity));
        }
    }
}