using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace StreamingService.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            _cloudinary = new Cloudinary(new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
                )
            );
        }

        public async Task<string> UploadVideoAsync(string filePath)
        {
            try
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(filePath),
                    PublicId = "my_uploaded_video",
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading video: {ex.Message}");
            }
        }

        public async Task DeleteVideoAsync(string publicId)
        {
            try
            {
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Video
                };

                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Result == "ok")
                {
                    Console.WriteLine("Video deleted successfully!");
                }
                else
                {
                    Console.WriteLine($"Failed to delete video. Status: {deletionResult.Result}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting video: {ex.Message}");
            }
        }
    }
}
