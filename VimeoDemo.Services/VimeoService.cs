using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VimeoDotNet;
using VimeoDotNet.Net;

namespace VimeoDemo.Services
{
    public class VimeoService
    {
        private readonly string _accessToken;

        public VimeoService(IConfiguration configuration)
        {
            _accessToken = configuration["Vimeo:AccessToken"];
        }

        public async Task<string> UploadVideoAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found at the specified path.");
            }

            try
            {
                // Initialize the Vimeo client
                var vimeoClient = new VimeoClient(_accessToken);

                // Convert the file path to IBinaryContent
                var fileContent = new BinaryContent(filePath);

                // Upload the video
                var uploadRequest = await vimeoClient.UploadEntireFileAsync(fileContent);

                // Get the uploaded video details
               return uploadRequest.ClipUri;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading video to Vimeo:  {ex.Message}");
            }
        }
    }
}
