using Tcc.Api.Models;
using Tcc.Api.Interfaces;
using System.Text.Json;
using Tcc.Api.Controllers;

namespace Tcc.Api.Services
{
    public class ClassicTelemetryService : ITelemetryProcessor
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<ProcessingResult> Process(Stream sensorData)
        {
            int totalItems = 0, checkStatusWarning = 0;
            double checkTemp = 0;
            long checkDateTime = 0;

            var data = await JsonSerializer.DeserializeAsync<List<SensorData>>(sensorData, options);

            if (data is null)
            {
                return new ProcessingResult { };
            }

            foreach (var d in data)
            {
                totalItems++;

                checkTemp += d.Value;

                if (d.Status == "Warning")
                    checkStatusWarning++;

                checkDateTime += d.Timestamp.Ticks;
            }

            return new ProcessingResult { CheckTemp = checkTemp, CheckStatusWarning = checkStatusWarning, CheckDateTime = checkDateTime, TotalItems = totalItems };
        }
    }
}