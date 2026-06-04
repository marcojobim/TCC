using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using Tcc.Api.Models;
using Tcc.Api.Services;

[MemoryDiagnoser]
public class TelemetryBenchmark
{
    private ClassicTelemetryService _classic;
    private OptimizedTelemetryService _optimized;
    private static byte[] _fileBytes;

    [GlobalSetup]
    public void Setup()
    {
        string filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../telemetry.json"));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {Path.GetFullPath(filePath)}");
        }

        _fileBytes = File.ReadAllBytes(filePath);

        _classic = new ClassicTelemetryService();
        _optimized = new OptimizedTelemetryService();
    }

    [Benchmark(Baseline = true)]
    public async Task Classic()
    {
        using var stream = new MemoryStream(_fileBytes);
        await _classic.Process(stream);
    }

    [Benchmark]
    public async Task Optimized()
    {
        using var stream = new MemoryStream(_fileBytes);
        await _optimized.Process(stream);
    }
}