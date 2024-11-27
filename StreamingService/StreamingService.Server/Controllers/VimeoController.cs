using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;
using StreamingService.Services.Models;

namespace StreamingService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VimeoController : ControllerBase
    {
        private readonly VimeoService _vimeoService;

        public VimeoController(VimeoService vimeoService)
        {
            _vimeoService = vimeoService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideoAsync([FromForm] UploadVideoVimeoRequest r)
        {
            var filePath = Path.GetTempFileName();
            try
            {
                // Save the file temporarily on disk
                using (var stream = System.IO.File.Create(filePath))
                {
                    await r.video.CopyToAsync(stream);
                }

                // Use Vimeo service to upload the file
                var url = await _vimeoService.UploadVideoAsync(filePath, r.title, r.description);

                // Return the video URI (e.g., "/videos/123456789")
                return Ok(new { message = "Video uploaded successfully!", url });
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

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteVideoAsync(long id)
        {
            try
            {
                await _vimeoService.DeleteVideoAsync(id);
                return Ok(new { message = "Video is deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
