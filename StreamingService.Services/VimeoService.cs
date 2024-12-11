using Microsoft.Extensions.Configuration;
using VimeoDotNet;
using VimeoDotNet.Models;
using VimeoDotNet.Net;
using Xabe.FFmpeg;

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
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
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

        // Premium Vimeo API
        public async Task<string> UploadVideoAndGetMp4LinkAsync(string filePath, string title, string description)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found at the specified path.");
            }

            try
            {
                // Get media info for debugging or validation
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var duration = mediaInfo.Duration;

                // Convert the file path to IBinaryContent
                var fileContent = new BinaryContent(filePath);

                // Upload the video
                var uploadResponse = await _vimeoClient.UploadEntireFileAsync(fileContent);

                // Get the video ID from the upload response
                var videoId = uploadResponse.ClipId.Value;

                // Fetch the video details using the video ID
                var videoDetails = await _vimeoClient.GetVideoAsync(videoId);

                // Extract the MP4 link from video details
                var mp4Link = videoDetails.Files.FirstOrDefault(f => f.Quality == "hd" && f.Type == "video/mp4")?.Link;

                if (string.IsNullOrEmpty(mp4Link))
                {
                    throw new Exception("MP4 link not found for the uploaded video.");
                }

                return mp4Link;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading video to Vimeo: {ex.Message}");
            }
        }

        public async Task<string> UploadVideoAsync(string filePath, string title, string description)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found at the specified path.");
            }

            try
            {
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var duration = mediaInfo.Duration;

                // Convert the file path to IBinaryContent
                var fileContent = new BinaryContent(filePath);

                // Upload the video
                var uploadRequest = await _vimeoClient.UploadEntireFileAsync(fileContent);

                // Set the title and description for the uploaded video
                var videoUri = uploadRequest.ClipUri;

                // Update the video metadata (title and description)
                var updateRequest = new VideoUpdateMetadata
                {
                    Name = title,
                    Description = description
                };

                // Return the URI of the uploaded video
                if (uploadRequest.ClipId != null)
                {
                    await _vimeoClient.UpdateVideoMetadataAsync(uploadRequest.ClipId.Value, updateRequest);
                }
                return videoUri;
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
