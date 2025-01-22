using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;
using StreamingService.Services.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace StreamingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VirusTotalController : ControllerBase
    {
        private readonly VirusTotalService _virusTotalService;

        public VirusTotalController(VirusTotalService virusTotalService)
        {
            _virusTotalService = virusTotalService;
        }

        [SwaggerOperation(Summary = "Free tier API", Description = "For uploading files smaller than 32MB")]
        [HttpPost("scan")]
        public async Task<IActionResult> ScanVideoFile([FromForm] ScanFileRequest req)
        {
            try
            {
                var scanResult = await _virusTotalService.ScanFileWithVirusTotalAsync(req.video);

                return Ok(scanResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [SwaggerOperation(Summary = "Premium API.", Description = "For uploading files smaller than 600MB")]
        [HttpPost("scan-v2")]
        public async Task<IActionResult> ScanVideoFileV2([FromForm] ScanFileRequest req)
        {
            try
            {
                var scanResult = await _virusTotalService.ScanFileWithVirusTotalAsyncV2(req.video);

                return Ok(scanResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
