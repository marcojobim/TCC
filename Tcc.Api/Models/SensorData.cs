namespace Tcc.Api.Models
{
    public class LocationData
    {
        public string? Sector { get; init; }
        public string? Rack { get; init; }
    }
    public class SensorData
    {
        public Guid SensorId { get; init; }
        public DateTime Timestamp { get; init; }
        public string? MetricType { get; init; }
        public double Value { get; init; }
        public string? Status { get; init; }
        public List<LocationData>? Location { get; init; }
        public List<string>? Tags { get; init; }
        public string? Description { get; init; }
    }
}