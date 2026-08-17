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

        ApplicationConfiguration.Initialize();
        Application.Run(new MainShellForm());
    }

    private static Task RunSmokeTestAsync()
    {
        Console.WriteLine("Smoke test requires captcha in UI. Run the WinForms app and click Find.");
        return Task.CompletedTask;
    }
}
