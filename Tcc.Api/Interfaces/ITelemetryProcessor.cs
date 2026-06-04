using Tcc.Api.Models;

namespace Tcc.Api.Interfaces
{
    public interface ITelemetryProcessor
    {
        Task<ProcessingResult> Process(Stream sensorData);
    }
}