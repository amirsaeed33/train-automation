using System.Drawing;
using System.Drawing.Drawing2D;
using Tesseract;

namespace train_automation;

public static class CaptchaAutoSolver
{
    private const float MinConfidence = 55f;
    private static readonly SemaphoreSlim InitLock = new(1, 1);
    private static string? _tessDataPath;
    private static bool _initFailed;

    public static string? TrySolve(byte[] imageBytes)
    {
        if (_initFailed || imageBytes.Length == 0 || !EnsureTessData())
        {
            return null;
        }

        string? bestText = null;
        var bestConfidence = 0f;

        foreach (var threshold in new[] { 115, 130, 145, 160 })
        {
            foreach (var mode in new[] { PageSegMode.SingleWord, PageSegMode.SingleLine, PageSegMode.RawLine })
            {
                var candidate = TrySolveVariant(imageBytes, threshold, mode, out var confidence);
                if (candidate is null || confidence <= bestConfidence)
                {
                    continue;
                }

                bestText = candidate;
                bestConfidence = confidence;
            }
        }

        return bestConfidence >= MinConfidence ? bestText : null;
    }

    private static string? TrySolveVariant(byte[] imageBytes, int threshold, PageSegMode mode, out float confidence)
    {
        confidence = 0f;

        try
        {
            var processed = PreprocessImage(imageBytes, threshold);
            using var engine = new TesseractEngine(_tessDataPath!, "eng", EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
            engine.DefaultPageSegMode = mode;

            using var pix = Pix.LoadFromMemory(processed);
            using var page = engine.Process(pix);
            confidence = page.GetMeanConfidence();
            var text = new string(page.GetText().Where(char.IsLetterOrDigit).ToArray());
            return text.Length is >= 4 and <= 10 ? text : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool EnsureTessData()
    {
        if (_tessDataPath is not null)
        {
            return true;
        }

        InitLock.Wait();
        try
        {
            if (_tessDataPath is not null)
            {
                return true;
            }

            if (_initFailed)
            {
                return false;
            }

            var tessDataDir = Path.Combine(AppContext.BaseDirectory, "tessdata");
            var trainedDataPath = Path.Combine(tessDataDir, "eng.traineddata");
            if (!File.Exists(trainedDataPath))
            {
                Directory.CreateDirectory(tessDataDir);
                using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
                var data = client
                    .GetByteArrayAsync("https://github.com/tesseract-ocr/tessdata_best/raw/main/eng.traineddata")
                    .GetAwaiter()
                    .GetResult();
                File.WriteAllBytes(trainedDataPath, data);
            }

            _tessDataPath = tessDataDir;
            return true;
        }
        catch
        {
            _initFailed = true;
            return false;
        }
        finally
        {
            InitLock.Release();
        }
    }

    private static byte[] PreprocessImage(byte[] input, int threshold)
    {
        using var inputStream = new MemoryStream(input);
        using var source = new Bitmap(inputStream);
        var scale = Math.Max(4, 160 / Math.Max(1, source.Height));
        using var scaled = new Bitmap(source.Width * scale, source.Height * scale);
        using (var graphics = Graphics.FromImage(scaled))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(source, 0, 0, scaled.Width, scaled.Height);
        }

        for (var y = 0; y < scaled.Height; y++)
        {
            for (var x = 0; x < scaled.Width; x++)
            {
                var color = scaled.GetPixel(x, y);
                var gray = (int)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                scaled.SetPixel(x, y, gray > threshold ? Color.White : Color.Black);
            }
        }

        using var output = new MemoryStream();
        scaled.Save(output, System.Drawing.Imaging.ImageFormat.Png);
        return output.ToArray();
    }
}
