using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;

namespace StreamingService.Services
{
    public class YoutubeService
    {
        private const string AppName = "Edu Trailblaze";
        private static readonly string[] Scopes = { YouTubeService.Scope.YoutubeUpload };
        private readonly string _clientId;
        private readonly string _clientSecret;

        public YoutubeService(IConfiguration configuration)
        {
            _clientId = configuration["Youtube:ClientId"];
            _clientSecret = configuration["Youtube:ClientSecret"];
        }

        public async Task UploadVideoAsync(string videoFilePath, string title, string description, string categoryId = "22", string[] tags = null)
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                },
                Scopes,
                "user",
                CancellationToken.None);

            // Create the YouTubeService instance
            var youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });

            // Prepare video metadata
            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    Tags = tags,
                    CategoryId = categoryId
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = "unlisted"
                }
            };

            // Upload video
            using var fileStream = new FileStream(videoFilePath, FileMode.Open);
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");

            videosInsertRequest.ProgressChanged += UploadProgressChanged;
            videosInsertRequest.ResponseReceived += UploadResponseReceived;

            await videosInsertRequest.UploadAsync();
        }

        private static void UploadProgressChanged(IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine($"Uploading: {progress.BytesSent} bytes sent.");
                    break;
                case UploadStatus.Failed:
                    Console.WriteLine($"Upload failed: {progress.Exception}");
                    break;
            }
        }

        private static void UploadResponseReceived(Video video)
        {
            Console.WriteLine($"Video uploaded successfully. Video ID: {video.Id}");
            Console.WriteLine($"Video URL: https://www.youtube.com/watch?v={video.Id}");
        }
    }
}
