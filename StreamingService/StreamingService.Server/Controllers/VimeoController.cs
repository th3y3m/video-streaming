using Microsoft.AspNetCore.Mvc;
using VimeoDemo.Services;

namespace StreamingService.Server.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public class VimeoController : ControllerBase
    {
        private readonly VimeoService _vimeoService;

        public VimeoController(VimeoService vimeoService)
        {
            _vimeoService = vimeoService;
        }
        
        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideoAsync([FromForm] req r)
        {
            try
            {
                // Save the file temporarily on disk
                var filePath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath))
                {
                    await r.video.CopyToAsync(stream);
                }

                // Use Vimeo service to upload the file
                var url = await _vimeoService.UploadVideoAsync(filePath);

                // Return the video URI (e.g., "/videos/123456789")
                return Ok(new { message = "Video uploaded successfully!", url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public class req
        {
            public IFormFile video { get; set; }
            public string title { get; set; }
            public string description { get; set; }
        }
    }
}
