using NBomber.CSharp;

public class Program
{
    private static byte[] _fileBytes = [];

    public static void Setup()
    {
        string filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../telemetry.json"));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {Path.GetFullPath(filePath)}");
        }

        _fileBytes = File.ReadAllBytes(filePath);
    }

    public static async Task Main()
    {
        Setup();

        var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(60) };

        string urlClassic = "http://localhost:5293/api/telemetry/classic";
        string urlOptimized = "http://localhost:5293/api/telemetry/optimized";

        var scenarioOptimized = Scenario.Create("load_test_optimized", async context =>
        {
            using var jsonContent = new ByteArrayContent(_fileBytes);
            jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(urlOptimized, jsonContent);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(30))
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1)),
            Simulation.Inject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );

        Console.WriteLine("Starting Optimized Route Load Test...");
        NBomberRunner
            .RegisterScenarios(scenarioOptimized)
            .Run();


        var scenarioClassic = Scenario.Create("load_test_classic", async context =>
        {
            using var jsonContent = new ByteArrayContent(_fileBytes);
            jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(urlClassic, jsonContent);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(30))
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1)),
            Simulation.Inject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );

        Console.WriteLine("Starting Classic Route Load Test...");
        NBomberRunner
            .RegisterScenarios(scenarioClassic)
            .Run();
    }
}