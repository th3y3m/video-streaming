using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;
using StreamingService.Services.Models;

namespace StreamingService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YoutubeController : ControllerBase
    {
        private readonly YoutubeService _youtubeService;

        public YoutubeController(YoutubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideoAsync([FromForm] UploadVideoYoutubeRequest r)
        {
            var filePath = Path.GetTempFileName();
            try
            {
                // Save the file temporarily on disk
                using (var stream = System.IO.File.Create(filePath))
                {
                    await r.video.CopyToAsync(stream);
                }
                // Use YouTube service to upload the file
                await _youtubeService.UploadVideoAsync(filePath, r.title, r.description);
                return Ok(new { message = "Video uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
