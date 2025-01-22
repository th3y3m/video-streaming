using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;

namespace StreamingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HybridAnalysisController : ControllerBase
    {
        private readonly HybridAnalysisService _hybridAnalysisService;

        public HybridAnalysisController(HybridAnalysisService hybridAnalysisService)
        {
            _hybridAnalysisService = hybridAnalysisService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFile(IFormFile file)
        {
            try
            {
                var result = await _hybridAnalysisService.ScanFileWithHybridAnalysisAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
