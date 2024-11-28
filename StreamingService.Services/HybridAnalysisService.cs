using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace StreamingService.Services
{
    public class HybridAnalysisService
    {
        private readonly string _hybridAnalysisApiKey;
        private readonly string _hybridAnalysisUrl;

        public HybridAnalysisService(IConfiguration configuration)
        {
            _hybridAnalysisApiKey = configuration["HybridAnalysis:ApiKey"];
            _hybridAnalysisUrl = configuration["HybridAnalysis:BaseUrl"];
        }

        public async Task<string> ScanFileWithHybridAnalysisAsync(IFormFile file)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("api-key", _hybridAnalysisApiKey);
                    client.DefaultRequestHeaders.Add("accept", "application/json");

                    using var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");

                    using var formData = new MultipartFormDataContent
                    {
                        { fileContent, "file", file.FileName },
                        { new StringContent("all"), "scan_type" },
                        { new StringContent("string"), "comment" },
                        { new StringContent("string"), "submit_name" }
                    };

                    var response = await client.PostAsync(_hybridAnalysisUrl, formData);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }

                    throw new Exception($"File scanning failed. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error scanning file with Hybrid Analysis: {ex.Message}");
            }
        }
    }
}
