using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tcc.Api.Services;
using Tcc.Api.Models;
using Tcc.Api.Interfaces;

namespace Tcc.Api.Controllers
{
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        [HttpPost("classic")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Classic([FromKeyedServices("classic")] ITelemetryProcessor service)
        {
            var response = await service.Process(Request.Body);
            return Ok(response);
        }

        [HttpPost("optimized")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Optimized([FromKeyedServices("optimized")] ITelemetryProcessor service)
        {
            var response = await service.Process(Request.Body);
            return Ok(response);
        }
    }
}