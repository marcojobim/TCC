using BenchmarkDotNet.Running;
public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<TelemetryBenchmark>();
    }
}