using Microsoft.Extensions.Configuration;
using VimeoDotNet;
using VimeoDotNet.Net;

namespace StreamingService.Services
{
    public class VimeoService
    {
        private readonly VimeoClient _vimeoClient;

        public VimeoService(IConfiguration configuration)
        {
            _vimeoClient = new VimeoClient(configuration["Vimeo:AccessToken"]);
        }

        public async Task<string> UploadVideoAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found at the specified path.");
            }

            try
            {
                // Convert the file path to IBinaryContent
                var fileContent = new BinaryContent(filePath);

                // Upload the video
                var uploadRequest = await _vimeoClient.UploadEntireFileAsync(fileContent);

                // Get the uploaded video details
                return uploadRequest.ClipUri;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading video to Vimeo:  {ex.Message}");
            }
        }

        public async Task DeleteVideoAsync(long videoId)
        {
            try
            {
                await _vimeoClient.DeleteVideoAsync(videoId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting video from Vimeo: {ex.Message}");
            }
        }
    }
}
