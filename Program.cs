using System;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Serilog;
using Serilog.Events;

namespace Logging
{
    class Program
    {
        static void Main ()
        {
            if(Directory.Exists("logs"))
                Directory.Delete("logs", true);

            var config = ConfigurationFactory.FromResource<Program> ("Logging.config.hocon");
            LoggingProvider.Instance.SetMasterLogLevel(LogEventLevel.Information);
            using (var sys = ActorSystem.Create ("system", config))
            {
                for (int i = 0; i < 50; i++)
                {
                    var name = RandomString(10);
                    sys.ActorOf (Props.Create (() => new RandomLoggingActor(name)), name);
                }
                Console.CancelKeyPress += (s, e) => sys.Terminate ();
                sys.WhenTerminated.Wait ();
            }
            Log.CloseAndFlush ();
        }

        private static readonly Random random = new Random ();
        
        public static string RandomString (int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string (Enumerable.Repeat (chars, length)
                .Select (s => s[random.Next (s.Length)]).ToArray ());
        }
    }

    public class RandomLoggingActor : ReceiveActor
    {
        private readonly string _name;
        private readonly UniqueLogger _logger;
        private readonly Random _random;

        public RandomLoggingActor (string name)
        {
            _name = name;
            _logger = LoggingProvider.Instance.GetLogger(_name);
            _logger.Switch.Disable();
            _random = new Random();
            var guid = Guid.NewGuid();

            Context.System.Scheduler.Advanced.ScheduleRepeatedly(
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(1000),
                () => _logger.Logger.Write((LogEventLevel)_random.Next(0, 6), "Message {Guid}", guid));
            // Context.System.Scheduler.Advanced.ScheduleRepeatedly(
            //     TimeSpan.FromSeconds(2),
            //     TimeSpan.FromSeconds(2),
            //     () => 
            //     {
            //         var value = _logger.Switch.MinimumLevel + 1;
            //         if(value > LogEventLevel.Fatal)
            //             value = 0;
            //         _logger.Switch.MinimumLevel = value;
            //     });
        }
    }
}