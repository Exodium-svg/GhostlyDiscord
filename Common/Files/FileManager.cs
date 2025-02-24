using System.Net;
using System.Net.Http.Headers;

namespace Common.Files
{
    public class FileManager
    {
        private static readonly HttpClient _httpClient = new HttpClient(); // Reuse client
        private readonly string _webUrl;

        public FileManager(string webUrl) => _webUrl = webUrl.TrimEnd('/');

        public async Task<WebFile?> GetFileAsync(string guildId, string filename)
        {
            string path = GetPath(guildId, filename);

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(path);

                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Console.WriteLine($"Tried to get file {path} but does not exist, database out of sync?");
                            return null;
                        case HttpStatusCode.GatewayTimeout:
                        case HttpStatusCode.RequestTimeout:
                            Console.WriteLine($"Request timed out for content {path}");
                            return null;
                        default:
                            Console.WriteLine($"Unhandled error code in file retrieval: {response.StatusCode}");
                            return null;
                    }
                }

                byte[] data = await response.Content.ReadAsByteArrayAsync();
                return new WebFile(filename, path, data);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
                return null;
            }
        }

        public async Task UploadFileAsync(string guildId, string filename, byte[] data)
        {
            string path = GetPath(guildId, filename);

            using ByteArrayContent content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PutAsync(path, content);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to upload file {path}: {ex.Message}");
                return;
            }

            if (response.StatusCode != HttpStatusCode.Created)
            {
                Console.WriteLine($"Failed to create file {path}, status: {response.StatusCode}.");
            }
        }

        private string GetPath(string guildId, string filename) => $"{_webUrl}/{guildId}/{filename}";
    }
}
