using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace StreamingService.Services
{
    public class VirusTotalService
    {
        private readonly string _virusTotalApiKey;
        private readonly long _maxFileSize;
        private readonly string _virusTotalUrlForLargeFile;
        public string _virusTotalUrl;

        public VirusTotalService(IConfiguration configuration)
        {
            _virusTotalApiKey = configuration["VirusTotal:ApiKey"];
            _maxFileSize = long.Parse(configuration["VirusTotal:MaxFileSize"]);
            _virusTotalUrlForLargeFile = configuration["VirusTotal:UrlForLargeFile"];
            _virusTotalUrl = configuration["VirusTotal:BaseUrl"];
        }

        public async Task<string> ScanFileWithVirusTotalAsync(IFormFile video)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Set the Authorization header with your VirusTotal API Key
                    client.DefaultRequestHeaders.Add("x-apikey", _virusTotalApiKey);

                    // Create a multipart form data content to send the video file
                    using (var formData = new MultipartFormDataContent())
                    {
                        // Convert the uploaded video file to a stream
                        using (var fileStream = video.OpenReadStream())
                        {
                            // Add the video file to the form data
                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            formData.Add(fileContent, "file", video.FileName);

                            // Send the file to VirusTotal
                            var response = await client.PostAsync(_virusTotalUrl, formData);

                            if (!response.IsSuccessStatusCode)
                            {
                                return "Error: Unable to scan the file with VirusTotal.";
                            }

                            // Parse the response to get the analysis link
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            dynamic result = JsonConvert.DeserializeObject(jsonResponse);

                            // Retrieve the self link to get the scan result
                            var analysisUrl = result?.data?.links?.self;
                            if (analysisUrl == null)
                            {
                                return "Error: Unable to retrieve the analysis link.";
                            }

                            // Step 2: Use the analysis URL to get the scan result
                            var analysisResponse = await client.GetAsync(analysisUrl.ToString());

                            if (!analysisResponse.IsSuccessStatusCode)
                            {
                                return $"Error fetching analysis. Status: {analysisResponse.StatusCode}";
                            }

                            // Parse the analysis response
                            var analysisJson = await analysisResponse.Content.ReadAsStringAsync();
                            dynamic analysisResult = JsonConvert.DeserializeObject(analysisJson);

                            // Check the scan status
                            var scanStatus = analysisResult?.data?.attributes?.last_analysis_stats;
                            if (scanStatus?.malicious > 0)
                            {
                                return $"The video file is infected! It was flagged by {scanStatus.malicious} antivirus engines.";
                            }

                            return "The video file is clean.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error scanning file: {ex.Message}";
            }
        }

        public async Task<string> ScanFileWithVirusTotalAsyncV2(IFormFile file)
        {
            try
            {
                long fileSize = file.Length;

                using (var client = new HttpClient())
                {
                    // Set the Authorization header with your VirusTotal API Key
                    client.DefaultRequestHeaders.Add("x-apikey", _virusTotalApiKey);

                    // Check if the file size is greater than the maximum allowed size
                    if (fileSize > _maxFileSize)
                    {
                        // Step 1: Get the upload URL for large files
                        var response = await client.GetAsync(_virusTotalUrlForLargeFile);
                        if (!response.IsSuccessStatusCode)
                        {
                            return $"Error: Unable to get the upload URL for large files: {response.ReasonPhrase}";
                        }

                        // Parse the response to get the upload URL
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                        _virusTotalUrl = result?.data;
                        if (_virusTotalUrl == null)
                        {
                            return "Error: Unable to retrieve the upload URL for large files.";
                        }
                    }

                    // Log the upload URL to verify it
                    Console.WriteLine($"Uploading to URL: {_virusTotalUrl}");

                    // Step 2: Upload the file
                    using (var formData = new MultipartFormDataContent())
                    {
                        // Open the file as a stream
                        using (var fileStream = file.OpenReadStream())
                        {
                            // Create the content for the file part
                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                            // Ensure the field name "file" matches what VirusTotal expects
                            formData.Add(fileContent, "file", file.FileName);

                            // Now send the multipart data
                            var uploadResponse = await client.PostAsync(_virusTotalUrl, formData);

                            // Check response for issues
                            if (!uploadResponse.IsSuccessStatusCode)
                            {
                                return $"Error: Unable to scan the file with VirusTotal: {uploadResponse.ReasonPhrase}";
                            }

                            // Handle the response from VirusTotal
                            var jsonResponse = await uploadResponse.Content.ReadAsStringAsync();
                            dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                            var analysisUrl = result?.data?.links?.self;

                            if (analysisUrl == null)
                            {
                                return "Error: Unable to retrieve the analysis link.";
                            }

                            // Fetch the scan result
                            var analysisResponse = await client.GetAsync(analysisUrl.ToString());
                            if (!analysisResponse.IsSuccessStatusCode)
                            {
                                return $"Error fetching analysis. Status: {analysisResponse.StatusCode}";
                            }

                            // Parse the analysis result
                            var analysisJson = await analysisResponse.Content.ReadAsStringAsync();
                            dynamic analysisResult = JsonConvert.DeserializeObject(analysisJson);

                            // Check scan status
                            var scanStatus = analysisResult?.data?.attributes?.last_analysis_stats;
                            if (scanStatus?.malicious > 0)
                            {
                                return $"The file is infected! It was flagged by {scanStatus.malicious} antivirus engines.";
                            }

                            return "The file is clean.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error scanning file: {ex.Message}";
            }
        }
    }
}
