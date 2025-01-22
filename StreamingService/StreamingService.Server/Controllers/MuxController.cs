using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;

namespace StreamingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MuxController : ControllerBase
    {
        private readonly MuxService _muxService;

        public MuxController(MuxService muxService)
        {
            _muxService = muxService;
        }

        // Endpoint to get the streaming URL for a video
        [HttpGet("playback-url/{videoId}")]
        public async Task<IActionResult> GetPlaybackUrl(string videoId)
        {
            try
            {
                var playbackUrl = await _muxService.UploadAndGetPlaybackUrlAsync(videoId);
                return Ok(new { playbackUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
