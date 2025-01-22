using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;
using StreamingService.Services.Models;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClamAVController : ControllerBase
    {
        private readonly ClamAVService _clamAVService;

        public ClamAVController(ClamAVService clamAVService)
        {
            _clamAVService = clamAVService;
        }

        //docker run -d -p 3310:3310 mkodockx/docker-clamav
        [HttpPost("scan")]
        public async Task<IActionResult> ScanFile([FromForm] ScanFileRequest req)
        {
            try
            {
                var (message, virusDetected) = await _clamAVService.ScanFileWithClamd(req.video);
                return Ok(new { message, virusDetected });
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
