namespace train_automation;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Contains("--smoke-test"))
        {
            RunSmokeTestAsync().GetAwaiter().GetResult();
            return;
        }

        if (args.Contains("ghumo-test"))
        {
            try { GhumoScraperTest.TestAsync("NDLS", "HWH", DateTime.Today.AddDays(5).ToString("yyyy-MM-dd")).GetAwaiter().GetResult(); }
            catch (Exception ex) { System.IO.File.WriteAllText("D:\\github\\train-automation-desktop\\ghumo_test_output.txt", ex.ToString()); }
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainShellForm());
    }

    private static Task RunSmokeTestAsync()
    {
        Console.WriteLine("Smoke test requires captcha in UI. Run the WinForms app and click Find.");
        return Task.CompletedTask;
    }
}
