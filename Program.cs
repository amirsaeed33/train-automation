namespace train_automation
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains("--smoke-test"))
            {
                RunSmokeTestAsync().GetAwaiter().GetResult();
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static async Task RunSmokeTestAsync()
        {
            await using var scraper = new EtrainScraperService();
            var progress = new Progress<string>(Console.WriteLine);
            var results = await scraper.SearchTrainsAsync(new TrainSearchSettings(), progress);
            Console.WriteLine($"Total trains: {results.Count}");
            foreach (var train in results.Take(5))
            {
                Console.WriteLine($"{train.TrainNumber} | {train.TrainName} | {train.Departure}-{train.Arrival} | {train.Availability}");
            }
        }
    }
}