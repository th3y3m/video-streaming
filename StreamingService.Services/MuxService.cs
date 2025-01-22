using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StreamingService.Services
{
    public class MuxService
    {
        private readonly HttpClient _httpClient;

        public MuxService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // Retrieve the Mux credentials from configuration
            var accessToken = configuration["Mux:AccessToken"];
            var secretKey = configuration["Mux:Secret"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("Mux credentials are missing in configuration.");
            }

            // Combine the Access Token and Secret Key for Basic Authentication
            var byteArray = Encoding.ASCII.GetBytes($"{accessToken}:{secretKey}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
        public async Task<string> CreateUploadUrlAsync()
        {
            var requestBody = new
            {
                new_asset_settings = new
                {
                    playback_policy = new[] { "public" }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.mux.com/video/v1/uploads", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create upload URL: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            return jsonResponse?.RootElement.GetProperty("data").GetProperty("url").GetString();
        }
        public async Task<string> GetVideoPlaybackUrlAsync(string videoId)
        {
            // Fetch the video asset from Mux using the asset ID
            var response = await _httpClient.GetAsync($"https://api.mux.com/video/v1/assets/{videoId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get video playback URL: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(responseContent);

            // Return the playback URL
            return jsonResponse?.RootElement
                .GetProperty("data")
                .GetProperty("playback_ids")[0]
                .GetProperty("id")
                .GetString();
        }
        public async Task<string> UploadAndGetPlaybackUrlAsync(string filePath)
        {
            // Step 1: Create upload URL
            var uploadUrl = await CreateUploadUrlAsync();

            // Step 2: Upload video
            string videoId = await UploadVideoAsync(uploadUrl, filePath);

            // Step 3: Get the playback URL
            string playbackUrl = await GetVideoPlaybackUrlAsync(videoId);

            return playbackUrl;
        }

        public async Task<string> UploadVideoAsync(string uploadUrl, string filePath)
        {
            var videoFileContent = new MultipartFormDataContent();
            var videoFile = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
            videoFile.Headers.Add("Content-Type", "video/mp4");
            videoFileContent.Add(videoFile, "file", "video.mp4");

            var response = await _httpClient.PostAsync(uploadUrl, videoFileContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload video: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            return jsonResponse?.RootElement.GetProperty("data").GetProperty("id").GetString();
        }
    }
}
