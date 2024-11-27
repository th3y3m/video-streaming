using Microsoft.AspNetCore.Mvc;
using StreamingService.Services;
using StreamingService.Services.Models;

namespace StreamingService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudinaryController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public CloudinaryController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideoAsyncUsingCloudinary([FromForm] UploadVideoCloudinaryRequest r)
        {
            var filePath = Path.GetTempFileName();
            try
            {
                // Save the file temporarily on disk
                using (var stream = System.IO.File.Create(filePath))
                {
                    await r.video.CopyToAsync(stream);
                }

                var url = await _cloudinaryService.UploadVideoAsync(filePath);

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
        public async Task<IActionResult> DeleteVideoAsync(string id)
        {
            try
            {
                await _cloudinaryService.DeleteVideoAsync(id);
                return Ok(new { message = "Video is deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
