using Serilog;
using System.Diagnostics;

namespace TrademarkOfficer
{
    internal partial class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(Debugger.IsAttached ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();
            IEnumerable<string> trademarkNames = PrepareTrademarks(args, Log.Logger);
            if (trademarkNames == null || !trademarkNames.Any())
            {
                Log.Logger.Error("No trademarks provided. Please provide at least one trademark name as an argument.");
                Log.Logger.Information("Usage: TrademarkOfficer <trademark1> <trademark2> ... <trademarkN>");
                return;
            }
            foreach (string trademarkName in trademarkNames)
            {
                Log.Logger.Information("Processing trademark: {TrademarkName}", trademarkName);
                await TrademarkChecker.CheckTrademarkAsync(trademarkName, Log.Logger);
            }
        }

        private static IEnumerable<string> PrepareTrademarks(string[] args, ILogger logger)
        {
            foreach (string arg in args)
            {
                // if the argument contains something except regular letters, digits, and dashes, skip it
                // The arg must be fully compatible with a domain name rules
                if (!DomainTrademarkRegex().IsMatch(arg))
                {
                    logger.Warning("Invalid trademark name '{TrademarkName}'. It must contain only letters, digits, and dashes.", arg);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(arg))
                {
                    yield return arg.ToLowerInvariant().Trim();
                }
            }
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"^[a-zA-Z0-9-]+$")]
        private static partial System.Text.RegularExpressions.Regex DomainTrademarkRegex();
    }
}
