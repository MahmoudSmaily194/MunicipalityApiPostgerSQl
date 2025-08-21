using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SawirahMunicipalityWeb.Services.ImageService
{
    public class SupabaseImageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _serviceRoleKey;
        private readonly string _publicPrefix;

        public SupabaseImageService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _baseUrl = config.GetValue<string>("Supabase:Url")?.TrimEnd('/')
                       ?? throw new Exception("Supabase:Url not found in configuration.");

            _serviceRoleKey = config.GetValue<string>("Supabase:ServiceRoleKey")
                              ?? throw new Exception("Supabase:ServiceRoleKey not found in configuration.");

            _httpClient = httpClientFactory.CreateClient("SupabaseStorageClient");
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);

            _publicPrefix = $"{_baseUrl}/storage/v1/object/public";
        }

        /// <summary>
        /// رفع الصورة وإرجاع رابط public
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile file, string bucket = "sawirah-images", bool upsert = true)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            var url = $"storage/v1/object/{bucket}/{fileName}";
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };

            if (upsert)
                request.Headers.Add("x-upsert", "true");

            var resp = await _httpClient.SendAsync(request);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"Supabase storage upload failed. Status: {resp.StatusCode}. Body: {body}");
            }

            // رابط public للصورة
            var publicUrl = $"{_publicPrefix}/{bucket}/{Uri.EscapeDataString(fileName)}";
            return publicUrl;
        }
    }
}
