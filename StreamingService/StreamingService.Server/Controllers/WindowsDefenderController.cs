using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;

namespace StreamingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WindowsDefenderController : ControllerBase
    {
        private readonly WindowsDefenderService _windowsDefenderService;

        public WindowsDefenderController(WindowsDefenderService windowsDefenderService)
        {
            _windowsDefenderService = windowsDefenderService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFileV2(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file.");
            }

            string scanResult = await _windowsDefenderService.ScanFileWithWindowsDefenderReturnMessage(file);
            return Ok(new { ScanResult = scanResult });
        }
    }
}
