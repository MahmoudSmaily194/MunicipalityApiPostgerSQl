// file: Services/ImageService/SupabaseImageService.cs
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
        private readonly string _anonKey;
        private readonly string _publicPrefix; // لبناء رابط عام

        public SupabaseImageService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _baseUrl = config.GetValue<string>("Supabase:Url")?.TrimEnd('/')
                       ?? throw new Exception("Supabase:Url not found in configuration.");
            _anonKey = config.GetValue<string>("Supabase:AnonKey")
                       ?? throw new Exception("Supabase:AnonKey not found in configuration.");

            // أنشئ HttpClient عبر factory
            _httpClient = httpClientFactory.CreateClient("SupabaseStorageClient");
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _anonKey);

            _publicPrefix = $"{_baseUrl}/storage/v1/object/public";
        }

        /// <summary>
        /// يرفع صورة إلى bucket ويرجع رابط public (يعمل إذا كان الـ bucket Public)
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile file, string bucket = "sawirah-images", bool upsert = true)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            // اسم الحقل 'file'، واسم الملف كـ fileName
            content.Add(streamContent, "file", fileName);

            // جهّز الطلب (POST إلى /storage/v1/object/{bucket})
            var request = new HttpRequestMessage(HttpMethod.Post, $"storage/v1/object/{bucket}")
            {
                Content = content
            };

            // خيار للكتابة فوق الملف إذا موجود
            if (upsert)
                request.Headers.Add("x-upsert", "true");

            var resp = await _httpClient.SendAsync(request);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"Supabase storage upload failed. Status: {resp.StatusCode}. Body: {body}");
            }

            // رابط عام للملف (pattern المستخدم في Supabase للـ public objects)
            var publicUrl = $"{_publicPrefix}/{bucket}/{Uri.EscapeDataString(fileName)}";
            return publicUrl;
        }
    }
}
