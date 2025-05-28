namespace TrademarkOfficer
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<string> trademarkNames = PrepareTrademarks(args);
            if (trademarkNames == null || !trademarkNames.Any())
            {
                Console.WriteLine("No trademarks provided. Example usage: TrademarkOfficer.exe trademark1 trademark2 ...");
                return;
            }
        }

        private static IEnumerable<string> PrepareTrademarks(string[] args)
        {
            foreach (string arg in args)
            {
                // if the argument contains something except regular letters, digits, and dashes, skip it
                // The arg must be fully compatible with a domain name rules
                if (!DomainTrademarkRegex().IsMatch(arg))
                {
                    throw new ArgumentException($"Invalid trademark name: '{arg}'. Only letters, digits, and dashes are allowed.");
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
