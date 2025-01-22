using Microsoft.AspNetCore.Http;

namespace StreamingService.Services.Models
{
    public class UploadVideoVimeoRequest
    {
        public IFormFile video { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class UploadVideoCloudinaryRequest
    {
        public IFormFile video { get; set; }
    }

    public class UploadVideoYoutubeRequest
    {
        public IFormFile video { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class ScanFileRequest
    {
        public required IFormFile video { get; set; }
    }
}
